using AMESA_be.LotteryDevice.DTOs;
using AMESA_BELotteryDevice.Models;

namespace AMESA_BELotteryDevice.Mappers
{
    public static class LotteryMapper
    {
        public static LotteryDto ToDto(Lottery lottery)
        {
            return new LotteryDto
            {
                Id = lottery.Id,
                Name = lottery.Name,
                Status = lottery.Status,
                // The Address property mapping has been removed
            };
        }
    }
}