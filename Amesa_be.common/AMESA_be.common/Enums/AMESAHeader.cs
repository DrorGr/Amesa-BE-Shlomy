
namespace AMESA_be.common.Enums
{

    public enum AMESAHeaders
    {
        Authorization,
        SessionId,
        Content_Language,
        Query_Context
    }

    public static class TaHeadersExtensions
    {
        public static string GetValue(this AMESAHeaders header)
        {
            return header.ToString().Replace("_", "-");
        }
    }
}
