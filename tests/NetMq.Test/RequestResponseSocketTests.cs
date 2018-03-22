namespace NetMq.Test
{
    using FluentAssertions;

    using NetMQ;
    using NetMQ.Sockets;

    using Xunit;
    using Xunit.Categories;

    public class RequestResponseSocketTests
    {
        [Fact, Exploratory]
        public void RequestResponseSocketTest()
        {
            // arrange
            const string ADDRESS = "inproc://inproc-address";

            // act
            using (var server = new ResponseSocket())
            using (var client = new RequestSocket())
            {
                server.Bind(ADDRESS);
                client.Connect(ADDRESS);

                client.SendFrame("Hello");
                var reveivedFromClient = server.ReceiveFrameString();

                server.SendFrame("Hi Back");
                var receivedFromServer = client.ReceiveFrameString();

                // assert
                reveivedFromClient.Should().Be("Hello");
                receivedFromServer.Should().Be("Hi Back");
            }
        }
    }
}
