using System.Linq;

namespace ExifToolWrapper.Test
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using FluentAssertions;

    using Xunit;

    public class ExifToolStayOpenStreamTest : IDisposable
    {
        private readonly ExifToolStayOpenStream _sut;
        private readonly List<DataCapturedArgs> _capturedEvents;

        public ExifToolStayOpenStreamTest()
        {
            _capturedEvents = new List<DataCapturedArgs>();
            _sut = new ExifToolStayOpenStream(Encoding.UTF8);
            _sut.Update += SutOnUpdate;
        }

        public void Dispose()
        {
            _sut.Update -= SutOnUpdate;
            _sut?.Dispose();
        }

        [Fact]
        public void SingleWriteShouldNotFireEvent()
        {
            // arrange
            const string msg = "dummy data without delimitor";

            // act
            WriteMessageToSut(msg);

            // assert
            _capturedEvents.Should().BeEmpty();
        }

        [Fact]
        public void ParseSingleMessage()
        {
            // arrange
            const string msg = "a b c\r\nd e f\r\n{ready0}";

            // act
            WriteMessageToSut(msg);

            // assert
            _capturedEvents.Should().HaveCount(1);
            _capturedEvents.First().Key.Should().Be("0");
            _capturedEvents.First().Data.Should().Be("a b c\r\nd e f");
        }

        [Fact]
        public void ParseTwoMessagesInSingleWrite()
        {
            // arrange
            const string msg = "a b c\r\n{ready0}\r\nd e f\r\n{ready1}\r\nxyz";

            // act
            WriteMessageToSut(msg);

            // assert
            _capturedEvents.Should().HaveCount(2);

            _capturedEvents[0].Key.Should().Be("0");
            _capturedEvents[0].Data.Should().Be("a b c");

            _capturedEvents[1].Key.Should().Be("1");
            _capturedEvents[1].Data.Should().Be("d e f");
        }

        [Fact]
        public void ParseTwoMessagesOverFourWrites()
        {
            // arrange
            const string msg1 = "a b c\r\nd e f\r\n{ready0}\r\nghi";
            const string msg2 = " jkl\r\n{re";
            const string msg3 = "ad";
            const string msg4 = "y2133}\r\n";

            // act
            WriteMessageToSut(msg1);
            WriteMessageToSut(msg2);
            WriteMessageToSut(msg3);
            WriteMessageToSut(msg4);

            // assert
            _capturedEvents.Should().HaveCount(2)
                           .And.Contain(x => x.Key == "0" && x.Data == "a b c\r\nd e f")
                           .And.Contain(x => x.Key == "2133" && x.Data == "ghi jkl");
        }

        private void WriteMessageToSut(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            _sut.Write(buffer, 0, buffer.Length);
        }

        private void SutOnUpdate(object sender, DataCapturedArgs dataCapturedArgs)
        {
            _capturedEvents.Add(dataCapturedArgs);
        }
    }
}