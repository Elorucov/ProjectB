namespace ELOR.ProjectB.API.DTO {
    public class EnumInfoDTO {
        public byte Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool Supported { get; private set; }

        public EnumInfoDTO(byte id, string name, string description, bool supported) {
            Id = id;
            Name = name;
            Description = description;
            Supported = supported;
        }
    }

    public class ServerEnumsDTO {
        public List<EnumInfoDTO> Severities { get; init; }
        public List<EnumInfoDTO> ProblemTypes { get; init; }
        public List<EnumInfoDTO> ReportStatuses { get; init; }
    }
}
