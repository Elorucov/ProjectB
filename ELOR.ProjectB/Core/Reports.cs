using ELOR.ProjectB.API.DTO;
using ELOR.ProjectB.Core.Exceptions;
using ELOR.ProjectB.DataBase;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace ELOR.ProjectB.Core {
    public static class Reports {
        public static async Task<uint> CreateAsync(uint creatorId, uint productId, string title, string text, string actual, string expected) {
            bool isFinished = await Products.CheckIsFinishedAsync(productId);
            if (isFinished) throw new FinishedProductException();

            string sql = $"BEGIN; INSERT INTO reports (product_id, creator_id, time, title, text, actual, expected) VALUES (@pid, @mid, @time, @title, @text, @act, @exp); SELECT LAST_INSERT_ID(); COMMIT;";
            MySqlCommand cmd1 = new MySqlCommand(sql, DBClient.Connection);
            cmd1.Parameters.AddWithValue("@mid", creatorId);
            cmd1.Parameters.AddWithValue("@pid", productId);
            cmd1.Parameters.AddWithValue("@time", DateTimeOffset.Now.ToUnixTimeSeconds());
            cmd1.Parameters.AddWithValue("@title", title);
            cmd1.Parameters.AddWithValue("@text", text);
            cmd1.Parameters.AddWithValue("@act", actual);
            cmd1.Parameters.AddWithValue("@exp", expected);
            object resp = await cmd1.ExecuteScalarAsync();
            cmd1.Dispose();

            if (resp != null) {
                return Convert.ToUInt32(resp);
            } else {
                throw new ApplicationException("unable to add entry in DB, try later");
            }
        }

        public static async Task<Tuple<List<ReportDTO>, List<MemberDTO>>> GetAsync(uint ownerId, uint productId, bool fromOneMember, bool extended) {
            string sql = string.Empty;
            if (!extended) {
                sql = $"SELECT id, product_id, creator_id, time, title FROM reports";
            } else {
                sql = $"SELECT * FROM reports";
            }
            if (fromOneMember) sql += $" WHERE creator_id = {ownerId}";
            if (productId != 0) {
                sql += fromOneMember ? " AND " : " WHERE ";
                sql += $"product_id = {productId}";
            }

            MySqlCommand cmd1 = new MySqlCommand(sql, DBClient.Connection);
            DbDataReader resp = await cmd1.ExecuteReaderAsync();
            cmd1.Dispose();

            List<ReportDTO> reports = new List<ReportDTO>();
            List<uint> mids = new List<uint>();
            List<MemberDTO> members = null;
            if (resp.HasRows) {
                while (resp.Read()) {
                    uint reportId = (uint)resp.GetDecimal(0);
                    uint productId2 = (uint)resp.GetDecimal(1);
                    uint creatorId = (uint)resp.GetDecimal(2);
                    long creationTime = (long)resp.GetDecimal(3);
                    string title = resp.GetString(4);
                    string steps = extended ? resp.GetString(5) : null;
                    string actual = extended ? resp.GetString(6) : null;
                    string expected = extended ? resp.GetString(7) : null;
                    reports.Add(new ReportDTO {
                        Id = reportId,
                        ProductId = productId2,
                        CreatorId = creatorId,
                        CreationTime = creationTime,
                        Title = title,
                        Steps = steps,
                        Actual = actual,
                        Expected = expected
                    });
                    if (extended && !mids.Contains(creatorId)) mids.Add(creatorId);
                }
                resp.Close();
                await resp.DisposeAsync();
                if (extended && mids.Count > 0) members = await Members.GetAsync(mids);
            }
            return new Tuple<List<ReportDTO>, List<MemberDTO>>(reports, members);
        }
    }
}
