namespace ELOR.ProjectB.API.DTO {
    public class ReportDTO {
        public uint Id { get; init; }
        public uint ProductId { get; init; }
        public uint CreatorId { get; init; }
        public long Created { get; init; }
        public long? Updated { get; init; }
        public EnumInfoDTO Severity { get; init; }
        public EnumInfoDTO ProblemType { get; init; }
        public EnumInfoDTO Status { get; init; }
        public string Title { get; init; }
        public string Steps { get; init; }
        public string Actual { get; init; }
        public string Expected { get; init; }
    }
}
