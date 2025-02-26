﻿using ELOR.ProjectB.API.DTO;
using ELOR.ProjectB.Core;

namespace ELOR.ProjectB.API.Methods {
    public class ReportsAPI {
        public static async Task<IResult> ChangeSeverityAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();

            uint reportId = request.ValidateAndGetUIntValue("report_id");
            byte severity = request.ValidateAndGetByteConstValue("severity", StaticValues.SeverityList.GetKeys(), "Check server.getEnumStrings method first");
            request.TryGetParameter("comment", out string comment);

            uint newCommentId = await Reports.ChangeSeverityAsync(mid, reportId, severity, comment);
            return Results.Json(new APIResponse<uint>(newCommentId));
        }

        public static async Task<IResult> ChangeStatusAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();

            uint reportId = request.ValidateAndGetUIntValue("report_id");
            byte status = request.ValidateAndGetByteConstValue("status", StaticValues.BugreportStatuses.GetKeys(), "Check server.getEnumStrings method first");
            request.TryGetParameter("comment", out string comment);

            uint newCommentId = await Reports.ChangeStatusAsync(mid, reportId, status, comment);
            return Results.Json(new APIResponse<uint>(newCommentId));
        }

        public static async Task<IResult> CreateAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();

            uint productId = request.ValidateAndGetUIntValue("product_id");
            string title = request.ValidateAndGetValue("title", 0, 128);
            string steps = request.ValidateAndGetValue("steps", 0, 4096);
            string actual = request.ValidateAndGetValue("actual", 0, 2048);
            string expected = request.ValidateAndGetValue("expected", 0, 2048);
            byte severity = request.ValidateAndGetByteConstValue("severity", StaticValues.SeverityList.GetKeys(), "Check server.getEnumStrings method first");
            byte problemType = request.ValidateAndGetByteConstValue("problem_type", StaticValues.ProblemTypesList.GetKeys(), "Check server.getEnumStrings method first");

            uint reportId = await Reports.CreateAsync(mid, productId, title, steps, actual, expected, severity, problemType);
            return Results.Json(new APIResponse<uint>(reportId));
        }

        public static async Task<IResult> CreateCommentAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();

            uint reportId = request.ValidateAndGetUIntValue("report_id");
            string comment = request.ValidateAndGetValue("comment", 0, 1024);

            uint commentId = await Reports.CreateCommentAsync(mid, reportId, comment);
            return Results.Json(new APIResponse<uint>(commentId));
        }

        public static async Task<IResult> DeleteAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();

            uint reportId = request.ValidateAndGetUIntValue("report_id");

            bool result = await Reports.DeleteAsync(mid, reportId);
            return Results.Json(new APIResponse<bool>(result));
        }

        // TODO: check and edit only these parameters that requested.
        public static async Task<IResult> EditAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();

            uint reportId = request.ValidateAndGetUIntValue("report_id");
            string title = request.ValidateAndGetValue("title", 0, 128);
            string steps = request.ValidateAndGetValue("steps", 0, 4096);
            string actual = request.ValidateAndGetValue("actual", 0, 2048);
            string expected = request.ValidateAndGetValue("expected", 0, 2048);
            byte problemType = request.ValidateAndGetByteConstValue("problem_type", StaticValues.ProblemTypesList.GetKeys(), "Check server.getEnumStrings method first");

            bool result = await Reports.EditAsync(mid, reportId, title, steps, actual, expected, problemType);
            return Results.Json(new APIResponse<bool>(result));
        }

        public static async Task<IResult> EditCommentAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();

            uint commentId = request.ValidateAndGetUIntValue("comment_id");
            string comment = request.ValidateAndGetValue("comment", 0, 1024);

            bool result = await Reports.EditCommentAsync(mid, commentId, comment);
            return Results.Json(new APIResponse<bool>(result));
        }

        public static async Task<IResult> GetAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();

            uint creatorId = 0;
            uint productId = 0;
            byte severity = 0;
            byte problemType = 0;
            byte status = 0;
            bool extended = false;

            if (request.TryGetParameter("creator_id", out string cidStr)) uint.TryParse(cidStr, out creatorId);
            if (request.TryGetParameter("product_id", out string pidStr)) uint.TryParse(pidStr, out productId);
            if (request.TryGetParameter("severity", out string sevStr)) byte.TryParse(sevStr, out severity);
            if (request.TryGetParameter("problem_type", out string ptStr)) byte.TryParse(ptStr, out problemType);
            if (request.TryGetParameter("status", out string stStr)) byte.TryParse(stStr, out status);
            if (request.TryGetParameter("extended", out string ext)) extended = ext == "1";

            var data = await Reports.GetAsync(mid, creatorId, productId, severity, problemType, status, extended);
            var reports = data.Item1;
            var products = data.Item2;
            var members = data.Item3;

            object response = null;
            if (extended) {
                response = new APIResponse<ReportsList>(new ReportsList(reports, reports.Count) { Members = members, Products = products });
            } else {
                response = new APIResponse<APIList<ReportDTO>>(new APIList<ReportDTO>(reports, reports.Count));
            }

            return Results.Json(response);
        }

        public static async Task<IResult> GetByIdAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();

            uint reportId = request.ValidateAndGetUIntValue("report_id");

            var data = await Reports.GetByIdAsync(mid, reportId);
            var report = data.Item1;
            var product = data.Item2;
            var author = data.Item3;

            var response = new APIResponse<SingleReportResponseDTO>(new SingleReportResponseDTO {
                Report = report,
                Product = product,
                Author = author
            });

            return Results.Json(response);
        }

        public static async Task<IResult> GetCommentsAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();

            uint reportId = 0;
            bool extended = false;

            if (request.TryGetParameter("report_id", out string cidStr)) uint.TryParse(cidStr, out reportId);
            if (request.TryGetParameter("extended", out string ext)) extended = ext == "1";

            var data = await Reports.GetCommentsAsync(mid, reportId, extended);
            var comments = data.Item1;
            var members = data.Item2;

            object response = null;
            if (extended) {
                response = new APIResponse<APIListWithMembers<ReportCommentDTO>>(new APIListWithMembers<ReportCommentDTO>(comments, comments.Count) { Members = members });
            } else {
                response = new APIResponse<APIList<ReportCommentDTO>>(new APIList<ReportCommentDTO>(comments, comments.Count));
            }

            return Results.Json(response);
        }
    }
}
