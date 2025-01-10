using ELOR.ProjectB.Core.Exceptions;
using ELOR.ProjectB.DataBase;
using MySql.Data.MySqlClient;

namespace ELOR.ProjectB.Core {
    public static class Products {
        public static async Task<uint> CreateAsync(uint ownerId, string name) {
            string sql = $"BEGIN; INSERT INTO products (owner_id, name) VALUES (@mid, @name); SELECT LAST_INSERT_ID(); COMMIT;";
            MySqlCommand cmd1 = new MySqlCommand(sql, DBClient.Connection);
            cmd1.Parameters.AddWithValue("@mid", ownerId);
            cmd1.Parameters.AddWithValue("@name", name);
            object resp = await cmd1.ExecuteScalarAsync();
            cmd1.Dispose();

            if (resp != null) {
                return Convert.ToUInt32(resp);
            } else {
                throw new ApplicationException("unable to add entry in DB, try later");
            }
        }

        public static async Task<bool> CheckIsOwnerAsync(uint memberId, uint productId) {
            string sql = $"SELECT owner_id FROM products WHERE id = @id;";
            MySqlCommand cmd1 = new MySqlCommand(sql, DBClient.Connection);
            cmd1.Parameters.AddWithValue("@id", productId);
            object resp = await cmd1.ExecuteScalarAsync();
            cmd1.Dispose();

            if (resp != null) {
                uint ownerId = Convert.ToUInt32(resp);
                return ownerId == memberId;
            } else {
                throw new NotFoundException();
            }
        }

        public static async Task<bool> SetAsFinishedAsync(uint ownerId, uint productId) {
            if (await CheckIsOwnerAsync(ownerId, productId) == false) throw new PermissionException("you are not an owner");

            string sql = $"UPDATE products SET is_finished = 1 WHERE id = @pid;";
            MySqlCommand cmd1 = new MySqlCommand(sql, DBClient.Connection);
            cmd1.Parameters.AddWithValue("@pid", productId);
            int resp = await cmd1.ExecuteNonQueryAsync();
            cmd1.Dispose();

            if (resp > 0) {
                return true;
            } else {
                throw new ApplicationException("unable to update an entry in DB, try later");
            }
        }
    }
}
