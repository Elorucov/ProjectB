using ELOR.ProjectB.Core.Exceptions;
using ELOR.ProjectB.DataBase;
using MySql.Data.MySqlClient;

namespace ELOR.ProjectB.Core {
    public static class Invites {

        // TODO: procedure
        public static async Task<string> CreateAsync(uint creatorId, string userName) {
            MySqlCommand cmd0 = new MySqlCommand(@"SELECT id FROM members WHERE user_name = @uname", DBClient.Connection);
            cmd0.Parameters.AddWithValue("@uname", userName);
            var extmid = await cmd0.ExecuteScalarAsync();

            if (extmid != null && (uint)extmid > 0) throw new AlreadyExistException($"a member with provided user_name is already registered");

            MySqlCommand cmd1 = new MySqlCommand(@"SELECT code FROM invites WHERE user_name = @uname AND invited_member_id = 0;", DBClient.Connection);
            cmd1.Parameters.AddWithValue("@uname", userName);
            var resp = await cmd1.ExecuteReaderAsync();

            if (!resp.HasRows) {
                resp.Close();
                cmd1.Dispose();

                string code = Guid.NewGuid().ToString("N");

                string sql = $"INSERT INTO invites (code, user_name, creation_time, created_by) VALUES (@code, @un, @ct, @cb);";
                MySqlCommand cmd2 = new MySqlCommand(sql, DBClient.Connection);
                cmd2.Parameters.AddWithValue("@code", code);
                cmd2.Parameters.AddWithValue("@un", userName);
                cmd2.Parameters.AddWithValue("@ct", DateTimeOffset.Now.ToUnixTimeSeconds());
                cmd2.Parameters.AddWithValue("@cb", creatorId);
                int rows = await cmd2.ExecuteNonQueryAsync();
                cmd2.Dispose();

                if (rows == 1) {
                    return code;
                } else {
                    throw new ApplicationException("unable to add entry in DB, try later");
                }
            } else {
                resp.Read();
                string code = resp.GetString(0);
                resp.Close();
                cmd1.Dispose();
                return code;
            }
        }

        // TODO: procedure
        public static async Task<Tuple<uint, string>> RegisterUserByCodeAsync(string code, string password, string firstName, string lastName) {
            MySqlCommand cmd1 = new MySqlCommand(@"SELECT user_name FROM invites WHERE code = @code AND invited_member_id = 0;", DBClient.Connection);
            cmd1.Parameters.AddWithValue("@code", code);
            var resp = await cmd1.ExecuteReaderAsync();

            if (!resp.HasRows) {
                resp.Close();
                cmd1.Dispose();
                throw new NotFoundException("this invite code is not found or already used");
            } else {
                resp.Read();

                string userName = resp.GetString(0);
                resp.Close();
                cmd1.Dispose();

                string sql = $"BEGIN; INSERT INTO members (user_name, first_name, last_name) VALUES (@un, @fn, @ln); INSERT INTO member_credentials (member_id, password_hash) VALUES (LAST_INSERT_ID(), @ph); UPDATE invites SET invited_member_id = LAST_INSERT_ID() WHERE code = @code; SELECT id, user_name FROM members WHERE id = LAST_INSERT_ID(); COMMIT;";
                MySqlCommand cmd2 = new MySqlCommand(sql, DBClient.Connection);
                cmd2.Parameters.AddWithValue("@un", userName);
                cmd2.Parameters.AddWithValue("@fn", firstName);
                cmd2.Parameters.AddWithValue("@ln", lastName);
                cmd2.Parameters.AddWithValue("@ph", Cryptography.GetHashedPassword(password));
                cmd2.Parameters.AddWithValue("@code", code);
                var resp2 = await cmd2.ExecuteReaderAsync();

                if (!resp2.HasRows) {
                    resp2.Close();
                    cmd2.Dispose();
                    throw new ApplicationException("database problem, try later");
                }

                resp2.Read();
                uint id = (uint)resp2.GetDecimal(0);
                string login = resp2.GetString(1);
                resp2.Close();
                cmd2.Dispose();
                return new Tuple<uint, string>(id, login);
            }
        }
    }
}
