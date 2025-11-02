using System.ComponentModel.DataAnnotations;

namespace AMESA_be.common.DTOs.LotteryDevice
{
    public class ExecuteLotteryRequestDto
    {
        [Required]
        public long LotteryId { get; set; }
        public bool IsDryRun { get; set; } = false;
    }
}