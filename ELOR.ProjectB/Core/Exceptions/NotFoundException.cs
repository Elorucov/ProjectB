namespace ELOR.ProjectB.Core.Exceptions {
    public class NotFoundException : ServerException {
        public NotFoundException() : base(11) {
            HTTPCode = 404;
        }

        public NotFoundException(string message) : base(11, null, message) {
            HTTPCode = 404;
        }
    }
}