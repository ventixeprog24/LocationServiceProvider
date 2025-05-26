using LocationServiceProvider;
using LocationServiceProvider.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace Tests.Helpers
{
    // Tester genererade till stor del med hjälp av AI

    public class CacheHandler_Tests
    {
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly CacheHandler _cacheHandler;

        public CacheHandler_Tests()
        {
            _memoryCacheMock = new Mock<IMemoryCache>();
            _cacheHandler = new CacheHandler(_memoryCacheMock.Object);
        }

        [Fact]
        public void GetFromCache_ShouldReturnData_WhenKeyExistsInCache()
        {
            // Arrange
            var cacheKey = "test_key";
            var expected = new List<Location> { new() { Id = "1", Name = "Test Location" } };

            object? outValue = expected;

            _memoryCacheMock
                .Setup(m => m.TryGetValue(cacheKey, out outValue!))
                .Returns(true);

            // Act
            var result = _cacheHandler.GetFromCache(cacheKey);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetFromCache_ShouldReturnNull_WhenKeyDoesNotExistInCache()
        {
            // Arrange
            var cacheKey = "missing_key";
            object? outValue = null;

            _memoryCacheMock
                .Setup(m => m.TryGetValue(cacheKey, out outValue!))
                .Returns(false);

            // Act
            var result = _cacheHandler.GetFromCache(cacheKey);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void SetCache_ShouldStoreDataAndReturnSameData()
        {
            // Arrange
            var cacheKey = "new_key";
            var data = new List<Location> { new() { Id = "2", Name = "New Location" } };

            var cacheEntryMock = new Mock<ICacheEntry>();
            _memoryCacheMock
                .Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntryMock.Object);

            // Act
            var result = _cacheHandler.SetCache(cacheKey, data);

            // Assert
            Assert.Equal(data, result);
            _memoryCacheMock.Verify(m => m.Remove(cacheKey), Times.Once);
        }
    }
}
