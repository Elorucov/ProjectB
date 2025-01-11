using ELOR.ProjectB.API.DTO;
using ELOR.ProjectB.Core;

namespace ELOR.ProjectB.API.Methods {
    public class ReportsAPI {
        public static async Task<IResult> CreateAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();

            uint productId = request.ValidateAndGetUIntValue("product_id");
            string title = request.ValidateAndGetValue("title", 0, 128);
            string text = request.ValidateAndGetValue("text", 0, 4096);
            string actual = request.ValidateAndGetValue("actual", 0, 2048);
            string excepted = request.ValidateAndGetValue("excepted", 0, 2048);

            await Task.Delay(1);
            throw new NotImplementedException();
            // return Results.Json(new APIResponse<uint>(0));
        }
    }
}
