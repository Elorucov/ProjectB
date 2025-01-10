namespace ELOR.ProjectB.Core.Exceptions {
    public class PermissionException : ServerException {
        public PermissionException() : base(16, "Permission to perform this action is denied") {
            HTTPCode = 403;
        }

        public PermissionException(string message) : base(16, "Permission to perform this action is denied", message) {
            HTTPCode = 403;
        }
    }
}
