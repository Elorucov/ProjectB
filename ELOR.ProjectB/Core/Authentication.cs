using ELOR.ProjectB.Core.Exceptions;
using ELOR.ProjectB.DataBase;
using MySql.Data.MySqlClient;

namespace ELOR.ProjectB.Core {
    public static class Authentication {
        public const uint ACCESS_TOKEN_EXPIRES_IN = 31536000;

        public static async Task<KeyValuePair<uint, string>> GetAccessTokenForMemberAsync(string login, string password) {
            string phash = Cryptography.GetHashedPassword(password);

            MySqlCommand cmd1 = new MySqlCommand(@"SELECT member_id FROM member_credentials WHERE password_hash = @ph AND member_id IN (SELECT id FROM members where user_name = @login);", DBClient.Connection);
            cmd1.Parameters.AddWithValue("@ph", phash);
            cmd1.Parameters.AddWithValue("@login", login);
            var resp = await cmd1.ExecuteReaderAsync();

            if (!resp.HasRows) {
                resp.Close();
                cmd1.Dispose();
                throw new InvalidCredentialException();
            }

            resp.Read();
            uint id = (uint)resp.GetDecimal(0);
            resp.Close();
            cmd1.Dispose();

            string hash = Cryptography.ComputeSHA256(password);
            string token = Cryptography.GenerateAccessToken(id, hash);
            return new KeyValuePair<uint, string>(id, token);
        }

        public static uint CheckAccessToken(HttpRequest request) {
            string token = request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(token)) throw new AuthorizationFailedException("no access token passed");
            string[] bearer = token.Split(" ");
            if (bearer.Length == 2 && bearer[0] == "Bearer") {
                long result = Cryptography.CheckAccessToken(bearer[1]);
                if (result > 0) return (uint)result;
            }
            throw new AuthorizationFailedException("invalid access token");
        }
    }
}
