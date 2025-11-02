namespace AMESA_be.common.Enums
{
    public enum ResultCode
    {
        Success = 200,
        DoneWithErrors = 201,

        //authentication errors
        ChangePassword = 309,
        ChangeSecret = 310,
        Verify2FA = 311,
        UnAuthorized = 401,
        NoPermission = 403,

        GeneralServerError = 450,
        IllegalArgumentException = 460,
        MissingId = 470,
        MissingParameter = 490,
        NoConnectionToUserValidationServer = 501,
        NotImplementedSection = 510,
        ValidationError = 520,
        AuditGeneralError = 540,
        AuditUnimplemented = 550,
        AuditUpdateNoOriginal = 560,
        AuditUnmatchIDs = 570
    }
}
