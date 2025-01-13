namespace ELOR.ProjectB.API.DTO {
    public class ProductDTO {
        public uint Id { get; init; }
        public uint OwnerId { get; init; }
        public string Name { get; init; }
        public bool IsFinished { get; init; }
    }

    public class ReportsList : APIListWithMembers<ReportDTO> {
        public ReportsList(List<ReportDTO> items, int count) : base(items, count) { }

        public List<ProductDTO> Products { get; init; }
    }
}
