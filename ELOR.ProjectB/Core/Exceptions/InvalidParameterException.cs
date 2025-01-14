namespace ELOR.ProjectB.Core.Exceptions {
    public class InvalidParameterException : ServerException {
        public InvalidParameterException() : base(10) {
            HTTPCode = 400;
        }

        public InvalidParameterException(string message) : base(10, null, message) {
            HTTPCode = 400;
        }
    }
}