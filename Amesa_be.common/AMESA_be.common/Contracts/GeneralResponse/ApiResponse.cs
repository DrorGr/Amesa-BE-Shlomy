namespace AMESA_be.common.Contracts.GeneralResponse
{
    public class ApiResponse<T>
    {
        public string Version { get; set; } = "1.0.0.0";
        public int Code { get; set; } = 200;
        public string Message { get; set; }
        public bool IsError { get; set; }
        public ApiError ResponseException { get; set; }
        public T Data { get; set; }
    }
}
