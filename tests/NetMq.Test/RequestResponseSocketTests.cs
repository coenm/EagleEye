namespace NetMq.Test
{
    using FluentAssertions;

    using NetMQ;
    using NetMQ.Sockets;

    using Xunit;
    using Xunit.Categories;

    public class RequestResponseSocketTests
    {
        [Fact]
        [Exploratory]
        public void RequestResponseSocketTest()
        {
            // arrange
            const string address = "inproc://inproc-address";

            // act
            using (var server = new ResponseSocket())
            using (var client = new RequestSocket())
            {
                server.Bind(address);
                client.Connect(address);

                client.SendFrame("Hello");
                var receivedFromClient = server.ReceiveFrameString();

                server.SendFrame("Hi Back");
                var receivedFromServer = client.ReceiveFrameString();

                // assert
                receivedFromClient.Should().Be("Hello");
                receivedFromServer.Should().Be("Hi Back");
            }
        }
    }
}
