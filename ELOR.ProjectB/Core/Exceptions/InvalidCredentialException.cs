namespace ELOR.ProjectB.Core.Exceptions {
    public class InvalidCredentialException : ServerException {
        public InvalidCredentialException() : base(4, "Login or password is incorrect") {
            HTTPCode = 403;
        }
    }
}
