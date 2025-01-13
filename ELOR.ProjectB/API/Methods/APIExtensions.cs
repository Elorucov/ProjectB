using ELOR.ProjectB.Core.Exceptions;
using ELOR.ProjectB.API.DTO;
using ELOR.ProjectB.Core;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Xml.Linq;

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

        public static byte ValidateAndGetByteConstValue(this HttpRequest request, string paramName, byte firstConst, byte lastConst) {
            if (!request.TryGetParameter(paramName, out string valueStr)) throw new InvalidParameterException($"{paramName} is missing");
            if (!byte.TryParse(valueStr, out byte value)) throw new InvalidParameterException($"{paramName}'s value is invalid");
            if (value < firstConst || value > lastConst) throw new InvalidParameterException($"{paramName}'s value should be between {firstConst} and {lastConst}");
            return value;
        }

        public static byte ValidateAndGetByteConstValue(this HttpRequest request, string paramName, byte[] supportedValues) {
            if (!request.TryGetParameter(paramName, out string valueStr)) throw new InvalidParameterException($"{paramName} is missing");
            if (!byte.TryParse(valueStr, out byte value)) throw new InvalidParameterException($"{paramName}'s value is invalid");
            if (!supportedValues.Contains(value)) throw new InvalidParameterException($"{paramName}'s value should be one of these values: {string.Join(", ", supportedValues)}");
            return value;
        }
    }
}
