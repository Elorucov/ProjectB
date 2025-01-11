using ELOR.ProjectB.API.DTO;
using ELOR.ProjectB.Core;
using ELOR.ProjectB.Core.Exceptions;

namespace ELOR.ProjectB.API.Methods {
    public static class AuthAPI {
        public static async Task<IResult> SignInAsync(HttpRequest request) {
            string login = request.ValidateAndGetValue("login");
            string password = request.ValidateAndGetValue("password");

            var token = await Authentication.GetAccessTokenForMemberAsync(login, password);

            return Results.Json(new APIResponse<AuthenticationResponse>(new AuthenticationResponse {
                MemberId = token.Key,
                AccessToken = token.Value,
                ExpiresIn = Authentication.ACCESS_TOKEN_EXPIRES_IN
            }));
        }

        public static async Task<IResult> SignUpAsync(HttpRequest request) {
            string inviteCode = request.ValidateAndGetValue("invite_code");
            string password = request.ValidateAndGetValue("password", 6);

            string firstName = request.ValidateAndGetValue("first_name", 2, 50);
            string lastName = request.ValidateAndGetValue("last_name", 2, 50);

            var data = await Invites.RegisterUserByCodeAsync(inviteCode, password, firstName, lastName);

            return Results.Json(new APIResponse<SignUpResult>(new SignUpResult {
                MemberId = data.Item1,
                Login = data.Item2
            }));
        }
    }
}