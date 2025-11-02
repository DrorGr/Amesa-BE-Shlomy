using AMESA_be.LotteryDevice.Data;
using AMESA_be.LotteryDevice.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using AMESA_be.LotteryDevice.Enums;
using AMESA_BE.LotteryService.DAL.Implementations;
using AMESA_BELotteryDevice.BL.Implementations;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Amesa_be.LotteryDevice.Tests.Repository
{
    [TestFixture]
    public class LotteryRepositoryTests
    {
        private LotteryDbContext _context;
        private LotteryRepository _repository;
        private Mock<ILogger<LotteryRepository>> _mockLogger;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<LotteryDbContext>()
                .UseInMemoryDatabase(databaseName: "LotteryTestDb_" + Guid.NewGuid().ToString())
                .Options;

            var mockConfiguration = new Mock<IConfiguration>();
            _context = new LotteryDbContext(options, mockConfiguration.Object);
            _context.Database.EnsureCreated();
            _mockLogger = new Mock<ILogger<LotteryRepository>>(); 
            _repository = new LotteryRepository(_context, _mockLogger.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task GetLotteryByIdAsync_ExistingId_ReturnsLottery()
        {
            // Arrange
            var lottery = new Lottery { Id = 1, Name = "Test Lottery" };
            _context.Lotteries.Add(lottery);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetLotteryByIdAsync(1, false, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Test Lottery", result.Name);
        }

        [Test]
        public async Task GetLotteryParticipantsByLotteryIdAsync_ExistingLottery_ReturnsParticipants()
        {
            // Arrange
            _context.LotteryUsers.AddRange(new List<LotteryUser>
            {
                // Add IsActive = true to ensure the records are returned by the query
                new() { Id = 1, LotteryId = 10, UserId = 1, IsActive = true },
                new() { Id = 2, LotteryId = 10, UserId = 2, IsActive = true },
                new() { Id = 3, LotteryId = 20, UserId = 3, IsActive = true } // This participant won't be returned by the query, which is the correct behavior
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetLotteryParticipantsByLotteryIdAsync(10, false, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0].UserId);
        }

        [Test]
        public async Task UpdateLotteryStatusAsync_ExistingLottery_UpdatesStatus()
        {
            // Arrange
            var lottery = new Lottery
            {
                Id = 1,
                Status = (int)LotteryStatus.Created,
                Name = "Test Lottery" // Add a value for the required Name property
            };
            _context.Lotteries.Add(lottery);
            await _context.SaveChangesAsync();

            // Act
            await _repository.UpdateLotteryStatusAsync(1, (int)LotteryStatus.ReadyToExecute, false, CancellationToken.None);
            var updatedLottery = await _context.Lotteries.FindAsync(1L);

            // Assert
            Assert.IsNotNull(updatedLottery);
            Assert.AreEqual((int)LotteryStatus.ReadyToExecute, updatedLottery.Status);
        }

        [Test]
        public async Task SaveLotteryWinnersAsync_AddsNewRecord()
        {
            // Arrange
            var lotteryId = 1;
            var winners = new long[] { 101, 102, 103 };
            var userId = 1001;

            // Act
            await _repository.SaveLotteryWinnersAsync(lotteryId, winners, false, CancellationToken.None);

            // Assert
            var savedWinners = await _context.LotteryWinners.FirstOrDefaultAsync();
            Assert.IsNotNull(savedWinners);
            Assert.AreEqual(lotteryId, savedWinners.LotteryId);
            Assert.AreEqual(winners.Length, savedWinners.Winners.Length);
        }
    }
}