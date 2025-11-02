using AMESA_be.Middleware.Attributes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AMESA_be.Middleware.Extensions
{
    // Placeholder for your custom attribute (if not defined elsewhere)
    public class GatewayType : Attribute
    {
        public GatewayTypes[] Arguments { get; set; }
        public GatewayType(params GatewayTypes[] args)
        {
            Arguments = args;
        }
    }

    public enum GatewayTypes
    {
        AdminStudio,
        Intsight,
        Plugin
    }

    // You may have this extension method in your 'common' project
    public static class EnumExtensions
    {
        public static string GetDescription<T>(this T enumValue) where T : Enum
        {
            // Placeholder: A simple .ToString() for demonstration
            return enumValue.ToString();
        }
    }

    public static class JwtAuthenticationExtensions
    {
        public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JWT:Issuer"],
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"] ?? string.Empty))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var wsProtocol = context.Request.Headers["sec-websocket-protocol"];
                        switch (wsProtocol.Count)
                        {
                            case 0:
                                return Task.CompletedTask;
                            case > 1:
                                throw new InvalidOperationException("Multiple protocols found");
                        }
                        Log.Information("{Path} - Websock request", context.Request.Path);
                        var protocols = wsProtocol.ToString().Split(",");
                        if (protocols is not { Length: 2 } || !string.Equals(protocols[0], JwtBearerDefaults.AuthenticationScheme))
                            throw new InvalidOperationException("Protocol not supported");
                        context.Token = protocols[1].Trim();
                        return Task.CompletedTask;
                    }
                };
            });
        }

        public static void AddSwaggerWithJwt(this IServiceCollection services, string serviceName, string? assemblyName)
        {
            var version = Environment.GetEnvironmentVariable("BUILD_VERSION") ?? "Dev";

            services.AddSwaggerGen(swagger =>
            {
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = $"{version}",
                    Title = $"{serviceName}"
                });

                // 1. Define the JWT Bearer security scheme
                swagger.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme()
                {
                    Description = "JWT token authorization header.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                // 2. Define the security requirement that references the scheme
                swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "bearerAuth"
                            }
                        },
                        new List<string>()
                    }
                });

                // Add operation filters
                swagger.OperationFilter<CustomSwaggerPlatformsFilter>();
                swagger.OperationFilter<BasicAuthOperationsFilter>();
                swagger.OperationFilter<PlatformTest>();

                swagger.EnableAnnotations();

                if (!string.IsNullOrEmpty(assemblyName))
                {
                    var xmlFile = $"{assemblyName}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    if (File.Exists(xmlPath))
                    {
                        swagger.IncludeXmlComments(xmlPath);
                    }
                }
            });

            services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                options.EnableForHttps = true;
            });
        }
    }

    public class BasicAuthOperationsFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var noAuthRequired = context.MethodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any();
            if (noAuthRequired) return;
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "bearerAuth"
                            },
                            Scheme = "bearer" // This scheme property is a workaround for some Swagger UI versions.
                        },
                        new List<string>()
                    }
                }
            };
        }
    }

    public class CustomSwaggerPlatformsFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.MethodInfo.GetCustomAttribute<GatewayType>() is GatewayType gatewayAttribute)
            {
                var platforms = gatewayAttribute.Arguments.Select(g => g.GetDescription()).ToList();
                if (operation.Extensions == null) operation.Extensions = new Dictionary<string, IOpenApiExtension>();
                operation.Extensions.Add("x-platforms", new OpenApiString(string.Join(",", platforms)));
            }
        }
    }

    public enum Platforms
    {
        adminStudio,
        intsight,
        plugin
    }

    public class PlatformsAttribute : Attribute
    {
        public Platforms[] Platforms { get; set; }
        public PlatformsAttribute(params Platforms[] platforms)
        {
            Platforms = platforms;
        }
    }

    public class PlatformTest : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attributes = context.MethodInfo.GetCustomAttributes<PlatformsAttribute>(true).FirstOrDefault();
            if (attributes == null) return;
            var finalString = string.Join(",", attributes.Platforms.Select(p => p.ToString()));
            operation.Extensions = new Dictionary<string, IOpenApiExtension>
            {
                {"x-platforms", new OpenApiString(finalString)}
            };
        }
    }
}