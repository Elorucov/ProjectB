﻿using ELOR.ProjectB.Core;
using ELOR.ProjectB.API.DTO;

namespace ELOR.ProjectB.API.Methods {
    public class ProductsAPI {
        public static async Task<IResult> CreateAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();

            string name = request.ValidateAndGetValue("name", 2, 64);

            uint productId = await Products.CreateAsync(mid, name);
            return Results.Json(new APIResponse<uint>(productId));
        }

        public static async Task<IResult> GetAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();

            ProductsFilter filter = ProductsFilter.All;
            bool onlyOwned = false;
            bool extended = false;
            if (request.TryGetParameter("filter", out string filterStr) && byte.TryParse(filterStr, out byte filterB) && Enum.IsDefined(typeof(ProductsFilter), filterB)) filter = (ProductsFilter)filterB;
            if (request.TryGetParameter("owned", out string ownedStr) && ownedStr == "1") onlyOwned = true;
            if (request.TryGetParameter("extended", out string ext)) extended = ext == "1";

            var data = await Products.GetFilteredAsync(mid, onlyOwned, filter, extended);
            var products = data.Item1;
            var members = data.Item2;

            object response = null;
            if (extended) {
                response = new APIResponse<APIListWithMembers<ProductDTO>>(new APIListWithMembers<ProductDTO>(products, products.Count) { Members = members });
            } else {
                response = new APIResponse<APIList<ProductDTO>>(new APIList<ProductDTO>(products, products.Count));
            }

            return Results.Json(response);
        }

        public static async Task<IResult> GetCardAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();

            uint productId = request.ValidateAndGetUIntValue("product_id");

            var data = await Products.GetProductWithReportsCountersAsync(productId);
            var product = data.Item1;
            var count = data.Item2;
            var openCount = data.Item3;
            var inProcessCount = data.Item4;
            var fixedCount = data.Item5;
            var members = data.Item6;

            return Results.Json(new APIResponse<ProductCardDTO>(new ProductCardDTO { 
                Product = product,
                ReportsCount = count,
                OpenReportsCount = openCount,
                InProcessReportsCount = inProcessCount,
                FixedReportsCount = fixedCount,
                Members = members
            }));
        }

        public static async Task<IResult> SetAsFinishedAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();

            uint productId = request.ValidateAndGetUIntValue("product_id");

            bool result = await Products.SetAsFinishedAsync(mid, productId);
            return Results.Json(new APIResponse<bool>(result));
        }
    }
}