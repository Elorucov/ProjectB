namespace ELOR.ProjectB.Core.Exceptions {
    public class InvalidParameterException : ServerException {
        public InvalidParameterException() : base(10, "One of the parameters specified was missing or invalid") {
            HTTPCode = 400;
        }

        public InvalidParameterException(string message) : base(10, "One of the parameters specified was missing or invalid", message) {
            HTTPCode = 400;
        }
    }
}