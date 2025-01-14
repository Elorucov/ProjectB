using ELOR.ProjectB.API.DTO;
using ELOR.ProjectB.Core.Exceptions;
using ELOR.ProjectB.DataBase;
using MySql.Data.MySqlClient;
using System.Data.Common;
using static Mysqlx.Error.Types;
using static System.Net.Mime.MediaTypeNames;

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

        public static async Task<uint> ChangeStatusAsync(uint authorizedMemberId, uint reportId, byte newStatus, string comment) {
            string sql = $"CALL updateStatus(@mid, @rid, @st, @cm);";
            MySqlCommand cmd1 = new MySqlCommand(sql, DBClient.Connection);
            cmd1.Parameters.AddWithValue("@mid", authorizedMemberId);
            cmd1.Parameters.AddWithValue("@rid", reportId);
            cmd1.Parameters.AddWithValue("@st", newStatus);
            cmd1.Parameters.AddWithValue("@cm", comment);
            object resp = await cmd1.ExecuteScalarAsync();
            cmd1.Dispose();

            if (resp != null) {
                return Convert.ToUInt32(resp);
            } else {
                throw new ApplicationException("unable to execute DB procedure, try later");
            }
        }

        public static async Task<Tuple<List<ReportDTO>, List<ProductDTO>, List<MemberDTO>>> GetAsync(uint authorizedMemberId, uint creatorId, uint productId, byte severity, byte problemType, byte status, bool extended) {
            if (creatorId > 0 && severity == 5 && authorizedMemberId != creatorId) throw new AccessException();
            bool dontGetVulnerabilities = severity == 0 && authorizedMemberId != creatorId;

            string sql = string.Empty;
            if (!extended) {
                sql = dontGetVulnerabilities ? "SELECT r.id, r.product_id, r.creator_id, r.time, r.title, r.severity, r.problem_type, r.status FROM reports r JOIN products p ON r.product_id = p.id" : "SELECT id, product_id, creator_id, time, title, severity, problem_type FROM reports";
            } else {
                sql = dontGetVulnerabilities ? "SELECT r.* FROM reports r JOIN products p ON r.product_id = p.id" : "SELECT * FROM reports";
            }

            List<string> filters = new List<string>();

            if (creatorId > 0) filters.Add($"creator_id = {creatorId}");
            if (productId > 0) filters.Add($"product_id = {productId}");
            if (severity > 0) filters.Add($"severity = {severity}");
            if (problemType > 0) filters.Add($"problem_type = {problemType}");
            if (status > 0) filters.Add($"status = {status}");
            if (dontGetVulnerabilities) filters.Add($"NOT (r.severity = 5 AND (r.creator_id != {authorizedMemberId} AND p.owner_id != {authorizedMemberId}));");

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
                    byte status2 = resp.GetByte(extended ? 10 : 7);
                    reports.Add(new ReportDTO {
                        Id = reportId,
                        ProductId = productId2,
                        CreatorId = creatorId2,
                        Created = creationTime,
                        Title = title,
                        Steps = steps,
                        Actual = actual,
                        Expected = expected,
                        Severity = StaticValues.SeverityList.ToAPIObject(severity2, extended),
                        ProblemType = StaticValues.ProblemTypesList.ToAPIObject(problemType2, extended),
                        Status = StaticValues.BugreportStatuses.ToAPIObject(status2, extended)
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