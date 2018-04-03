namespace SearchEngine.Lucene.Bootstrap.Test
{
    using System;
    using System.Linq;

    using FluentAssertions;

    using SearchEngine.Interface.Commands;
    using SearchEngine.Interface.Queries;

    using SimpleInjector;

    using Xunit;

    using Sut = SearchEngineLuceneBootstrapper;

    public class SearchEngineLuceneBootstrapperTest
    {
        [Fact]
        public void GetCommandTypes_ShouldReturnExpectedCommandsTests()
        {
            // arrange

            // act
            var result = Sut.GetCommandTypes();

            // assert
            result.Should().BeEquivalentTo(typeof(UpdateIndexCommand));
        }

        [Fact]
        public void GetQueryTypess_ShouldReturnExpectedQueriesTests()
        {
            // arrange
            var expectedQueryTypes = new[] { typeof(WildcardSearchQuery) };

            // act
            var result = Sut.GetQueryTypes();

            // assert
            result.Select(x => x.QueryType).Should().BeEquivalentTo(expectedQueryTypes);
        }

        [Fact]
        public void Bootstrap_ShouldRegisterWithoutExceptionsTest()
        {
            // arrange
            var container = new Container();

            // act
            Sut.Bootstrap(container);
            Action act = () => container.Verify();

            // assert
            act.Should().NotThrow();

        }
    }
}
