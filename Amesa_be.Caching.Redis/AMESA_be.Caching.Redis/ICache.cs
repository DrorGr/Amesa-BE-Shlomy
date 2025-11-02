namespace AMESA_be.Caching.Redis
{
    public interface ICache : IDisposable
    {
        /// <summary>
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="isGlobal"></param>
        /// <returns>Returns raw record as string without deserialization.</returns>
        public Task<string?> GetRecordAsync(string cacheKey, bool isGlobal = false);

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="isGlobal"></param>
        /// <returns>Returns deserialize record.</returns>
        public Task<T?> GetRecordAsync<T>(string cacheKey, bool isGlobal = false);

        public Task<T?> GetValueTypeRecordAsync<T>(string cacheKey, bool isGlobal = false) where T : struct;

        public Task SetRecordAsync<T>(string cacheKey, T data,
            TimeSpan? absoluteExpiteTime = null, TimeSpan? unusedExpiteTime = null, bool isGlobal = false);

        public Task RemoveRecordAsync(string cacheKey, bool isGlobal = false);
        public Task<bool> ClearAllCache();
        T? GetRecord<T>(string cacheKey, bool isGlobal = false);
        Task BatchSet<T>(Dictionary<string, T> data, bool isGlobal = false);

        Task<long> RemoveByControllerName(string controllerName);

        public Task<bool> DeleteByRegex(string regex);

    }
}
