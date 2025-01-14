namespace ELOR.ProjectB.Core.Exceptions {
    public class PermissionException : ServerException {
        public PermissionException() : base(16) {
            HTTPCode = 403;
        }

        public PermissionException(string message) : base(16, null, message) {
            HTTPCode = 403;
        }
    }
}
