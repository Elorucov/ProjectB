namespace ELOR.ProjectB.API.DTO {
    public class InviteDTO {
        public uint Id { get; init; }
        public uint CreatorId { get; init; }
        public long Created { get; init; }
        public string UserName { get; init; }
        public string Code { get; init; }
        public uint? InvitedMemberId {  get; init; }
    }
}
