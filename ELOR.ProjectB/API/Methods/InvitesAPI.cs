using ELOR.ProjectB.Core;
using ELOR.ProjectB.Core.Exceptions;
using ELOR.ProjectB.API.DTO;

namespace ELOR.ProjectB.API.Methods {
    public class InvitesAPI {
        public static async Task<IResult> CreateAsync(HttpRequest request) {
            await Task.Delay(1);
            uint mid = request.EnsureAuthorized();

            string userName;
            if (!request.TryGetParameter("user_name", out userName)) throw new InvalidParameterException("user_name is empty");
            if (userName.Length > 32) throw new InvalidParameterException("user_name length must be less than 32");
            if (userName.Length < 2) throw new InvalidParameterException("user_name length must be greater than 2");

            string code = await Invites.CreateAsync(mid, userName);
            return Results.Json(new APIResponse<string>(code));
        }
    }
}
