using ELOR.ProjectB.Core;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using System.Data;
using System.Diagnostics;

namespace ELOR.ProjectB.DataBase {
    public static class DBClient {
        static MySqlConnection _connection;
        public static MySqlConnection Connection { get { return GetConnection(); } }

        private static MySqlConnection GetConnection() {
            if (_connection == null) {
                string dburl = Program.Config.DBURL;
                string login = Program.Config.DBLogin;
                string pass = Program.Config.DBPass;
                string name = Program.Config.DBName;
                _connection = new MySqlConnection($"server={dburl};uid={login};pwd={pass};database={name}");
                _connection.Open();
            }
            return _connection;
        }

        public static async Task TestDBProcedure() {
            string sql = $"CALL testerror (3);";
            MySqlCommand cmd2 = new MySqlCommand(sql, Connection);
            await cmd2.ExecuteNonQueryAsync();
        }

        public static async Task<bool> SetupDatabaseAsync() {
            bool needSetup = false;
            Debug.WriteLine($"DBClient.SetupDatabaseAsync: starting");
            MySqlCommand cmd1 = new MySqlCommand(@"SELECT * FROM members", Connection);
            try {
                object resp = await cmd1.ExecuteScalarAsync();
            } catch (MySqlException dbex) {
                needSetup = dbex.Message.EndsWith("doesn't exist");
            }

            if (!needSetup) {
                Debug.WriteLine($"DBClient.SetupDatabaseAsync: \"members\" table has data! Setup is not required.");
                return false;
            }

            Debug.WriteLine($"DBClient.SetupDatabaseAsync: Dropping all and creating tables in DB...");
            MySqlScript script = new MySqlScript(Connection, File.ReadAllText("db.sql"));
            int executedStatements = await script.ExecuteAsync();
            Debug.WriteLine($"DBClient.SetupDatabaseAsync: Executed statements: {executedStatements}");

            Debug.WriteLine($"DBClient.SetupDatabaseAsync: Creating first user...");
            string phash = Cryptography.GetHashedPassword(Program.Config.FirstUserPassword);

            string sql = $"BEGIN; INSERT INTO members (user_name, first_name, last_name) VALUES (@un, @fn, @ln); INSERT INTO member_credentials (member_id, password_hash) VALUES (1, @ph); COMMIT;";
            MySqlCommand cmd2 = new MySqlCommand(sql, Connection);
            cmd2.Parameters.AddWithValue("@un", Program.Config.FirstUserLogin);
            cmd2.Parameters.AddWithValue("@fn", Program.Config.FirstUserName);
            cmd2.Parameters.AddWithValue("@ln", Program.Config.FirstUserLastName);
            cmd2.Parameters.AddWithValue("@ph", phash);
            int rows = await cmd2.ExecuteNonQueryAsync();
            Debug.WriteLine($"DBClient.SetupDatabaseAsync: User creation result: {rows}");
            if (rows <= 0) throw new InvalidOperationException("");

            return true;
        }
    }
}