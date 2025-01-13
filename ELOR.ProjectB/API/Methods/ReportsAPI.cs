using ELOR.ProjectB.API.DTO;
using ELOR.ProjectB.Core;

namespace ELOR.ProjectB.API.Methods {
    public class ReportsAPI {
        public static async Task<IResult> CreateAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();

            uint productId = request.ValidateAndGetUIntValue("product_id");
            string title = request.ValidateAndGetValue("title", 0, 128);
            string steps = request.ValidateAndGetValue("steps", 0, 4096);
            string actual = request.ValidateAndGetValue("actual", 0, 2048);
            string expected = request.ValidateAndGetValue("expected", 0, 2048);
            byte severity = request.ValidateAndGetByteConstValue("severity", 1, 5);
            byte problemType = request.ValidateAndGetByteConstValue("problem_type", 1, 8);

            uint reportId = await Reports.CreateAsync(mid, productId, title, steps, actual, expected, severity, problemType);
            return Results.Json(new APIResponse<uint>(reportId));
        }

        public static async Task<IResult> GetAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();

            uint creatorId = 0;
            uint productId = 0;
            byte severity = 0;
            byte problemType = 0;
            bool extended = false;

            if (request.TryGetParameter("creator_id", out string cidStr)) uint.TryParse(cidStr, out creatorId);
            if (request.TryGetParameter("product_id", out string pidStr)) uint.TryParse(pidStr, out productId);
            if (request.TryGetParameter("severity", out string sevStr)) byte.TryParse(sevStr, out severity);
            if (request.TryGetParameter("problem_type", out string ptStr)) byte.TryParse(ptStr, out problemType);
            if (request.TryGetParameter("extended", out string ext)) extended = ext == "1";

            var data = await Reports.GetAsync(mid, creatorId, productId, severity, problemType, extended);
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
    }
}
