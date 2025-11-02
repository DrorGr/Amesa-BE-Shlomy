using AMESA_be.LotteryDevice.DAL.Interfaces;
using AMESA_be.LotteryDevice.Data;
using AMESA_be.LotteryDevice.DTOs;
using AMESA_be.LotteryDevice.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AMESA_BE.LotteryService.DAL.Implementations
{
    public class LotteryRepository : ILotteryRepository
    {
        private readonly LotteryDbContext _context;
        private readonly ILogger<LotteryRepository> _logger;
        public LotteryRepository(LotteryDbContext context, ILogger<LotteryRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        public async Task<Lottery?> GetLotteryByIdAsync(long lotteryId, bool isDryRun, CancellationToken cancellationToken)
        {
            if(!isDryRun)
            {
                return await _context.Lotteries.FirstOrDefaultAsync(l => l.Id == lotteryId, cancellationToken);
            }

            _logger.LogInformation("isDryRun is true, returning mock data");
            return new Lottery { Id = lotteryId, Status = (int)LotteryStatus.Created, CurrentParticipants = 10, ParticipantAmount = 10 };
        }

        public async Task<List<LotteryUser>> GetLotteryParticipantsByLotteryIdAsync(long lotteryId, bool isDryRun, CancellationToken cancellationToken)
        {
            if (isDryRun) return new List<LotteryUser> { /* mock data */ };
            return await _context.LotteryUsers.Where(lu => lu.LotteryId == lotteryId && lu.IsActive).ToListAsync(cancellationToken);
        }

        public async Task UpdateLotteryStatusAsync(long lotteryId, int status, bool isDryRun, CancellationToken cancellationToken)
        {
            if (isDryRun) return;
            var lottery = await _context.Lotteries.FindAsync(new object[] { lotteryId }, cancellationToken);
            if (lottery != null)
            {
                lottery.Status = status;
                lottery.LastUpdateDate = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task SaveLotteryWinnersAsync(long lotteryId, long[] winners, bool isDryRun, CancellationToken cancellationToken)
        {
            if (isDryRun) return;
            var lotteryWinners = new LotteryWinners { LotteryId = lotteryId, Winners = winners.Select(w => w.ToString()).ToArray(), Status = (int)LotteryStatus.Executed, ExecutedOn = DateTime.UtcNow };
            await _context.LotteryWinners.AddAsync(lotteryWinners, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}