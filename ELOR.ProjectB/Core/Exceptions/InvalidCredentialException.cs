namespace ELOR.ProjectB.Core.Exceptions {
    public class InvalidCredentialException : ServerException {
        public InvalidCredentialException() : base(4) {
            HTTPCode = 403;
        }
    }
}
