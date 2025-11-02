using AMESA_be.common;
using Infra.Audit.Filter;
using AMESA_be.Caching.Redis;
using AMESA_be.common.Enums.Audit;
using Infra.Dtos.Authentication;
using Infra.JwtAuthentication;
using Infra.Middleware.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Threading.Tasks;
using TA9.Intsight.AuthenticationService.Dtos;
using TA9.Intsight.AuthenticationService.Services;

namespace AMESA_be.Authentication.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class JWTAuthenticationController(IJWTAuthenticationService authenticationService, IJwtTokenManager jwtTokenManager, ICache cache) : ControllerBase
    {
        private readonly IJWTAuthenticationService _authenticationService = authenticationService;
        private readonly IJwtTokenManager _jwtTokenManager = jwtTokenManager;
        private readonly ICache _cache = cache;

     
        [AllowAnonymous]
        [SendToAudit(AuditAction.Add, ModuleType.Authentication, AuditItemType.LoginAttempt)]
        [HttpPost("login")]
        [Platforms(Platforms.adminStudio, Platforms.plugin, Platforms.intsight)]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginCredentialsDto loginCredentials, [FromQuery] int? languageId = null)
        {
            var result = await _authenticationService.WebLogin(loginCredentials.UserName!, loginCredentials.Pass!, languageId);
            switch (result.code)
            {
                case ResultCode.Success:
                    return Ok(result.loginResponse);
                case ResultCode.ChangePassword:
                    return StatusCode((int)ResultCode.ChangePassword, result.loginResponse);
                case ResultCode.ChangeSecret:
                    return StatusCode((int)ResultCode.ChangeSecret, result.loginResponse);
                case ResultCode.Verify2FA:
                    return StatusCode((int)ResultCode.Verify2FA, result.loginResponse);
                default:
                    return BadRequest(result.loginResponse);
            }
        }

        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("service-to-service-login")]
        public async Task<ActionResult<LoginResponseDto>> ServiceLogin([FromBody] LoginCredentialsDto loginCredentials)
        {
            var result = await _authenticationService.ServiceLogin(loginCredentials.UserName!, loginCredentials.Pass!);
            switch (result.code)
            {
                case ResultCode.Success:
                    return Ok(result.loginResponse);
                case ResultCode.ChangePassword:
                    return StatusCode((int)ResultCode.ChangePassword, result.loginResponse);
                case ResultCode.ChangeSecret:
                    return StatusCode((int)ResultCode.ChangeSecret, result.loginResponse);
                case ResultCode.Verify2FA:
                    return StatusCode((int)ResultCode.Verify2FA, result.loginResponse);
                default:
                    return BadRequest(result.loginResponse);
            }
        }

        [SendToAudit(AuditAction.Add, ModuleType.Authentication, AuditItemType.LogoutAttempt)]
        [HttpPost("logout")]
        public async Task<ActionResult<LoginResponseDto>> Logout()
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
            var res = await _authenticationService.Logout(token);
            return Ok(res);
        }

        [Obsolete("Temporarily unavailable.")]
        [HttpPost("refresh-token")]
        public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto refreshTokenRequestDto)
        {
            try
            {
                var refreshTokenExpTime = await _cache.GetRecordAsync<DateTime>(refreshTokenRequestDto.RefreshToken);
                if (string.IsNullOrWhiteSpace(refreshTokenRequestDto.RefreshToken))
                {
                    return Unauthorized();
                }
                else if (refreshTokenExpTime == default || refreshTokenExpTime > DateTime.Now)
                {
                    return Unauthorized();
                }

                var user = _jwtTokenManager.GetPrincipalFromExpiredToken(
                    HttpContext.Request.Headers.TryGetValue("Authorization", out var token)
                    ? token : string.Empty);
                var userName = user.Identity!.Name;

                var newAccessToken = _jwtTokenManager.GenerateAccessToken(user.Claims, DateTime.Now.AddMinutes(15));
                var newRefreshToken = _jwtTokenManager.GenerateRefreshToken();

                await _cache.RemoveRecordAsync(refreshTokenRequestDto.RefreshToken);
                await _cache.SetRecordAsync(newRefreshToken, DateTime.Now.AddDays(1));

                return Ok(new RefreshTokenResponseDto
                {
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken
                });
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("service-admin-login")]
        public async Task<ActionResult<LoginResponseDto>> ServiceAdminLogin()
        {
            var result = await _authenticationService.ServiceAdminLogin();
            HttpContext.Response.Headers.Add("Authorization", $"Bearer {result.LoginToken}");
            return Ok(result);
        }

        //[AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("touch-session")]
        public async Task<ActionResult<LoginResponseDto>> TouchSession()
        {
            var result = await _authenticationService.TouchSession();
            return Ok(result);
        }

        /// <summary>
        /// argus-api-name: AuthenticationServices/Login2FactorAuth
        /// </summary>
        /// <returns></returns>
        [SendToAudit(AuditAction.Add, ModuleType.Authentication, AuditItemType.LoginAttempt)]
        [HttpPost("2fa-login")]
        public async Task<ActionResult<LoginResponseDto>> Login2FactorAuth([FromBody] string secureId)
        {
            var result = await _authenticationService.Login2FactorAuth(secureId, HttpContext.Request.Headers["Authorization"].ToString());
            switch (result.code)
            {
                case ResultCode.Success:
                    return Ok(result.loginResponse);
                case ResultCode.UnAuthorized:
                    return StatusCode((int)ResultCode.UnAuthorized, result.loginResponse);
                default:
                    return BadRequest(result.loginResponse);
            }
        }

        [AllowAnonymous]
        [SendToAudit(AuditAction.Add, ModuleType.Authentication, AuditItemType.LoginAttempt)]
        [HttpPost("loginJWT")]
        [Platforms(Platforms.adminStudio, Platforms.plugin, Platforms.intsight)]

        public async Task<ActionResult<LoginResponseDto>> LoginJWT([FromBody] string jwtToken)
        {
            var result = await _authenticationService.LoginJWT(jwtToken);
            switch (result.code)
            {
                case ResultCode.Success:
                    return Ok(result.loginResponse);
                default:
                    return BadRequest(result.loginResponse);
            }

        }

        [AllowAnonymous]
        [SendToAudit(AuditAction.Add, ModuleType.Authentication, AuditItemType.LoginAttempt)]
        [HttpPost("AdminLogin")]
        [Platforms(Platforms.adminStudio, Platforms.plugin, Platforms.intsight)]
        public async Task<ActionResult<LoginResponseDto>> AdminLogin([FromBody] LoginCredentialsDto loginCredentials)
        {
            var result = await _authenticationService.AdminLogin(loginCredentials.UserName!, loginCredentials.Pass!);
            switch (result.code)
            {
                case ResultCode.Success:
                    return Ok(result.loginResponse);
                case ResultCode.ChangePassword:
                    return StatusCode((int)ResultCode.ChangePassword, result.loginResponse);
                case ResultCode.ChangeSecret:
                    return StatusCode((int)ResultCode.ChangeSecret, result.loginResponse);
                case ResultCode.Verify2FA:
                    return StatusCode((int)ResultCode.Verify2FA, result.loginResponse);
                default:
                    return BadRequest(result.loginResponse);
            }
        }

    }
