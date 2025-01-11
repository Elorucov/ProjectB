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

            uint reportId = await Reports.CreateAsync(mid, productId, title, steps, actual, expected);
            return Results.Json(new APIResponse<uint>(reportId));
        }

        public static async Task<IResult> GetAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();
            uint productId = 0;

            bool fromOneMember = false;
            bool extended = false;
            if (request.TryGetParameter("creator_id", out string cidStr) && uint.TryParse(cidStr, out mid)) fromOneMember = mid > 0;
            if (request.TryGetParameter("product_id", out string pidStr)) uint.TryParse(pidStr, out productId);
            if (request.TryGetParameter("extended", out string ext)) extended = ext == "1";

            var response = await Reports.GetAsync(mid, productId, fromOneMember, extended);
            var reports = response.Item1;
            var members = response.Item2;
            return Results.Json(new APIResponse<APIList<ReportDTO>>(new APIList<ReportDTO>(reports, reports.Count) { Members = members }));
        }
    }
}
