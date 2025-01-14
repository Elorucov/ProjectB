namespace ELOR.ProjectB.Core.Exceptions {
    public class AuthorizationFailedException : ServerException {
        public AuthorizationFailedException() : base(5) {
            HTTPCode = 403;
        }

        public AuthorizationFailedException(string message) : base(5, null, message) {
            HTTPCode = 403;
        }
    }
}
