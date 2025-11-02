
namespace AMESA_be.common.DTOs.Authentication
{
    public class EndpointDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public bool IsManaged { get; set; }
        public bool IsActive { get; set; }
    }
}
