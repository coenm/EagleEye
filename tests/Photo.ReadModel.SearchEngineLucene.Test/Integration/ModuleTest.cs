namespace Photo.ReadModel.SearchEngineLucene.Test.Integration
{
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.Photo.ReadModel.SearchEngineLucene.Interface;
    using FluentAssertions;
    using Xunit;

    public class ModuleTest : IClassFixture<ModuleTestFixture>, IAsyncLifetime
    {
        private readonly IReadModel readModel;
        private readonly ModuleTestFixture fixture;

        public ModuleTest(ModuleTestFixture fixture)
        {
            this.fixture = fixture;
            readModel = fixture.ReadModel;
        }

        public async Task InitializeAsync() => await fixture.Initialize();

        public Task DisposeAsync() => Task.CompletedTask;

        [Theory]
        [ClassData(typeof(SearchScenarios))]
        public void Search(string query, ModuleTestFixture.PhotoPersonItem[] expectedResults)
        {
            // arrange
            var expectedIds = expectedResults.Select(x => x.Guid).ToArray();

            // act
            var result1 = readModel.FullSearch(query);
            var result2 = readModel.Search(query);
            var result3 = readModel.Count(query);

            // assert
            result1.Select(x => x.Id).Should().BeEquivalentTo(expectedIds);
            result2.Select(x => x.Id).Should().BeEquivalentTo(expectedIds);
            result3.Should().Be(expectedIds.Length);
        }
    }
}
