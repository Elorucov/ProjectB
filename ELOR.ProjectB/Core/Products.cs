using ELOR.ProjectB.API.DTO;
using ELOR.ProjectB.Core.Exceptions;
using ELOR.ProjectB.DataBase;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace ELOR.ProjectB.Core {
    public enum ProductsFilter : byte {
        All = 1, NonFinished = 2
    }

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

        public static async Task<List<ProductDTO>> GetAsync(uint ownerId, bool onlyOwned, ProductsFilter filter) {
            string sql = $"SELECT owner_id FROM products WHERE id = @id;";
            switch (filter) {
                case ProductsFilter.All:
                    sql = $"SELECT * FROM products";
                    break;
                case ProductsFilter.NonFinished:
                    sql = $"SELECT * FROM products WHERE is_finished = 0";
                    break;
            }

            if (onlyOwned) {
                switch (filter) {
                    case ProductsFilter.All:
                        sql += $" WHERE owner_id = {ownerId};";
                        break;
                    case ProductsFilter.NonFinished:
                        sql += $" AND owner_id = {ownerId};";
                        break;
                }
            }

            MySqlCommand cmd1 = new MySqlCommand(sql, DBClient.Connection);
            DbDataReader resp = await cmd1.ExecuteReaderAsync();
            cmd1.Dispose();

            List<ProductDTO> products = new List<ProductDTO>();
            if (resp.HasRows) {
                while (resp.Read()) {
                    uint productId = (uint)resp.GetDecimal(0);
                    uint productOwnerId = (uint)resp.GetDecimal(1);
                    string name = resp.GetString(2);
                    bool isFinished = resp.GetBoolean(3);
                    products.Add(new ProductDTO{
                        Id = productId,
                        OwnerId = productOwnerId,
                        Name = name,
                        IsFinished = isFinished
                    });
                }
            }

            await resp.DisposeAsync();
            return products;
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
