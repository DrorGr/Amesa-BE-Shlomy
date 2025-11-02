using Microsoft.Extensions.Hosting; 
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Reflection;
using Serilog.Exceptions;

namespace AMESA_be.Logging.Extensions
{
    public static class AMESASerilogApplicationBuilderExtensions
    {
    /// <summary>
    /// Adds serilog as the logging framework for this Host. 
    /// A configuration file named 'serilog.json' MUST be on project top level folder
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IHostBuilder UseAMESASerilog(
        this IHostBuilder builder)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"{AssemblyDirectory}/serilog.json")
            .Build();
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithEnvironmentUserName()
            .CreateLogger();

        builder.UseAMESASerilog();

        return builder;
    }

    public static string AssemblyDirectory
    {
        get
        {
            string dllpath = Assembly.GetExecutingAssembly().Location;
            string directoryPath = Path.GetDirectoryName(dllpath);
            return directoryPath;
        }
    }
}
}
