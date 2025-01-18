namespace ELOR.ProjectB.API.DTO {
    public class ProductCardDTO {
        public ProductDTO Product { get; init; }
        public int ReportsCount { get; init; }
        public int OpenReportsCount { get; init; }
        public int InProcessReportsCount { get; init; }
        public int FixedReportsCount { get; init; }
        public List<MemberDTO> Members { get; init; }
    }
}
