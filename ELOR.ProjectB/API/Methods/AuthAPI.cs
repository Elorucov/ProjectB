using ELOR.ProjectB.API.DTO;
using ELOR.ProjectB.Core;
using ELOR.ProjectB.Core.Exceptions;

namespace ELOR.ProjectB.API.Methods {
    public static class AuthAPI {
        public static async Task<IResult> SignInAsync(HttpRequest request) {
            if (!request.TryGetParameter("login", out string login)) throw new InvalidParameterException("login is empty");
            if (!request.TryGetParameter("password", out string password)) throw new InvalidParameterException("password is empty");

            var token = await Authentication.GetAccessTokenForMemberAsync(login, password);

            return Results.Json(new APIResponse<AuthenticationResponse>(new AuthenticationResponse {
                MemberId = token.Key,
                AccessToken = token.Value,
                ExpiresIn = Authentication.ACCESS_TOKEN_EXPIRES_IN
            }));
        }

        public static async Task<IResult> SignUpAsync(HttpRequest request) {
            if (!request.TryGetParameter("invite_code", out string inviteCode)) throw new InvalidParameterException("invite_code is empty");

            if (!request.TryGetParameter("password", out string password)) throw new InvalidParameterException("password is empty");
            if (password.Length < 6) throw new InvalidParameterException("password length must be greater than 6");

            if (!request.TryGetParameter("first_name", out string firstName)) throw new InvalidParameterException("first_name is empty");
            if (firstName.Length > 50) throw new InvalidParameterException("user_name length must be less than 50");
            if (firstName.Length < 2) throw new InvalidParameterException("first_name length must be greater than 2");

            if (!request.TryGetParameter("last_name", out string lastName)) throw new InvalidParameterException("last_name is empty");
            if (lastName.Length > 50) throw new InvalidParameterException("user_name length must be less than 50");
            if (lastName.Length < 2) throw new InvalidParameterException("last_name length must be greater than 2");

            var data = await Invites.RegisterUserByCodeAsync(inviteCode, password, firstName, lastName);

            return Results.Json(new APIResponse<SignUpResult>(new SignUpResult {
                MemberId = data.Item1,
                Login = data.Item2
            }));
        }
    }
}