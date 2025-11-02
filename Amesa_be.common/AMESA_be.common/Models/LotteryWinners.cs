using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMESA_be.common.Models
{
    [Table("LotteryWinners")]
    public class LotteryWinners
    {
        [Key]
        public long Id { get; set; }
        public long LotteryId { get; set; }
        public string[] Winners { get; set; }
        public int Status { get; set; }
        public DateTime ExecutedOn { get; set; }
        public long ExecutedBy { get; set; }
    }
}