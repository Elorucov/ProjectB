using ELOR.ProjectB.Core.Exceptions;
using ELOR.ProjectB.API.DTO;
using ELOR.ProjectB.Core;
using MySql.Data.MySqlClient;
using System.Text;

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

        public static bool ContainsOneOfTheseParameters(this HttpRequest request, IEnumerable<string> paramNames) {
            IFormCollection form = null;
            var query = request.Query;

            try {
                form = request.Form;
            } catch (InvalidOperationException) { // if client doesn't sent "multipart/form-data" or "x-www-form-urlencoded"

            }

            foreach (var key in paramNames) {
                if (form != null && form.ContainsKey(key)) {
                    return true;
                } else if (query != null && query.ContainsKey(key)) {
                    return true;
                }
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
            } else if (ex is MySqlException sqlex) {
                (int code, string message) = GetAPIObjectForMySQLError(sqlex);
                return new APIError(code, message);
            } else if (ex is NotImplementedException niex) {
                return new APIError(2, ServerException.DefinedErrors[2]);
            } else {
                return new APIError(1, $"{ServerException.DefinedErrors[1]}: {ex.Message}");
            }
        }

        private static (int code, string message) GetAPIObjectForMySQLError(MySqlException sqlex) {
            switch (sqlex.Number) {
                case Constants.DB_ERRNUM_NOT_FOUND: return (11, ServerException.DefinedErrors[11]);
                case Constants.DB_ERRNUM_ACCESS_DENIED: return (15, ServerException.DefinedErrors[15]);
                case Constants.DB_ERRNUM_PERMISSION_DENIED: return (16, ServerException.DefinedErrors[16]);
                case Constants.DB_ERRNUM_WRONG_STATUS: return (40, ServerException.DefinedErrors[40]);
                case Constants.DB_ERRNUM_STATUS_COMMENT_REQUIRED: return (41, ServerException.DefinedErrors[41]);
                case Constants.DB_ERRNUM_WRONG_SEVERITY: return (42, ServerException.DefinedErrors[42]);
                case Constants.DB_ERRNUM_AUTHOR_CANNOT_EDIT_REPORT: return (43, ServerException.DefinedErrors[43]);
                case Constants.DB_ERRNUM_AUTHOR_CANNOT_DELETE_REPORT: return (44, ServerException.DefinedErrors[44]);
                case Constants.DB_ERRNUM_TEST: return (1, $"{ServerException.DefinedErrors[1]}: this is a test error from MySQL");
                default: return (1, $"{ServerException.DefinedErrors[1]}: {sqlex.Message}");
            }
        }

        public static string ValidateAndGetValue(this HttpRequest request, string paramName) {
            if (!request.TryGetParameter(paramName, out string value)) throw new InvalidParameterException($"{paramName} is missing");
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidParameterException($"{paramName}'s value is empty");
            return value;
        }

        public static string ValidateAndGetValue(this HttpRequest request, string paramName, int min, int max = 255) {
            string value = request.ValidateAndGetValue(paramName);
            if (value.Length < 2) throw new InvalidParameterException($"{paramName}'s value length must be greater than {min}");
            if (value.Length > 64) throw new InvalidParameterException($"{paramName}'s value length must be less than {max}");
            return value;
        }

        public static uint ValidateAndGetUIntValue(this HttpRequest request, string paramName) {
            if (!request.TryGetParameter(paramName, out string valueStr)) throw new InvalidParameterException($"{paramName} is missing");
            if (!uint.TryParse(valueStr, out uint value) || value == 0) throw new InvalidParameterException($"{paramName}'s value is invalid (must be uint and non-zero)");
            return value;
        }

        public static byte ValidateAndGetByteConstValue(this HttpRequest request, string paramName, byte firstConst, byte lastConst, string additionalInfo = null) {
            if (!request.TryGetParameter(paramName, out string valueStr)) throw new InvalidParameterException($"{paramName} is missing");
            if (!byte.TryParse(valueStr, out byte value)) throw new InvalidParameterException($"{paramName}'s value is invalid");
            if (value < firstConst || value > lastConst) {
                StringBuilder sb = new StringBuilder();
                sb.Append($"{paramName}'s value should be between {firstConst} and {lastConst}");
                if (!string.IsNullOrEmpty(additionalInfo)) sb.Append($". {additionalInfo}");
                throw new InvalidParameterException(sb.ToString());
            }
            return value;
        }

        public static byte ValidateAndGetByteConstValue(this HttpRequest request, string paramName, byte[] supportedValues, string additionalInfo = null) {
            if (!request.TryGetParameter(paramName, out string valueStr)) throw new InvalidParameterException($"{paramName} is missing");
            if (!byte.TryParse(valueStr, out byte value)) throw new InvalidParameterException($"{paramName}'s value is invalid");
            if (!supportedValues.Contains(value)) {
                StringBuilder sb = new StringBuilder();
                sb.Append($"{paramName}'s value should be one of these values: {string.Join(", ", supportedValues)}");
                if (!string.IsNullOrEmpty(additionalInfo)) sb.Append($". {additionalInfo}");
                throw new InvalidParameterException(sb.ToString());
            }
            return value;
        }

        public static void CheckContainsOneOfTheseParametersOrThrow(this HttpRequest request, IEnumerable<string> paramNames) {
            if (!request.ContainsOneOfTheseParameters(paramNames)) throw new InvalidParameterException($"you must be pass one of these parameters: {string.Join(", ", paramNames)}");
        }
    }
}
