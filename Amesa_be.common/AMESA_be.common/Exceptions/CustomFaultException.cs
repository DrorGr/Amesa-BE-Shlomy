
namespace AMESA_be.common.Exceptions
{
    public class CustomFaultException : Exception
    {
        public ServiceError StatusCode { get; set; }

        private string _message;
        public override string Message
        {
            get
            {
                return _message;
            }
        }

        /// <summary>
        /// Creates Custom ta9 exception. 
        /// </summary>
        /// <param name="errType">Error type.</param>
        /// <param name="message">Custom exception message.</param>
        public CustomFaultException(ServiceError errType, string message = null) : base(errType.ToString())
        {
            StatusCode = errType;
            _message = message ?? string.Empty;
        }
    }
}
