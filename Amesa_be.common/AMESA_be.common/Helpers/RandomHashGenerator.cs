namespace AMESA_be.common.Helpers
{
    public static class RandomHashGenerator
    {
        private static readonly Random _random = new Random();

        public static string GenerateUniqueHash(string input)
        {
            // A simple, unique-enough hash for demonstration. A real-world scenario might use a cryptographic hash with a salt.
            var combinedString = input + DateTime.UtcNow.Ticks + _random.Next();
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(combinedString);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}