namespace EagleEye.FileImporter.Test.Scenarios.UpdatePicasaIni
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.FileImporter.Scenarios.UpdatePicasaIni;
    using EagleEye.Picasa.Picasa;
    using FakeItEasy;
    using FluentAssertions;
    using VerifyXunit;
    using Xunit;
    using Xunit.Abstractions;

    public class UpdatePicasaIniFileExecutorTest : VerifyBase
    {
        private const string DummyFilename = "dummy.ini";
        private readonly IPicasaContactsProvider picasaContactsProvider;
        private readonly IPicasaIniFileProvider picasaIniFileProvider;

        private readonly UpdatePicasaIniFileExecutor sut;
        private readonly IPicasaIniFileWriter picasaIniFileWriter;

        private readonly PicasaPerson person123Alice = new PicasaPerson("123", "Alice");
        private readonly PicasaPerson person123AliceUpdated = new PicasaPerson("123", "AliceUpdated");
        private readonly PicasaPerson person1234Bob = new PicasaPerson("1234", "Bob");
        private readonly PicasaPerson person456Calvin = new PicasaPerson("456", "Calvin");
        private readonly PicasaPerson person7894Derek = new PicasaPerson("789", "Derek");

        private readonly Rect64RelativeRegion region1 = new Rect64RelativeRegion("rect64(935f5217a1696893)");
        private readonly Rect64RelativeRegion region2 = new Rect64RelativeRegion("rect64(4f5c884659bb98b2)");
        private readonly Rect64RelativeRegion region3 = new Rect64RelativeRegion("rect64(66273ab58849596a)");

        private readonly List<WrittenIniFilesData> writtenIniFiles;

        public UpdatePicasaIniFileExecutorTest(ITestOutputHelper output)
            : base(output)
        {
            picasaContactsProvider = A.Fake<IPicasaContactsProvider>();
            picasaIniFileProvider = A.Fake<IPicasaIniFileProvider>();
            picasaIniFileWriter = A.Fake<IPicasaIniFileWriter>();

            writtenIniFiles = new List<WrittenIniFilesData>();
            A.CallTo(() => picasaIniFileWriter.Write(DummyFilename, A<PicasaIniFileUpdater>._, A<PicasaIniFile>._))
             .Invokes(call =>
                      {
                          var filename = call.Arguments[0] as string;
                          var updater = call.Arguments[1] as PicasaIniFileUpdater;
                          var original = call.Arguments[2] as PicasaIniFile;

                          writtenIniFiles.Add(new WrittenIniFilesData(updater, original));
                          return;
                      });

            sut = new UpdatePicasaIniFileExecutor(picasaContactsProvider, picasaIniFileProvider, picasaIniFileWriter);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldNotGetContacts_WhenPicasaIniIsNull()
        {
            // arrange
            A.CallTo(() => picasaIniFileProvider.Get(DummyFilename)).Returns(null);

            // act
            await sut.HandleAsync(DummyFilename, null, CancellationToken.None);

            // assert
            A.CallTo(picasaContactsProvider).MustNotHaveHappened();
            A.CallTo(picasaIniFileWriter).MustNotHaveHappened();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldGetContacts_WhenPicasaIniIsNotNull()
        {
            // arrange
            var iniFile = new PicasaIniFileBuilder()
                          .WithFile("A.jpg")
                            .AddPerson(person123Alice, region1)
                            .Return()
                          .Build();

            SetupPicasaIniFileProvider(iniFile);

            // act
            await sut.HandleAsync(DummyFilename, null, CancellationToken.None);

            // assert
            A.CallTo(() => picasaContactsProvider.GetPicasaContacts()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldNotUpdate_WhenOriginalIniIsComplete()
        {
            // arrange
            var iniFile = new PicasaIniFileBuilder()
                          .WithFile("A.jpg")
                              .AddPerson(person123Alice, region1)
                              .Return()
                          .Build();

            SetupPicasaContactsProvider(person123AliceUpdated, person1234Bob);
            SetupPicasaIniFileProvider(iniFile);

            // act
            await sut.HandleAsync(DummyFilename, null, CancellationToken.None);

            // assert
            A.CallTo(picasaIniFileWriter).MustNotHaveHappened();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldUpdate_WhenOriginalIsNotCompleteAndUpdatedFromContacts()
        {
            // arrange
            var iniFile = new PicasaIniFileBuilder()
                          .WithFile("A.jpg")
                              .AddPerson(person123Alice, region1)
                              .AddUnlistedPerson("1234", region2)
                              .Return()
                          .WithFile("B.jpg")
                              .AddUnlistedPerson("1234", region3)
                              .Return()
                          .Build();

            SetupPicasaContactsProvider(person123AliceUpdated, person1234Bob);
            SetupPicasaIniFileProvider(iniFile);

            // act
            await sut.HandleAsync(DummyFilename, null, CancellationToken.None);

            // assert
            A.CallTo(() => picasaIniFileWriter.Write(DummyFilename, A<PicasaIniFileUpdater>._, iniFile)).MustHaveHappenedOnceExactly();
            await Verify(writtenIniFiles);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldUpdate_WhenOriginalIsIncompleteAndBackupHasDifferentIdAndNameOnRegion()
        {
            // arrange
            var iniFile = new PicasaIniFileBuilder()
                          .WithFile("A.jpg")
                              .AddPerson(person123Alice, region1)
                              .AddUnlistedPerson("1234", region2)
                              .Return()
                          .Build();

            var backupPicasaIni = new PicasaIniFileBuilder()
                                  .WithFile("A.jpg")
                                      .AddPerson(person123AliceUpdated, region1)
                                      .AddPerson(person456Calvin, region2)
                                    .Return()
                                  .Build();

            SetupPicasaContactsProvider(person123AliceUpdated, person1234Bob, person456Calvin);
            SetupPicasaIniFileProvider(iniFile);
            SetupPicasaBackups(backupPicasaIni);

            // act
            await sut.HandleAsync(DummyFilename, null, CancellationToken.None);

            // assert
            A.CallTo(() => picasaIniFileWriter.Write(DummyFilename, A<PicasaIniFileUpdater>._, iniFile)).MustHaveHappenedOnceExactly();
            writtenIniFiles[0].Updater.IniFile.Files[0].Persons.Should()
                              .BeEquivalentTo(
                                              new PicasaPersonLocation(person123Alice, region1),
                                              new PicasaPersonLocation(new PicasaPerson("1234", "Calvin"), region2));
            await Verify(writtenIniFiles);
        }

        private void SetupPicasaContactsProvider(params PicasaPerson[] persons)
        {
            A.CallTo(() => picasaContactsProvider.GetPicasaContacts()).Returns(persons);
        }

        private void SetupPicasaIniFileProvider(PicasaIniFile picasaIniFile)
        {
            A.CallTo(() => picasaIniFileProvider.Get(DummyFilename)).Returns(picasaIniFile);
        }

        private void SetupPicasaBackups(params PicasaIniFile[] backupPicasaIni)
        {
            A.CallTo(() => picasaIniFileProvider.GetBackups(DummyFilename)).Returns(backupPicasaIni);
        }

        private class PicasaIniFileBuilder
        {
            private readonly List<FileWithPersonsBuilder> files;
            private readonly List<PicasaPerson> contacts;

            public PicasaIniFileBuilder()
            {
                files = new List<FileWithPersonsBuilder>();
                contacts = new List<PicasaPerson>();
            }

            public FileWithPersonsBuilder WithFile(string filename)
            {
                var item = files.FirstOrDefault(f => f.Filename == filename);
                if (item != null)
                    return item;

                item = new FileWithPersonsBuilder(filename, this);
                files.Add(item);

                return item;
            }

            public PicasaIniFileBuilder AddToContacts(in PicasaPerson person)
            {
                contacts.Add(person);
                return this;
            }

            public PicasaIniFile Build()
            {
                return new PicasaIniFile(files.Select(x => x.Build()).ToList(), contacts.Distinct().ToList());
            }
        }

        private class FileWithPersonsBuilder
        {
            private readonly List<PicasaPersonLocation> items;
            private readonly PicasaIniFileBuilder parent;

            public FileWithPersonsBuilder(string filename, PicasaIniFileBuilder parent)
            {
                Filename = filename;
                this.parent = parent;
                items = new List<PicasaPersonLocation>();
            }

            public string Filename { get; }

            public FileWithPersonsBuilder AddPerson(PicasaPerson person, Rect64RelativeRegion region)
            {
                items.Add(new PicasaPersonLocation(person, region));
                _ = parent.AddToContacts(person);
                return this;
            }

            public FileWithPersonsBuilder AddUnlistedPerson(string id, Rect64RelativeRegion region)
            {
                var person = new PicasaPerson(id, string.Empty);
                items.Add(new PicasaPersonLocation(person, region));
                return this;
            }

            public PicasaIniFileBuilder Return()
            {
                return parent;
            }

            public FileWithPersons Build()
            {
                return new FileWithPersons(Filename, items.ToArray());
            }
        }

        private class WrittenIniFilesData
        {
            public WrittenIniFilesData(PicasaIniFileUpdater updater, PicasaIniFile original)
            {
                Updater = updater;
                Original = original;
            }

            public PicasaIniFileUpdater Updater { get; }

            public PicasaIniFile Original { get; }
        }
    }
}
