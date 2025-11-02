using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMESA_be.common.Models
{
    [Table("LotteryUsers")]
    public class LotteryUser
    {
        [Key]
        public long Id { get; set; }
        public long LotteryId { get; set; }
        public long UserId { get; set; }
        public long? RecommendedBy { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}