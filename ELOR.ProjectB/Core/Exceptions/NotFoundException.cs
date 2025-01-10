namespace ELOR.ProjectB.Core.Exceptions {
    public class NotFoundException : ServerException {
        public NotFoundException() : base(11, "Not found") {
            HTTPCode = 404;
        }

        public NotFoundException(string message) : base(11, "Not found", message) {
            HTTPCode = 404;
        }
    }
}