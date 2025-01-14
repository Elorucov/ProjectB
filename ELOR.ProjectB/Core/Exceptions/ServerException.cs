using System.Collections.ObjectModel;

namespace ELOR.ProjectB.Core.Exceptions {
    public class ServerException : ApplicationException {
        public ushort Code { get; private set; }
        public ushort HTTPCode { get; protected set; } = 500;
        public string AdditionalInfo { get; private set; }

        public ServerException(ushort code) : base(DefinedErrors[code]) {
            Code = code;
        }

        public ServerException(ushort code, string message) : base(message ?? DefinedErrors[code]) { 
            Code = code;
        }

        public ServerException(ushort code, string message, string additional) : base(message ?? DefinedErrors[code]) {
            Code = code;
            AdditionalInfo = additional;
        }

        public static ReadOnlyDictionary<ushort, string> DefinedErrors = new ReadOnlyDictionary<ushort, string>(new Dictionary<ushort, string> {
            { 1, "Internal server error" },
            { 2, "Not implemented" },
            { 3, "Unknown method passed" },
            { 4, "Login or password is incorrect" },
            { 5, "Member authorization failed" },
            { 10, "One of the parameters specified was missing or invalid" },
            { 11, "Not found" },
            { 12, "Already exist" },
            { 15, "Access denied" },
            { 16, "Permission to perform this action is denied" },
            { 20, "Testing of this product is over" },
            { 40, "Can't change the report status to a value you passed" },
            { 41, "This status requires a comment" },
            { 42, "Can't change the report severity to a value you passed" },
            { 43, "You cannot change the report severity because the product owner has set it himself" },
        });
    }
}
