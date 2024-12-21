using System.Text;
using System.Security.Cryptography;
namespace vendoo.Packages
{
    public class ExtraPackage
    {
            public static string HashPassword(string password)
            {
                using (var sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    StringBuilder builder = new StringBuilder();
                    foreach (byte b in bytes)
                    {
                        builder.Append(b.ToString("x2"));
                    }
                    return builder.ToString();
                }
            }
            public static bool ValidatePassword(string inputPassword, string dbPasswordHash)
            {
                string hashedInputPassword = HashPassword(inputPassword);
                return hashedInputPassword == dbPasswordHash;
            }
        }
}
