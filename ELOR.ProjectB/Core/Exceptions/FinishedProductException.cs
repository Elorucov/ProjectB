namespace ELOR.ProjectB.Core.Exceptions {
    public class FinishedProductException : ServerException {
        public FinishedProductException() : base(20, "Testing of this product is over") {
            HTTPCode = 400;
        }
    }
}
