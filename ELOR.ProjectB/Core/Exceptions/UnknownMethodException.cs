namespace ELOR.ProjectB.Core.Exceptions {
    public class UnknownMethodException : ServerException {
        public UnknownMethodException() : base(3, "Unknown method passed") {
            HTTPCode = 404;
        }
    }
}
