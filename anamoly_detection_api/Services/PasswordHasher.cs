// This code defines a PasswordHasher class that provides a method to hash passwords using the SHA256 algorithm. The HashPassword method takes a plain text password as input, converts it to bytes, computes the hash, and returns the hash as a Base64 string. This is a common approach for securely storing passwords in applications.
using System.Security.Cryptography;
using System.Text;

namespace anamoly_detection_api.Services
{
    public class PasswordHasher
    {
        
        
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
