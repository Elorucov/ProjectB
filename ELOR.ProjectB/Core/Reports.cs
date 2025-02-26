﻿using ELOR.ProjectB.API.DTO;
using ELOR.ProjectB.Core.Exceptions;
using ELOR.ProjectB.DataBase;
using MySql.Data.MySqlClient;
using System.ComponentModel.Design;
using System.Data.Common;
using System.Xml.Linq;
using static Mysqlx.Error.Types;

namespace ELOR.ProjectB.Core {
    public static class Reports {
        // TODO: procedure
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

        public static async Task<uint> CreateCommentAsync(uint creatorId, uint reportId, string comment) {
            string sql = $"CALL createComment(@mid, @rid, @cmt)";
            MySqlCommand cmd1 = new MySqlCommand(sql, DBClient.Connection);
            cmd1.Parameters.AddWithValue("@mid", creatorId);
            cmd1.Parameters.AddWithValue("@rid", reportId);
            cmd1.Parameters.AddWithValue("@cmt", comment);
            object resp = await cmd1.ExecuteScalarAsync();
            cmd1.Dispose();

            if (resp != null) {
                return Convert.ToUInt32(resp);
            } else {
                throw new ApplicationException("unable to execute DB procedure, try later");
            }
        }

        public static async Task<uint> ChangeSeverityAsync(uint authorizedMemberId, uint reportId, byte newSeverity, string comment) {
            string sql = $"CALL updateSeverity(@mid, @rid, @sv, @cm);";
            MySqlCommand cmd1 = new MySqlCommand(sql, DBClient.Connection);
            cmd1.Parameters.AddWithValue("@mid", authorizedMemberId);
            cmd1.Parameters.AddWithValue("@rid", reportId);
            cmd1.Parameters.AddWithValue("@sv", newSeverity);
            cmd1.Parameters.AddWithValue("@cm", comment);
            object resp = await cmd1.ExecuteScalarAsync();
            cmd1.Dispose();

            if (resp != null) {
                return Convert.ToUInt32(resp);
            } else {
                throw new ApplicationException("unable to execute DB procedure, try later");
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

        public static async Task<bool> DeleteAsync(uint authorizedMemberId, uint reportId) {
            string sql = $"CALL deleteReport(@mid, @rid);";
            MySqlCommand cmd1 = new MySqlCommand(sql, DBClient.Connection);
            cmd1.Parameters.AddWithValue("@mid", authorizedMemberId);
            cmd1.Parameters.AddWithValue("@rid", reportId);
            int resp = await cmd1.ExecuteNonQueryAsync();
            cmd1.Dispose();

            if (resp > 0) {
                return true;
            } else {
                throw new ApplicationException("unable to execute DB procedure, try later");
            }
        }

        public static async Task<bool> EditAsync(uint authorizedMemberId, uint reportId, string title, string text, string actual, string expected, byte problemType) {
            string sql = $"CALL editReport(@mid, @rid, @tle, @txt, @act, @exp, @pt);";
            MySqlCommand cmd1 = new MySqlCommand(sql, DBClient.Connection);
            cmd1.Parameters.AddWithValue("@mid", authorizedMemberId);
            cmd1.Parameters.AddWithValue("@rid", reportId);
            cmd1.Parameters.AddWithValue("@tle", title);
            cmd1.Parameters.AddWithValue("@txt", text);
            cmd1.Parameters.AddWithValue("@act", actual);
            cmd1.Parameters.AddWithValue("@exp", expected);
            cmd1.Parameters.AddWithValue("@pt", problemType);
            int resp = await cmd1.ExecuteNonQueryAsync();
            cmd1.Dispose();

            if (resp > 0) {
                return true;
            } else {
                throw new ApplicationException("unable to execute DB procedure, try later");
            }
        }

        public static async Task<bool> EditCommentAsync(uint authorizedMemberId, uint commentId, string comment) {
            string sql = $"CALL editComment(@mid, @cid, @cmt)";
            MySqlCommand cmd1 = new MySqlCommand(sql, DBClient.Connection);
            cmd1.Parameters.AddWithValue("@mid", authorizedMemberId);
            cmd1.Parameters.AddWithValue("@cid", commentId);
            cmd1.Parameters.AddWithValue("@cmt", comment);
            int resp = await cmd1.ExecuteNonQueryAsync();
            cmd1.Dispose();

            if (resp > 0) {
                return true;
            } else {
                throw new ApplicationException("unable to execute DB procedure, try later");
            }
        }

        private static ReportDTO ReadToReportDTO(DbDataReader resp, bool extended) {
            uint reportId = (uint)resp.GetDecimal(0);
            uint productId = (uint)resp.GetDecimal(1);
            uint creatorId = (uint)resp.GetDecimal(2);
            long creationTime = (long)resp.GetDecimal(3);
            string title = resp.GetString(4);
            string steps = extended ? resp.GetString(5) : null;
            string actual = extended ? resp.GetString(6) : null;
            string expected = extended ? resp.GetString(7) : null;
            byte severity = resp.GetByte(extended ? 8 : 5);
            byte problemType = resp.GetByte(extended ? 9 : 6);
            byte status = resp.GetByte(extended ? 10 : 7);
            long? updatedTime = resp.IsDBNull(extended ? 12 : 8) ? null : (long)resp.GetDecimal(extended ? 12 : 8);
            return new ReportDTO {
                Id = reportId,
                ProductId = productId,
                CreatorId = creatorId,
                Created = creationTime,
                Updated = updatedTime,
                Title = title,
                Steps = steps,
                Actual = actual,
                Expected = expected,
                Severity = StaticValues.SeverityList.ToAPIObject(severity, extended),
                ProblemType = StaticValues.ProblemTypesList.ToAPIObject(problemType, extended),
                Status = StaticValues.BugreportStatuses.ToAPIObject(status, extended)
            };
        }

        public static async Task<Tuple<List<ReportDTO>, List<ProductDTO>, List<MemberDTO>>> GetAsync(uint authorizedMemberId, uint creatorId, uint productId, byte severity, byte problemType, byte status, bool extended) {
            bool dontGetVulnerabilities = severity == 0 && authorizedMemberId != creatorId;

            string sql = string.Empty;
            if (!extended) {
                sql = dontGetVulnerabilities ? "SELECT r.id, r.product_id, r.creator_id, r.time, r.title, r.severity, r.problem_type, r.status, r.updated_time FROM reports r JOIN products p ON r.product_id = p.id" : "SELECT id, product_id, creator_id, time, title, severity, problem_type FROM reports";
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
                    var report = ReadToReportDTO(resp, extended);
                    reports.Add(ReadToReportDTO(resp, extended));
                    if (extended && !mids.Contains(report.CreatorId)) mids.Add(report.CreatorId);
                    if (extended && !pids.Contains(report.ProductId)) pids.Add(report.ProductId);
                }
                resp.Close();
                if (extended && mids.Count > 0) members = await Members.GetByIdAsync(mids);
                if (extended && pids.Count > 0) products = await Products.GetByIdAsync(pids);
            }
            await resp.DisposeAsync();
            return new Tuple<List<ReportDTO>, List<ProductDTO>, List<MemberDTO>>(reports, products, members);
        }

        public static async Task<Tuple<ReportDTO, ProductDTO, MemberDTO>> GetByIdAsync(uint authorizedMemberId, uint reportId) {
            string sql = "CALL getSingleReport(@mid, @rid)";
            MySqlCommand cmd1 = new MySqlCommand(sql, DBClient.Connection);
            cmd1.Parameters.AddWithValue("@mid", authorizedMemberId);
            cmd1.Parameters.AddWithValue("@rid", reportId);
            DbDataReader resp = await cmd1.ExecuteReaderAsync();
            cmd1.Dispose();

            ReportDTO report = null;
            List<uint> mids = new List<uint>();
            uint pid = 0;
            List<ProductDTO> products = null;
            List<MemberDTO> members = null;
            if (resp.HasRows) {
                while (resp.Read()) {
                    report = ReadToReportDTO(resp, true);
                    if (!mids.Contains(report.CreatorId)) mids.Add(report.CreatorId);
                    pid = report.ProductId;
                    break; // because we need 1 row.
                }
                resp.Close();
                if (mids.Count > 0) members = await Members.GetByIdAsync(mids);
                if (pid > 0) products = await Products.GetByIdAsync(new List<uint> { pid });
            }
            await resp.DisposeAsync();
            return new Tuple<ReportDTO, ProductDTO, MemberDTO>(report, products.FirstOrDefault(), members.FirstOrDefault());
        }

        public static async Task<Tuple<List<ReportCommentDTO>, List<MemberDTO>>> GetCommentsAsync(uint authorizedMemberId, uint reportId, bool extended) {
            string sql = "CALL getComments(@mid, @rid)";
            MySqlCommand cmd1 = new MySqlCommand(sql, DBClient.Connection);
            cmd1.Parameters.AddWithValue("@mid", authorizedMemberId);
            cmd1.Parameters.AddWithValue("@rid", reportId);
            DbDataReader resp = await cmd1.ExecuteReaderAsync();
            cmd1.Dispose();

            List<ReportCommentDTO> comments = new List<ReportCommentDTO>();
            List<uint> mids = new List<uint>();
            List<MemberDTO> members = null;
            if (resp.HasRows) {
                while (resp.Read()) {
                    uint commentId = (uint)resp.GetDecimal(0);
                    uint reportId2 = (uint)resp.GetDecimal(1);
                    uint creatorId2 = (uint)resp.GetDecimal(2);
                    long creationTime = (long)resp.GetDecimal(3);
                    long? updatedTime = resp.IsDBNull(4) ? null : (long)resp.GetDecimal(4);
                    byte? newSeverity = resp.IsDBNull(5) ? null : resp.GetByte(5);
                    byte? newStatus = resp.IsDBNull(6) ? null : resp.GetByte(6);
                    string comment = resp.IsDBNull(7) ? null : resp.GetString(7);
                    comments.Add(new ReportCommentDTO {
                        Id = reportId,
                        ReportId = reportId2,
                        CreatorId = creatorId2,
                        Created = creationTime,
                        Updated = updatedTime,
                        NewSeverity = newSeverity == null ? null : StaticValues.SeverityList.ToAPIObject(newSeverity.Value),
                        NewStatus = newStatus == null ? null : StaticValues.BugreportStatuses.ToAPIObject(newStatus.Value),
                        Comment = comment
                    });
                    if (extended && !mids.Contains(creatorId2)) mids.Add(creatorId2);
                }
                resp.Close();
                if (extended && mids.Count > 0) members = await Members.GetByIdAsync(mids);
            }
            await resp.DisposeAsync();
            return new Tuple<List<ReportCommentDTO>, List<MemberDTO>>(comments, members);
        }
    }
}