using AMESA_be.common.Contracts.Login;
using AMESA_be.common.Enums;
using AMESA_be.common.Enums.Audit;
using AMESA_be.common.IAuditItemProducers;

namespace AMESA_be.common.DTOs.Authentication
{
    public class LoginResponseDto : IDataAuditsProducer<LoginAudit>
    {
        public string RefreshToken { get; set; }
        public string SessionId { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string LoginToken { get; set; }
        public List<EndpointDto> Endpoints { get; set; }
        public string Auth2FactorQRImage { get; set; }
        public ResultCode ResultCode { get; set; }
        public LoginAction LoginAction { get; set; }

        public List<LoginAudit> ToAuditProducer()
        {
            List<LoginAudit> loginAudits = new List<LoginAudit>();
            LoginAudit loginAudit = new LoginAudit
            {
                UserName = UserName,
                Token = LoginToken,
                Status = GetLoginStatusByResponseCode(ResultCode).Item1,
                Reason = GetLoginStatusByResponseCode(ResultCode).Item2,
                SessionId = SessionId,
                CreatedAt = DateTime.UtcNow
            };

            if (LoginAction == LoginAction.Logout && ResultCode == ResultCode.Success)
            {
                loginAudit.Reason = "user logged out success";
            }

            loginAudits.Add(loginAudit);
            return loginAudits;
        }

        private (string, string) GetLoginStatusByResponseCode(ResultCode resultCode)
        {
            //Auditing
            LoginResult auditLoginResult = LoginResult.Failure;
            string reason = "";

            switch (resultCode)
            {
                case ResultCode.Verify2FA:
                    auditLoginResult = LoginResult.TwoFaChallenge;
                    reason = "2fa code to login";
                    break;
                case ResultCode.ChangeSecret:
                    auditLoginResult = LoginResult.TwoFaEnrollment;
                    reason = "2fa enrollment for user";
                    break;
                case ResultCode.Success:
                    auditLoginResult = LoginResult.Success;
                    reason = "user logged in success";
                    break;
                case ResultCode.ChangePassword:
                    auditLoginResult = LoginResult.ChangePassword;
                    reason = "user need to change password";
                    break;
                case ResultCode.NoPermission:
                    auditLoginResult = LoginResult.Failure;
                    reason = "user & password not match";
                    break;
                case ResultCode.NoConnectionToUserValidationServer:
                    auditLoginResult = LoginResult.Failure;
                    reason = "Authentication server unreachable";
                    break;
            }
            return (auditLoginResult.ToString(), reason);
        }
    }
}
