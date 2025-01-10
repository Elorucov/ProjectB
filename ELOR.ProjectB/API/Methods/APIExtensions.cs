using ELOR.ProjectB.Core.Exceptions;
using ELOR.ProjectB.API.DTO;
using ELOR.ProjectB.Core;

namespace ELOR.ProjectB.API.Methods {
    public static class APIExtensions {
        public static bool TryGetParameter(this HttpRequest request, string key, out string value) {
            value = null;

            IFormCollection form = null;
            var query = request.Query;

            try {
                form = request.Form;
            } catch (InvalidOperationException) { // if client doesn't sent "multipart/form-data" or "x-www-form-urlencoded"

            }

            if (form != null && form.ContainsKey(key)) {
                value = request.Form[key];
                return true;
            } else if (query != null && query.ContainsKey(key)) {
                value = query[key];
                return true;
            }
            return false;
        }

        public static uint EnsureAuthorized(this HttpRequest request) {
            return Authentication.CheckAccessToken(request);
        }

        public static APIError ToAPIObject(this Exception ex, out ushort httpCode) {
            httpCode = 500;
            if (ex is ServerException srvex) {
                httpCode = srvex.HTTPCode;
                string message = string.IsNullOrEmpty(srvex.AdditionalInfo) ? srvex.Message : srvex.Message + ": " + srvex.AdditionalInfo;
                return new APIError(srvex.Code, message);
            } else if (ex is NotImplementedException niex) {
                return new APIError(2, "Not implemented");
            } else {
                return new APIError(1, "Internal server error: " + ex.Message);
            }
        }
    }
}
