namespace AMESA_be.Middleware.Models
{
    public class AMESA9ErrorHttpResult<T>
    {
        /// <summary>
        /// Error code or 200 if the request succeed
        /// </summary>
        public bool success { get; set; }
        /// <summary>
        /// Error message in case of failure
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string message { get; set; }
        /// <summary>
        /// Response data on request success
        /// </summary>
        public T data { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}
