namespace EagleEye.ExifToolWrapper.Test.ExifTool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using EagleEye.ExifToolWrapper.ExifTool;

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
            const string MSG = "dummy data without delimitor";

            // act
            WriteMessageToSut(MSG);

            // assert
            _capturedEvents.Should().BeEmpty();
        }

        [Fact]
        public void ParseSingleMessage()
        {
            // arrange
            const string MSG = "a b c\r\nd e f\r\n{ready0}\r\n";

            // act
            WriteMessageToSut(MSG);

            // assert
            _capturedEvents.Should().HaveCount(1);
            _capturedEvents.First().Key.Should().Be("0");
            _capturedEvents.First().Data.Should().Be("a b c\r\nd e f");
        }

        [Fact]
        public void ParseTwoMessagesInSingleWrite()
        {
            // arrange
            const string MSG = "a b c\r\n{ready0}\r\nd e f\r\n{ready1}\r\nxyz";

            // act
            WriteMessageToSut(MSG);

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
            const string MSG1 = "a b c\r\nd e f\r\n{ready0}\r\nghi";
            const string MSG2 = " jkl\r\n{re";
            const string MSG3 = "ady";
            const string MSG4 = "213";
            const string MSG5 = "3}\r\n";

            // act
            WriteMessageToSut(MSG1);
            WriteMessageToSut(MSG2);
            WriteMessageToSut(MSG3);
            WriteMessageToSut(MSG4);
            WriteMessageToSut(MSG5);

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