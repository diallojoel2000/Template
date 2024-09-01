using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.SampleEntities.Create;
using FluentAssertions;
using Moq;

namespace Application.FunctionalTests
{
    public class UnitTest1:Testing
    {
        [Fact]
        public async Task ShouldReturnValidationException()
        {
            var command = new CreateSampleRequest
            {
                Name = "",
                Description = "Test Description"
            };
            await FluentActions.Invoking(() =>
           SendAsync(command)).Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task ShouldReturnValidId()
        {
            var command = new CreateSampleRequest
            {
                Name = "Sample Name",
                Description = "Test Description"
            };
            
            var response = await SendAsync(command);

            response.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task ShouldNotReturnValidationException()
        {
            //var mockUser = Mock.Of<IUser>(u => u.Id == "XXXXXX" && u.Username == "TestUser");
            //SetCurrentUser(mockUser);

            var mockUser = new Mock<IUser>();
            mockUser.Setup(m => m.Id).Returns("AnotherId");
            mockUser.Setup(m => m.Username).Returns("TestUser");

            var command = new CreateSampleRequest
            {
                Name = "Sample Name",
                Description = "Test Description"
            };
            await FluentActions.Invoking(() =>
           SendAsync(command)).Should().NotThrowAsync<ValidationException>();
        }
    }
}