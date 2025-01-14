namespace ELOR.ProjectB.Core.Exceptions {
    public class FinishedProductException : ServerException {
        public FinishedProductException() : base(20) {
            HTTPCode = 400;
        }
    }
}
