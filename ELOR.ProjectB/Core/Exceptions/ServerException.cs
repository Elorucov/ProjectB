namespace ELOR.ProjectB.Core.Exceptions {
    public class ServerException : ApplicationException {
        public ushort Code { get; private set; }
        public ushort HTTPCode { get; protected set; } = 500;
        public string AdditionalInfo { get; private set; }
        public ServerException(ushort code, string message) : base (message) { 
            Code = code;
        }
        public ServerException(ushort code, string message, string additional) : base(message) {
            Code = code;
            AdditionalInfo = additional;
        }
    }
}
