using Application.Common.Exceptions;
using Application.SampleEntities.Create;
using FluentAssertions;

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
    }
}