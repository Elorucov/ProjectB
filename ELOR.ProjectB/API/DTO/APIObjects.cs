namespace ELOR.ProjectB.API.DTO {
    public class APIError {
        public int Code { get; private set; }
        public string Message { get; private set; }

        internal APIError(int code, string message) {
            Code = code;
            Message = message;
        }
    }

    public class APIResponse<T> {
        public T? Response { get; private set; }
        public APIError Error { get; private set; }

        public APIResponse(T resp) {
            Response = resp;
        }

        public APIResponse() {
            Response = default;
        }

        public static APIResponse<T> GetForError(APIError error) {
            return new APIResponse<T> { Error = error };
        }
    }

    public class APIList<T> {
        public int Count { get; private set; }

        public List<T> Items { get; private set; }

        public APIList(List<T> items, int count) {
            Items = items;
            Count = count;
        }
    }

    public class ServerInfo {
        public int Version { get; set; }
        public uint? AuthorizedMemberId { get; set; }
        public string AuthErrorReason { get; set; }
    }

    public class AuthenticationResponse {
        public uint MemberId { get; init; }
        public string AccessToken { get; init; }
        public uint ExpiresIn { get; init; }
    }
}