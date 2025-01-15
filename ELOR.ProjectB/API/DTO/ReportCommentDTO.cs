namespace ELOR.ProjectB.API.DTO {
    public class ReportCommentDTO {
        public uint Id { get; init; }
        public uint ReportId { get; init; }
        public uint CreatorId { get; init; }
        public long Created { get; init; }
        public long? Updated { get; init; }
        public string Comment { get; init; }
        public EnumInfoDTO NewSeverity { get; init; }
        public EnumInfoDTO NewStatus { get; init; }
    }
}
