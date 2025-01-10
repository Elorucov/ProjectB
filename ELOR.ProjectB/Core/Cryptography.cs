using Branca;
using System.Security.Cryptography;
using System.Text;

namespace ELOR.ProjectB.Core {
    public static class Cryptography {
        public static string ComputeSHA256(string s) {
            string hash = string.Empty;

            using (SHA256 sha256 = SHA256.Create()) {
                byte[] hashValue = sha256.ComputeHash(Encoding.UTF8.GetBytes(s));

                foreach (byte b in hashValue) hash += $"{b:x2}";
            }

            return hash;
        }

        public static string GetHashedPassword(string password) {
            return ComputeSHA256(Program.Config.PasswordSalt + password + Program.Config.PasswordSalt);
        }

        static BrancaService branca = null;

        private static void CheckBrancaService() {
            if (branca == null) {
                byte[] b = Encoding.UTF8.GetBytes(Program.Config.BrancaKey);
                branca = new BrancaService(b, new BrancaSettings {
                    MaxStackLimit = 1024,
                    TokenLifetimeInSeconds = Authentication.ACCESS_TOKEN_EXPIRES_IN
                });
            }
        }

        public static string GenerateAccessToken(long userId, string salt) {
            CheckBrancaService();
            return branca.Encode(userId.ToString() + "\n" + salt);
        }

        public static long CheckAccessToken(string token) {
            CheckBrancaService();
            try {
                if (branca.TryDecode(token, out byte[] payload)) {
                    string data = Encoding.UTF8.GetString(payload);
                    return Convert.ToInt64(data.Split("\n")[0]);
                }
            } catch (Exception ex) {
                return -3;
            }
            return -2;
        }
    }
}