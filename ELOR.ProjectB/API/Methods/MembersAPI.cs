using ELOR.ProjectB.API.DTO;
using ELOR.ProjectB.Core;

namespace ELOR.ProjectB.API.Methods {
    public class MembersAPI {
        public static async Task<IResult> GetCardAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();
            uint memberId = 0;

            if (request.TryGetParameter("member_id", out string midStr)) uint.TryParse(midStr, out memberId);
            if (memberId == 0) memberId = mid;

            var data = await Members.GetByIdAsync(memberId);
            var member = data.Item1;
            var invitedById = data.Item2;
            var invitedByUserName = data.Item3;

            var data2 = await Products.GetCreatedReportCountByMemberPerProductsAsync(mid, memberId);
            var products = data2.Select(d => d.Item1).ToList();

            List<ReportCountByProductDTO> counters = new List<ReportCountByProductDTO>();
            foreach (var item in data2) counters.Add(new ReportCountByProductDTO { 
                ProductId = item.Item1.Id,
                Count = item.Item2
            });

            var response = new APIResponse<MemberCardDTO>(new MemberCardDTO {
                Member = member,
                InvitedBy = invitedById,
                InvitedByUserName = invitedByUserName,
                ReportsCountPerProduct = counters,
                Products = products
            });

            return Results.Json(response);
        }
    }
}
