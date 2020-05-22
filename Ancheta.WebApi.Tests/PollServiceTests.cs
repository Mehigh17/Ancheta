using System;
using Ancheta.Model.Services;
using Xunit;

namespace Ancheta.WebApi.Tests
{
    public class PollServiceTests
    {

        private readonly PollService _pollService;

        public PollServiceTests()
        {
            _pollService = new PollService();
        }

        [Fact]
        public void GenerateSecretCode_WithValidLength_ShouldReturn_PassHashPair()
        {
            // Arrange
            const int length = 10;

            // Act
            var (password, hash) = _pollService.GenerateSecretCode(length);

            // Assert
            Assert.NotEmpty(password);
            Assert.NotEmpty(hash);
        }

        [Fact]
        public void GenerateScretCode_WithInvalidLength_ShouldThrow_ArgumentException()
        {
            // Arrange
            const int length = 0;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => {
                var (password, hash) = _pollService.GenerateSecretCode(length);
            });
        }

        [Fact]
        public void IsPasswordValid_WithValidPassHashPair_ShouldReturn_True()
        {
            // Arange
            const int passLength = 10;
            var (password, hash) = _pollService.GenerateSecretCode(passLength);

            // Act
            bool isValid = _pollService.IsCodeValid(password, hash);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsPasswordValid_WithInvalidPassHashPair_ShouldReturn_False()
        {
            // Arange
            const int passLength = 10;
            var (password, hash) = _pollService.GenerateSecretCode(passLength);

            // Act
            bool isValid = _pollService.IsCodeValid(password.Substring(0, passLength / 2), hash);

            // Assert
            Assert.False(isValid);
        }

    }
}
