namespace ELOR.ProjectB.Core.Exceptions {
    public class AlreadyExistException : ServerException {
        public AlreadyExistException() : base(12, "Already exist") { }

        public AlreadyExistException(string message) : base(12, "Already exist", message) { }
    }
}
