using System.ComponentModel.DataAnnotations;

namespace AMESA_be.LotteryDevice.DTOs
{
    public class ExecuteLotteryRequestDto
    {
        [Required]
        public long LotteryId { get; set; }
        public bool IsDryRun { get; set; } = false;
    }
}