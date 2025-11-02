using System.Text.Json.Serialization;

namespace AMESA_be.common.Contracts.GeneralResponse
{
    public class ApiError
    {
        public string ExceptionMessage { get; set; }
        public string Details { get; set; }
        public string ReferenceErrorCode { get; set; }
        public string ReferenceDocumentLink { get; set; }
        public IEnumerable<ValidationError> ValidationErrors { get; set; }

        [JsonConstructor]
        public ApiError()
        {

        }

        public ApiError(string exceptionMessage)
        {
            ExceptionMessage = exceptionMessage;
        }

        public ApiError(string exceptionMessage, IEnumerable<ValidationError> validationErrors)
        {
            ExceptionMessage = exceptionMessage;
            ValidationErrors = validationErrors;
        }
    }
}