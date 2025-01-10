namespace ELOR.ProjectB.Core.Exceptions {
    public class AuthorizationFailedException : ServerException {
        public AuthorizationFailedException() : base(5, "Member authorization failed") {
            HTTPCode = 403;
        }

        public AuthorizationFailedException(string message) : base(5, "Member authorization failed", message) {
            HTTPCode = 403;
        }
    }
}
