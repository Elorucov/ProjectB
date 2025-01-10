using ELOR.ProjectB.Core;
using ELOR.ProjectB.Core.Exceptions;
using ELOR.ProjectB.API.DTO;

namespace ELOR.ProjectB.API.Methods {
    public class ProductsAPI {
        public static async Task<IResult> CreateAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();

            if (!request.TryGetParameter("name", out string name)) throw new InvalidParameterException("name is empty");
            if (name.Length > 64) throw new InvalidParameterException("name length must be less than 64");
            if (name.Length < 2) throw new InvalidParameterException("name length must be greater than 2");

            uint productId = await Products.CreateAsync(mid, name);
            return Results.Json(new APIResponse<uint>(productId));
        }

        public static async Task<IResult> SetAsFinishedAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();

            if (!request.TryGetParameter("product_id", out string pid)) throw new InvalidParameterException("product_id is not defined");
            if (!uint.TryParse(pid, out uint productId) || productId == 0) throw new InvalidParameterException("product_id is invalid");

            bool result = await Products.SetAsFinishedAsync(mid, productId);
            return Results.Json(new APIResponse<bool>(result));
        }
    }
}