namespace ELOR.ProjectB.Core.Exceptions {
    public class AccessException : ServerException {
        public AccessException() : base(15, "Access denied") {
            HTTPCode = 403;
        }

        public AccessException(string message) : base(15, "Access denied", message) {
            HTTPCode = 403;
        }
    }
}
