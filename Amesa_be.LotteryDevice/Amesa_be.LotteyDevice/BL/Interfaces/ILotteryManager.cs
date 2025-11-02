using AMESA_be.LotteryDevice.DTOs;
using AMESA_BELotteryDevice.Models;
using AMESA_BE.Models;

namespace AMESA_be.LotteryDevice.BL.Interfaces
{ public interface ILotteryManager
    {
        Task<GeneralActionResponse<LotteryDto>> PrepareLotteryAsync(long lotteryId, bool automateLottery, bool isDryRun, CancellationToken cancellationToken);
        Task<GeneralActionResponse<LotteryWinnersDto>> ExecuteLotteryAsync(long lotteryId, bool isDryRun, CancellationToken cancellationToken);
    }
}