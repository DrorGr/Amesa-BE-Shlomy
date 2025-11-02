using System.ComponentModel.DataAnnotations;

namespace AMESA_be.common.DTOs.LotteryDevice
{
    public class PrepareLotteryRequestDto
    {
        [Required]
        public long LotteryId { get; set; }
        public bool AutomateLottery { get; set; } = false;
    }
}