using AMESA_be.common.Enums.Audit;

namespace AMESA_be.common.Contracts.Login
{
    public class LoginAuditInfo
    {
        public LoginAction loginAction { get; set; }
        public LoginResult loginResult { get; set; }
        public string userName { get; set; }
        public string token { get; set; }
        public string sessionId { get; set; }
        public string reason { get; set; } = null;
    }
}
