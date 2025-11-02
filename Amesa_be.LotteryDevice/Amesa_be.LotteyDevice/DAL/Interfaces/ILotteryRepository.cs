using AMESA_be.LotteryDevice.DTOs;

namespace AMESA_be.LotteryDevice.DAL.Interfaces
{
    public interface ILotteryRepository
    {
        Task<Lottery> GetLotteryByIdAsync(long lotteryId, bool isDryRun, CancellationToken cancellationToken);
        Task<List<LotteryUser>> GetLotteryParticipantsByLotteryIdAsync(long lotteryId, bool isDryRun, CancellationToken cancellationToken);
        Task UpdateLotteryStatusAsync(long lotteryId, int status, bool isDryRun, CancellationToken cancellationToken);
        Task SaveLotteryWinnersAsync(long lotteryId, long[] winners, bool isDryRun, CancellationToken cancellationToken);
    }
}