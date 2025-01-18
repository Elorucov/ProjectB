﻿using ELOR.ProjectB.API.DTO;
using ELOR.ProjectB.Core.Exceptions;
using ELOR.ProjectB.DataBase;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Xml.Linq;

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

        public static async Task<List<ProductDTO>> GetByIdAsync(List<uint> ids) {
            string idstr = string.Join(",", ids);
            string sql = $"SELECT * FROM products WHERE id IN ({idstr})";

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
                    products.Add(new ProductDTO {
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

        public static async Task<List<Tuple<ProductDTO, int>>> GetCreatedReportCountByMemberPerProductsAsync(uint authorizedMemberId, uint creatorId) {
            bool dontGetVulnerabilities = authorizedMemberId != creatorId;

            string sql = !dontGetVulnerabilities ?
                "SELECT r.product_id, p.owner_id, p.name, p.is_finished, COUNT(r.id) AS count FROM reports r JOIN products p ON p.id = r.product_id WHERE creator_id = @mid GROUP BY product_id ORDER BY count DESC;" :
                "SELECT r.product_id, p.owner_id, p.name, p.is_finished, COUNT(r.id) AS count FROM reports r JOIN products p ON p.id = r.product_id WHERE creator_id = @mid AND severity != 5 GROUP BY product_id ORDER BY count DESC;";

            MySqlCommand cmd1 = new MySqlCommand(sql, DBClient.Connection);
            cmd1.Parameters.AddWithValue("@mid", creatorId);
            DbDataReader resp = await cmd1.ExecuteReaderAsync();
            cmd1.Dispose();

            List<Tuple<ProductDTO, int>> products = new List<Tuple<ProductDTO, int>>();
            if (resp.HasRows) {
                while (resp.Read()) {
                    uint productId = (uint)resp.GetDecimal(0);
                    uint productOwnerId = (uint)resp.GetDecimal(1);
                    string name = resp.GetString(2);
                    bool isFinished = resp.GetBoolean(3);
                    var product = new ProductDTO {
                        Id = productId,
                        OwnerId = productOwnerId,
                        Name = name,
                        IsFinished = isFinished
                    };
                    int count = resp.IsDBNull(4) ? 0 : (int)resp.GetDecimal(4);
                    if (count > 0) products.Add(new Tuple<ProductDTO, int>(product, count));
                }
            }
            await resp.DisposeAsync();
            return products;
        }

        public static async Task<Tuple<List<ProductDTO>, List<MemberDTO>>> GetFilteredAsync(uint ownerId, bool onlyOwned, ProductsFilter filter, bool extended) {
            string sql = string.Empty;
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
            List<uint> mids = new List<uint>();
            List<MemberDTO> members = null;
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
                    if (extended && !mids.Contains(productOwnerId)) mids.Add(productOwnerId);
                }
                resp.Close();
                if (extended && mids.Count > 0) members = await Members.GetByIdAsync(mids);
            }
            await resp.DisposeAsync();
            return new Tuple<List<ProductDTO>, List<MemberDTO>>(products, members);
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
                throw new NotFoundException("there is no product with given id");
            }
        }

        public static async Task<bool> CheckIsFinishedAsync(uint productId) {
            string sql = $"SELECT is_finished FROM products WHERE id = @id;";
            MySqlCommand cmd1 = new MySqlCommand(sql, DBClient.Connection);
            cmd1.Parameters.AddWithValue("@id", productId);
            object resp = await cmd1.ExecuteScalarAsync();
            cmd1.Dispose();

            if (resp != null && resp is bool result) {
                return result;
            } else {
                throw new NotFoundException("there is no product with given id");
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
