namespace ELOR.ProjectB.API.DTO {
    public class ReportDTO {
        public uint Id { get; init; }
        public uint ProductId { get; init; }
        public uint CreatorId { get; init; }
        public long CreationTime { get; init; }
        public byte Severity { get; init; }
        public byte ProblemType { get; init; }
        public string Title { get; init; }
        public string Steps { get; init; }
        public string Actual { get; init; }
        public string Expected { get; init; }
    }
}
