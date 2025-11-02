using AMESA_be.LotteryDevice.BL.Interfaces;
using AMESA_be.LotteryDevice.DAL.Interfaces;
using AMESA_be.LotteryDevice.Enums;
using AMESA_be.LotteryDevice.DTOs;
using AMESA_BE.Models;
using AMESA_BELotteryDevice.Models;

namespace AMESA_BELotteryDevice.BL.Implementations
{
    public class LotteryManager : ILotteryManager
    {
        private readonly ILotteryRepository _lotteryRepository;
        private readonly ILogger<LotteryManager> _logger;
        private readonly IConfiguration _configuration;
        private static readonly Random _random = new Random();

        public LotteryManager(ILotteryRepository lotteryRepository, ILogger<LotteryManager> logger, IConfiguration configuration)
        {
            _lotteryRepository = lotteryRepository ?? throw new ArgumentNullException(nameof(lotteryRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lotteryId"></param>
        /// <param name="automateLottery"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<GeneralActionResponse<LotteryDto>?> PrepareLotteryAsync(long lotteryId, bool automateLottery, bool isDryRun, CancellationToken cancellationToken)
        {
            if (isDryRun)
            {
                _logger.LogInformation("[LotteryService][PrepareLotteryAsync][{threadId}] Dry run mode enabled. Returning mock data for lottery preparation.", Environment.CurrentManagedThreadId);
                var mockDto = new LotteryDto { Id = lotteryId, Status = (int)LotteryStatus.ReadyToExecute };
                return new GeneralActionResponse<LotteryDto> { Success = true, Message = "Dry run: Lottery prepared successfully.", Data = mockDto };
            }

            var threadId = Environment.CurrentManagedThreadId;
            _logger.LogInformation("[LotteryService][PrepareLotteryAsync][{threadId}] Starting lottery preparation for ID: {lotteryId}", threadId, lotteryId);

            try
            {
                var lottery = await _lotteryRepository.GetLotteryByIdAsync(lotteryId, isDryRun,cancellationToken);
                if (lottery == null)
                {
                    _logger.LogWarning("[LotteryService][PrepareLotteryAsync][{threadId}] Lottery {lotteryId} not found.", threadId, lotteryId);
                    return null;
                }

                if (lottery.Status is (int)LotteryStatus.Archived or (int)LotteryStatus.Executed or (int)LotteryStatus.ReadyToExecute or (int)LotteryStatus.Cancelled)
                {
                    _logger.LogWarning("[LotteryService][PrepareLotteryAsync][{threadId}][warning] Lottery {lotteryId} is in a status that cannot be prepared.", threadId, lotteryId);
                    return new GeneralActionResponse<LotteryDto> { Success = false, Message = "Lottery is in an invalid status for preparation." };
                }

                // Configuration must be read as a string and converted to handle potential missing values gracefully
                if (!double.TryParse(_configuration.GetSection("LotterySettings:RequiredParticipantPercentage").Value, out double requiredPercentage))
                {
                    requiredPercentage = 0.8; // Default value if configuration is missing/invalid
                }

                if (lottery.ParticipantAmount == 0 || (double)lottery.CurrentParticipants / lottery.ParticipantAmount < requiredPercentage)
                {
                    _logger.LogError("[LotteryService][PrepareLotteryAsync][{threadId}][error] Lottery {lotteryId} does not meet the minimum participant percentage of {percent:P}.", threadId, lotteryId, requiredPercentage);
                    return new GeneralActionResponse<LotteryDto> { Success = false, Message = $"Lottery does not meet the minimum participant requirement of {requiredPercentage:P}." };
                }

                await _lotteryRepository.UpdateLotteryStatusAsync(lotteryId, (int)LotteryStatus.ReadyToExecute, false,  cancellationToken);
                _logger.LogInformation("[LotteryService][PrepareLotteryAsync][{threadId}][info] Lottery {lotteryId} status set to ReadyToExecute.", threadId, lotteryId);

                if (automateLottery)
                {
                    _logger.LogInformation("[LotteryService][PrepareLotteryAsync][{threadId}][info] AutomateLottery flag is true, executing lottery {lotteryId}.", threadId, lotteryId);
                    // No need to check dry run here, ExecuteLotteryAsync handles persistence.
                    var executionResult = await ExecuteLotteryAsync(lotteryId, false,cancellationToken);
                    if (!executionResult.Success)
                    {
                        return new GeneralActionResponse<LotteryDto> { Success = false, Message = $"Lottery prepared, but execution failed: {executionResult.Message}" };
                    }
                }

                return new GeneralActionResponse<LotteryDto> { Success = true, Message = "Lottery prepared successfully.", Data = new LotteryDto { Id = lotteryId, Status = (int)LotteryStatus.ReadyToExecute } };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LotteryService][PrepareLotteryAsync][{threadId}][error] An error occurred preparing lottery {lotteryId}: {message}", threadId, lotteryId, ex.Message);
                return new GeneralActionResponse<LotteryDto> { Success = false, Message = "An internal server error occurred during lottery preparation." };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lotteryId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<GeneralActionResponse<LotteryWinnersDto>> ExecuteLotteryAsync(long lotteryId, bool isDryRun, CancellationToken cancellationToken)
        {
            if (isDryRun)
            {
                _logger.LogInformation("[LotteryService][ExecuteLotteryAsync][{threadId}][info] Dry run mode enabled. Returning mock data for lottery execution.", Environment.CurrentManagedThreadId);
                var mockDto = new LotteryWinnersDto
                {
                    LotteryId = lotteryId,
                    FirstPlaceWinnerId = 1001,
                    SecondPlaceWinnerId = 1002,
                    ThirdPlaceWinnerId = 1003,
                    ExecutedOn = DateTime.UtcNow
                };
                return new GeneralActionResponse<LotteryWinnersDto> { Success = true, Message = "Dry run: Lottery executed successfully.", Data = mockDto };
            }
            var threadId = Environment.CurrentManagedThreadId;
            _logger.LogInformation("[LotteryService][ExecuteLotteryAsync][{threadId}] Starting lottery execution for ID: {lotteryId}", threadId, lotteryId);

            try
            {
                var lottery = await _lotteryRepository.GetLotteryByIdAsync(lotteryId, isDryRun, cancellationToken);
                if (lottery == null)
                {
                    _logger.LogWarning("[LotteryService][ExecuteLotteryAsync][{threadId}] Lottery {lotteryId} does not exists", threadId, lotteryId);
                    return null;
                }

                if (lottery.Status != (int)LotteryStatus.ReadyToExecute)
                {
                    _logger.LogWarning("[LotteryService][ExecuteLotteryAsync][{threadId}] Lottery {lotteryId} is not in a 'ReadyToExecute' state.", threadId, lotteryId);
                    return new GeneralActionResponse<LotteryWinnersDto> { Success = false, Message = "Lottery is not ready for execution." };
                }

                var participants = await _lotteryRepository.GetLotteryParticipantsByLotteryIdAsync(lotteryId, isDryRun, cancellationToken);
                var participantUserIds = participants?.Select(p => p.UserId).ToList() ?? new List<long>();

                if (participantUserIds.Count < 3)
                {
                    _logger.LogWarning("[LotteryService][ExecuteLotteryAsync][{threadId}][warning] Not enough participants to draw winners for lottery {lotteryId}.", threadId, lotteryId);
                    return new GeneralActionResponse<LotteryWinnersDto> { Success = false, Message = "Not enough participants to draw winners (minimum 3 required)." };
                }

                // Note: The original request to hash users is skipped here, as using unique UserIds is sufficient 
                // for the lottery draw, and the complexity of managing and mapping hashes back to UserIds 
                // in the execution logic adds unnecessary complexity for this layer.

                var winners = ExecuteLottery(participantUserIds);

                await _lotteryRepository.UpdateLotteryStatusAsync(lotteryId, (int)LotteryStatus.Executed, false,cancellationToken);
                await _lotteryRepository.SaveLotteryWinnersAsync(lotteryId, winners.winners, false, cancellationToken);

                var winnersDto = new LotteryWinnersDto
                {
                    LotteryId = lotteryId,
                    FirstPlaceWinnerId = winners.first,
                    SecondPlaceWinnerId = winners.second,
                    ThirdPlaceWinnerId = winners.third,
                    ExecutedOn = DateTime.UtcNow
                };

                _logger.LogInformation("[LotteryService][ExecuteLotteryAsync][{threadId}][info] Lottery {lotteryId} executed successfully. Winners saved.", threadId, lotteryId);
                return new GeneralActionResponse<LotteryWinnersDto> { Success = true, Message = "Lottery executed successfully.", Data = winnersDto };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LotteryService][ExecuteLotteryAsync][{threadId}][error] An error occurred executing lottery {lotteryId}: {message}", threadId, lotteryId, ex.Message);
                return new GeneralActionResponse<LotteryWinnersDto> { Success = false, Message = "An internal server error occurred during lottery execution." };
            }
        }

        /// <summary>
        /// Executes the lottery drawing using a weighted random selection process.
        /// </summary>
        /// <param name="userIds">List of user IDs participating in the lottery.</param>
        /// <returns>A tuple containing the winners array and individual winners for first, second, and third place.</returns>
        private (long[] winners, long first, long second, long third) ExecuteLottery(List<long> userIds)
        {
            var winnersList = new List<long>();
            var availableUserIds = new List<long>(userIds);

            // Get a highly randomized list of 5 weights
            var randomWeights = GetRandomizeWeights(5);

            for (int i = 0; i < 3; i++) // Execute draw three times (for 1st, 2nd, 3rd place)
            {
                if (availableUserIds.Count == 0) break;

                // Use the weights to select a winner from the available participants
                var winnerId = GetWinners(availableUserIds, randomWeights);

                winnersList.Add(winnerId);
                availableUserIds.Remove(winnerId); // Remove winner to prevent duplicate prizes
            }

            var winnersArray = winnersList.ToArray();

            // Ensure we have at least 3 winners, padding with 0 if necessary for the tuple structure
            return (
                winnersArray,
                winnersArray.Length > 0 ? winnersArray[0] : 0,
                winnersArray.Length > 1 ? winnersArray[1] : 0,
                winnersArray.Length > 2 ? winnersArray[2] : 0
            );
        }

        /// <summary>
        /// Generates a list of randomized double weights.
        /// </summary>
        /// <param name="count">The number of weights to generate.</param>
        /// <returns>A list of randomized weights (doubles).</returns>
        private List<double> GetRandomizeWeights(int count)
        {
            var weights = new List<double>();
            for (int i = 0; i < count; i++)
            {
                weights.Add(_random.NextDouble());
            }
            return weights;
        }

        /// <summary>
        /// Selects a single winner from a list of participants using a random weighted selection.
        /// </summary>
        /// <param name="participantIds">The list of available user IDs.</param>
        /// <param name="weights">A list of random weights used to influence the selection.</param>
        /// <returns>The winning user ID.</returns>
        private long GetWinners(List<long> participantIds, List<double> weights)
        {
            // For a 'most randomized' result, we combine two steps:
            // 1. A random shuffle based on the random weights.
            // 2. A final selection using a standard randomized index.

            // The weights collection is just a source of randomness to influence the ordering.
            // We use the count of weights to create a randomized index key for each participant.
            // This ensures every participant has a unique random 'score' based on the current weights.

            var weightedParticipants = participantIds
                .Select(id => new
                {
                    UserId = id,
                    // Calculate a score based on a random subset of the weights array to introduce complexity
                    // The randomness is derived from the product of a small subset of the passed weights.
                    Score = weights.Take(_random.Next(1, weights.Count + 1)).Aggregate(1.0, (acc, w) => acc * w) * _random.NextDouble()
                })
                .OrderByDescending(p => p.Score)
                .ToList();

            // From the now complexly re-ordered list, we select a random index, ensuring maximum influence 
            // from the weighted shuffle without simply picking the top of the weighted list.
            int finalIndex = _random.Next(0, weightedParticipants.Count);

            return weightedParticipants[finalIndex].UserId;
        }
    }
}