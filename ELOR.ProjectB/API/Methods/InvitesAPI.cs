using ELOR.ProjectB.Core;
using ELOR.ProjectB.API.DTO;

namespace ELOR.ProjectB.API.Methods {
    public class InvitesAPI {
        public static async Task<IResult> CreateAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();

            string userName = request.ValidateAndGetValue("user_name", 2, 32);

            string code = await Invites.CreateAsync(mid, userName);
            return Results.Json(new APIResponse<string>(code));
        }

        public static async Task<IResult> GetAsync(HttpRequest request) {
            uint mid = request.EnsureAuthorized();

            var list = await Invites.GetInvitesAsync(mid);
            return Results.Json(new APIResponse<APIList<InviteDTO>>(new APIList<InviteDTO>(list, list.Count)));
        }
    }
}