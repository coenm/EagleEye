namespace EagleEye.ExifToolWrapper.Test.ExifTool
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using EagleEye.ExifTool;
    using EagleEye.ExifTool.ExifTool;
    using FluentAssertions;

    using Xunit;

    public class ExifToolStayOpenStreamTest : IDisposable
    {
        private readonly ExifToolStayOpenStream sut;
        private readonly List<DataCapturedArgs> capturedEvents;

        public ExifToolStayOpenStreamTest()
        {
            capturedEvents = new List<DataCapturedArgs>();
            sut = new ExifToolStayOpenStream(Encoding.UTF8);
            sut.Update += SutOnUpdate;
        }

        public void Dispose()
        {
            sut.Update -= SutOnUpdate;
            sut?.Dispose();
        }

        [Fact]
        public void ExifToolStayOpenStreamCtorThrowsArgumentOutOfRangeWhenBufferSizeIsNegativeTest()
        {
            // arrange

            // act
            Action act = () => _ = new ExifToolStayOpenStream(null, -1);

            // assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void DefaultPropertiesShouldNoThrowAndDoNotDoAnythingTest()
        {
            sut.CanWrite.Should().BeTrue();
            sut.CanRead.Should().BeFalse();
            sut.CanSeek.Should().BeFalse();

            // nothing is written yet.
            sut.Length.Should().Be(0);
            sut.Position.Should().Be(0);
        }

        [Fact]
        public void SetPositionShouldNotDoAnythingTest()
        {
            // arrange

            // assume
            sut.Position.Should().Be(0);

            // act
            sut.Position = 100;

            // assert
            sut.Position.Should().Be(0);
        }

        [Fact]
        public void FlushShouldNotDoAnythingAndDefinitelyNotThrowTest()
        {
            sut.Flush();
        }

        [Fact]
        public void SeekAlwaysReturnsZeroTest()
        {
            // arrange

            // act
            var result = sut.Seek(0, SeekOrigin.Begin);

            // assert
            result.Should().Be(0);
        }

        [Fact]
        public void SetLengthDoesNotDoAnythingTest()
        {
            // arrange

            // assume
            sut.Length.Should().Be(0);

            // act
            sut.SetLength(100);

            // assert
            sut.Length.Should().Be(0);
        }

        [Fact]
        public void ReadThrowsTest()
        {
            // arrange
            var buffer = new byte[100];

            // act
            Action act = () => _ = sut.Read(buffer, 0, 100);

            // assert
            act.Should().Throw<NotSupportedException>();
        }

        [Fact]
        public void SingleWriteShouldNotFireEvent()
        {
            // arrange
            const string msg = "dummy data without delimiter";

            // act
            WriteMessageToSut(msg);

            // assert
            capturedEvents.Should().BeEmpty();
        }

        [Fact]
        public void ParseSingleMessage()
        {
            // arrange
            const string msg = "a b c\r\nd e f\r\n{ready0}\r\n";

            // act
            WriteMessageToSut(msg);

            // assert
            capturedEvents.Should().ContainSingle();
            capturedEvents.First().Key.Should().Be("0");
            capturedEvents.First().Data.Should().Be("a b c\r\nd e f".ConvertToOsString());
        }

        [Fact]
        public void ParseTwoMessagesInSingleWrite()
        {
            // arrange
            const string msg = "a b c\r\n{ready0}\r\nd e f\r\n{ready1}\r\nxyz";

            // act
            WriteMessageToSut(msg);

            // assert
            capturedEvents.Should().HaveCount(2);

            capturedEvents[0].Key.Should().Be("0");
            capturedEvents[0].Data.Should().Be("a b c");

            capturedEvents[1].Key.Should().Be("1");
            capturedEvents[1].Data.Should().Be("d e f");
        }

        [Fact]
        public void ParseTwoMessagesOverFourWrites()
        {
            // arrange
            const string msg1 = "a b c\r\nd e f\r\n{ready0}\r\nghi";
            const string msg2 = " jkl\r\n{re";
            const string msg3 = "ady";
            const string msg4 = "213";
            const string msg5 = "3}\r\n";

            // act
            WriteMessageToSut(msg1);
            WriteMessageToSut(msg2);
            WriteMessageToSut(msg3);
            WriteMessageToSut(msg4);
            WriteMessageToSut(msg5);

            // assert
            capturedEvents.Should().HaveCount(2)
                           .And.Contain(x => x.Key == "0" && x.Data == "a b c\r\nd e f".ConvertToOsString())
                           .And.Contain(x => x.Key == "2133" && x.Data == "ghi jkl".ConvertToOsString());
        }

        private void WriteMessageToSut(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message.ConvertToOsString());
            sut.Write(buffer, 0, buffer.Length);
        }

        private void SutOnUpdate(object sender, DataCapturedArgs dataCapturedArgs)
        {
            capturedEvents.Add(dataCapturedArgs);
        }
    }
}
