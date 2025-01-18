namespace ELOR.ProjectB.API.DTO {
    public class ReportCountByProductDTO {
        public uint ProductId { get; init; }
        public int Count { get; init; }

    }

    public class MemberCardDTO {
        public MemberDTO Member { get; init; }
        public uint? InvitedBy { get; init; }
        public string InvitedByUserName { get; init; }
        public List<ReportCountByProductDTO> ReportsCountPerProduct { get; init; }
        public List<ProductDTO> Products { get; init; }
    }
}
