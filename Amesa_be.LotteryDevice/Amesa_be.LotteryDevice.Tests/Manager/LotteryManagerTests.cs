using AMESA_be.LotteryDevice.DAL.Interfaces;
using AMESA_be.LotteryDevice.DTOs;
using AMESA_be.LotteryDevice.Enums;
using AMESA_BELotteryDevice.BL.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Amesa_be.LotteryDevice.Tests.Manager
{
    [TestFixture]
    public class LotteryManagerTests
    {
        private Mock<ILotteryRepository> _mockLotteryRepository;
        private Mock<ILogger<LotteryManager>> _mockLogger;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IConfigurationSection> _mockConfigurationSection;
        private LotteryManager _lotteryManager;

        [SetUp]
        public void Setup()
        {
            _mockLotteryRepository = new Mock<ILotteryRepository>();
            _mockLogger = new Mock<ILogger<LotteryManager>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfigurationSection = new Mock<IConfigurationSection>();

            // Mock the configuration to return a required participant percentage of 0.8
            _mockConfigurationSection.Setup(x => x.Value).Returns("0.8");
            _mockConfiguration.Setup(x => x.GetSection("LotterySettings:RequiredParticipantPercentage")).Returns(_mockConfigurationSection.Object);

            _lotteryManager = new LotteryManager(_mockLotteryRepository.Object, _mockLogger.Object, _mockConfiguration.Object);
        }

        #region PrepareLotteryAsync Tests

        [Test]
        public async Task PrepareLotteryAsync_ValidLottery_SetsStatusToReadyToExecute()
        {
            // Arrange
            var lotteryId = 1;
            var lottery = new Lottery { Id = lotteryId, Status = (int)LotteryStatus.Created, CurrentParticipants = 8, ParticipantAmount = 10 };
            _mockLotteryRepository.Setup(r => r.GetLotteryByIdAsync(lotteryId, false,It.IsAny<CancellationToken>())).ReturnsAsync(lottery);
            _mockLotteryRepository.Setup(r => r.UpdateLotteryStatusAsync(lotteryId,(int)LotteryStatus.ReadyToExecute, false, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var result = await _lotteryManager.PrepareLotteryAsync(lotteryId, false, false,CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Lottery prepared successfully.", result.Message);
            Assert.AreEqual((int)LotteryStatus.ReadyToExecute, result.Data.Status);
            _mockLotteryRepository.Verify(r => r.UpdateLotteryStatusAsync(lotteryId, (int)LotteryStatus.ReadyToExecute, false, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task PrepareLotteryAsync_InvalidStatus_ReturnsFailure()
        {
            // Arrange
            var lotteryId = 1;
            var lottery = new Lottery { Id = lotteryId, Status = (int)LotteryStatus.Executed };
            _mockLotteryRepository.Setup(r => r.GetLotteryByIdAsync(lotteryId, false, It.IsAny<CancellationToken>())).ReturnsAsync(lottery);

            // Act
            var result = await _lotteryManager.PrepareLotteryAsync(lotteryId, false, false,CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Lottery is in an invalid status for preparation.", result.Message);
            _mockLotteryRepository.Verify(r => r.UpdateLotteryStatusAsync(It.IsAny<long>(), It.IsAny<int>(), false, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task PrepareLotteryAsync_InsufficientParticipants_ReturnsFailure()
        {
            // Arrange
            var lotteryId = 1;
            var lottery = new Lottery { Id = lotteryId, Status = (int)LotteryStatus.Created, CurrentParticipants = 5, ParticipantAmount = 10 };
            _mockLotteryRepository.Setup(r => r.GetLotteryByIdAsync(lotteryId, false, It.IsAny<CancellationToken>())).ReturnsAsync(lottery);

            // Act
            var result = await _lotteryManager.PrepareLotteryAsync(lotteryId, false, false, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Lottery does not meet the minimum participant requirement of 80.00%.", result.Message);
            _mockLotteryRepository.Verify(r => r.UpdateLotteryStatusAsync(It.IsAny<long>(), It.IsAny<int>(), false, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task PrepareLotteryAsync_AutomateLotteryIsTrue_CallsExecuteLotteryAsync()
        {
            // Arrange
            var lotteryId = 1;
            var lotteryForPrepare = new Lottery { Id = lotteryId, Status = (int)LotteryStatus.Created, CurrentParticipants = 8, ParticipantAmount = 10 };
            var lotteryForExecute = new Lottery { Id = lotteryId, Status = (int)LotteryStatus.ReadyToExecute };
            var participants = new List<LotteryUser> { new() { UserId = 1 }, new() { UserId = 2 }, new() { UserId = 3 } };

            // Set up the repository's method to return different objects on subsequent calls.
            // First call for PrepareLotteryAsync will get lotteryForPrepare.
            // Second call for ExecuteLotteryAsync will get lotteryForExecute.
            _mockLotteryRepository.SetupSequence(r => r.GetLotteryByIdAsync(lotteryId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lotteryForPrepare)
                .ReturnsAsync(lotteryForExecute);

            // Mock the remaining dependencies as before.
            _mockLotteryRepository.Setup(r => r.UpdateLotteryStatusAsync(lotteryId, (int)LotteryStatus.ReadyToExecute, false,It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockLotteryRepository.Setup(r => r.GetLotteryParticipantsByLotteryIdAsync(lotteryId, false, It.IsAny<CancellationToken>())).ReturnsAsync(participants);
            _mockLotteryRepository.Setup(r => r.UpdateLotteryStatusAsync(lotteryId, (int)LotteryStatus.Executed, false, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockLotteryRepository.Setup(r => r.SaveLotteryWinnersAsync(lotteryId, It.IsAny<long[]>(), false, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var result = await _lotteryManager.PrepareLotteryAsync(lotteryId, true, false, CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success);
            _mockLotteryRepository.Verify(r => r.UpdateLotteryStatusAsync(lotteryId, (int)LotteryStatus.Executed, false, It.IsAny<CancellationToken>()), Times.Once);
            _mockLotteryRepository.Verify(r => r.SaveLotteryWinnersAsync(lotteryId, It.IsAny<long[]>(), false, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region ExecuteLotteryAsync Tests

        [Test]
        public async Task ExecuteLotteryAsync_ValidLottery_ReturnsWinnersDtoAndSavesThreeUniqueWinners()
        {
            // Arrange
            var lotteryId = 1;
            var lottery = new Lottery { Id = lotteryId, Status = (int)LotteryStatus.ReadyToExecute };
            var participants = new List<LotteryUser>
            {
                new() { UserId = 101 }, new() { UserId = 102 }, new() { UserId = 103 }, new() { UserId = 104 }, new() { UserId = 105 }
            };

            _mockLotteryRepository.Setup(r => r.GetLotteryByIdAsync(lotteryId, false,It.IsAny<CancellationToken>())).ReturnsAsync(lottery);
            _mockLotteryRepository.Setup(r => r.GetLotteryParticipantsByLotteryIdAsync(lotteryId, false,It.IsAny<CancellationToken>())).ReturnsAsync(participants);
            _mockLotteryRepository.Setup(r => r.UpdateLotteryStatusAsync(lotteryId, (int)LotteryStatus.Executed, false, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockLotteryRepository.Setup(r => r.SaveLotteryWinnersAsync(lotteryId, It.IsAny<long[]>(), false, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var result = await _lotteryManager.ExecuteLotteryAsync(lotteryId, false, CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Data);

            var winners = new List<long> { result.Data.FirstPlaceWinnerId, result.Data.SecondPlaceWinnerId, result.Data.ThirdPlaceWinnerId };

            Assert.AreEqual(3, winners.Count);
            Assert.AreEqual(3, winners.Distinct().Count(), "Winners should be unique.");

            _mockLotteryRepository.Verify(r => r.UpdateLotteryStatusAsync(lotteryId, (int)LotteryStatus.Executed, false, It.IsAny<CancellationToken>()), Times.Once);
            _mockLotteryRepository.Verify(r => r.SaveLotteryWinnersAsync(lotteryId, It.Is<long[]>(arr => arr.Length == 3), false, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ExecuteLotteryAsync_NotReadyToExecute_ReturnsFailure()
        {
            // Arrange
            var lotteryId = 1;
            var lottery = new Lottery { Id = lotteryId, Status = (int)LotteryStatus.Created };
            _mockLotteryRepository.Setup(r => r.GetLotteryByIdAsync(lotteryId, false, It.IsAny<CancellationToken>())).ReturnsAsync(lottery);

            // Act
            var result = await _lotteryManager.ExecuteLotteryAsync(lotteryId, false, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Lottery is not ready for execution.", result.Message);
            _mockLotteryRepository.Verify(r => r.GetLotteryParticipantsByLotteryIdAsync(It.IsAny<long>(), false, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task ExecuteLotteryAsync_NotEnoughParticipants_ReturnsFailure()
        {
            // Arrange
            var lotteryId = 1;
            var lottery = new Lottery { Id = lotteryId, Status = (int)LotteryStatus.ReadyToExecute };
            var participants = new List<LotteryUser> { new() { UserId = 1 }, new() { UserId = 2 } };

            _mockLotteryRepository.Setup(r => r.GetLotteryByIdAsync(lotteryId, false, It.IsAny<CancellationToken>())).ReturnsAsync(lottery);
            _mockLotteryRepository.Setup(r => r.GetLotteryParticipantsByLotteryIdAsync(lotteryId, false, It.IsAny<CancellationToken>())).ReturnsAsync(participants);

            // Act
            var result = await _lotteryManager.ExecuteLotteryAsync(lotteryId, false, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Not enough participants to draw winners (minimum 3 required).", result.Message);
            _mockLotteryRepository.Verify(r => r.UpdateLotteryStatusAsync(It.IsAny<long>(), It.IsAny<int>(), false, It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion
    }
}