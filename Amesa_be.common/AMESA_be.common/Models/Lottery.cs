using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMESA_be.common.Models
{
    [Table("Lotteries")]
    public class Lottery
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public int ParticipantAmount { get; set; }
        public int CurrentParticipants { get; set; }
        public long Tenant { get; set; }
        public int Status { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public long UpdatedBy { get; set; }
    }
}