namespace ELOR.ProjectB.API.DTO {
    public class MemberDTO {
        public uint Id { get; init; }
        public string UserName { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
    }

    public class APIListWithMembers<T> : APIList<T> {
        public APIListWithMembers(List<T> items, int count) : base(items, count) { }

        public List<MemberDTO> Members { get; init; }
    }
}
