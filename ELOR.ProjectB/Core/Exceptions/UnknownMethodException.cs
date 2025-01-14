namespace ELOR.ProjectB.Core.Exceptions {
    public class UnknownMethodException : ServerException {
        public UnknownMethodException() : base(3) {
            HTTPCode = 404;
        }
    }
}
