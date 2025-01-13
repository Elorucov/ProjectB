using ELOR.ProjectB.API.DTO;
using ELOR.ProjectB.Core.Exceptions;
using ELOR.ProjectB.DataBase;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace ELOR.ProjectB.Core {
    public static class Reports {
        public static async Task<uint> CreateAsync(uint creatorId, uint productId, string title, string text, string actual, string expected, byte severity, byte problemType) {
            bool isFinished = await Products.CheckIsFinishedAsync(productId);
            if (isFinished) throw new FinishedProductException();

            string sql = $"BEGIN; INSERT INTO reports (product_id, creator_id, time, title, text, actual, expected, severity, problem_type) VALUES (@pid, @mid, @time, @title, @text, @act, @exp, @s, @pt); SELECT LAST_INSERT_ID(); COMMIT;";
            MySqlCommand cmd1 = new MySqlCommand(sql, DBClient.Connection);
            cmd1.Parameters.AddWithValue("@mid", creatorId);
            cmd1.Parameters.AddWithValue("@pid", productId);
            cmd1.Parameters.AddWithValue("@time", DateTimeOffset.Now.ToUnixTimeSeconds());
            cmd1.Parameters.AddWithValue("@title", title);
            cmd1.Parameters.AddWithValue("@text", text);
            cmd1.Parameters.AddWithValue("@act", actual);
            cmd1.Parameters.AddWithValue("@exp", expected);
            cmd1.Parameters.AddWithValue("@s", severity);
            cmd1.Parameters.AddWithValue("@pt", problemType);
            object resp = await cmd1.ExecuteScalarAsync();
            cmd1.Dispose();

            if (resp != null) {
                return Convert.ToUInt32(resp);
            } else {
                throw new ApplicationException("unable to add entry in DB, try later");
            }
        }

        public static async Task<Tuple<List<ReportDTO>, List<ProductDTO>, List<MemberDTO>>> GetAsync(uint authorizedMemberId, uint creatorId, uint productId, byte severity, byte problemType, bool extended) {
            if (creatorId > 0 && severity == 5 && authorizedMemberId != creatorId) throw new AccessException();
            
            string sql = string.Empty;
            if (!extended) {
                sql = $"SELECT id, product_id, creator_id, time, title, severity, problem_type FROM reports";
            } else {
                sql = $"SELECT * FROM reports";
            }

            List<string> filters = new List<string>();

            if (creatorId > 0) filters.Add($"creator_id = {creatorId}");
            if (productId > 0) filters.Add($"product_id = {productId}");
            if (severity > 0) filters.Add($"severity = {severity}");
            if (problemType > 0) filters.Add($"problem_type = {problemType}");
            if (severity == 0 && authorizedMemberId != creatorId) filters.Add($"NOT(severity = 5 AND creator_id != {authorizedMemberId})");

            if (filters.Count > 0) sql +=  " WHERE " + string.Join(" AND ", filters);

            MySqlCommand cmd1 = new MySqlCommand(sql, DBClient.Connection);
            DbDataReader resp = await cmd1.ExecuteReaderAsync();
            cmd1.Dispose();

            List<ReportDTO> reports = new List<ReportDTO>();
            List<uint> mids = new List<uint>();
            List<uint> pids = new List<uint>();
            List<ProductDTO> products = null;
            List<MemberDTO> members = null;
            if (resp.HasRows) {
                while (resp.Read()) {
                    uint reportId = (uint)resp.GetDecimal(0);
                    uint productId2 = (uint)resp.GetDecimal(1);
                    uint creatorId2 = (uint)resp.GetDecimal(2);
                    long creationTime = (long)resp.GetDecimal(3);
                    string title = resp.GetString(4);
                    string steps = extended ? resp.GetString(5) : null;
                    string actual = extended ? resp.GetString(6) : null;
                    string expected = extended ? resp.GetString(7) : null;
                    byte severity2 = resp.GetByte(extended ? 8 : 5);
                    byte problemType2 = resp.GetByte(extended ? 9 : 6);
                    reports.Add(new ReportDTO {
                        Id = reportId,
                        ProductId = productId2,
                        CreatorId = creatorId2,
                        CreationTime = creationTime,
                        Title = title,
                        Steps = steps,
                        Actual = actual,
                        Expected = expected,
                        Severity = severity2,
                        ProblemType = problemType2
                    });
                    if (extended && !mids.Contains(creatorId2)) mids.Add(creatorId2);
                    if (extended && !pids.Contains(productId2)) pids.Add(productId2);
                }
                resp.Close();
                if (extended && mids.Count > 0) members = await Members.GetByIdAsync(mids);
                if (extended && pids.Count > 0) products = await Products.GetByIdAsync(pids);
            }
            await resp.DisposeAsync();
            return new Tuple<List<ReportDTO>, List<ProductDTO>, List<MemberDTO>>(reports, products, members);
        }
    }
}