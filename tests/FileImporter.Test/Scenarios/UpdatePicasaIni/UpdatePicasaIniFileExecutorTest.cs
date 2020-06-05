namespace EagleEye.FileImporter.Test.Scenarios.UpdatePicasaIni
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using EagleEye.FileImporter.Scenarios.UpdatePicasaIni;
    using EagleEye.Picasa.Picasa;
    using FakeItEasy;
    using Xunit;

    public class UpdatePicasaIniFileExecutorTest
    {
        private const string DummyFilename = "dummy.ini";
        private readonly IPicasaContactsProvider picasaContactsProvider;
        private readonly IPicasaIniFileProvider picasaIniFileProvider;

        private readonly UpdatePicasaIniFileExecutor sut;
        private readonly IPicaseIniFileWriter picasaIniFileWriter;

        public UpdatePicasaIniFileExecutorTest()
        {
            picasaContactsProvider = A.Fake<IPicasaContactsProvider>();
            picasaIniFileProvider = A.Fake<IPicasaIniFileProvider>();
            picasaIniFileWriter = A.Fake<IPicaseIniFileWriter>();
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
            A.CallTo(() => picasaIniFileProvider.Get(DummyFilename))
             .Returns(new PicasaIniFile(
                                        new List<FileWithPersons>
                                        {
                                            new FileWithPersons(
                                                                "A.jpg",
                                                                new PicasaPersonLocation(
                                                                                         new PicasaPerson("123", "Alice"),
                                                                                         new RelativeRegion(1, 2, 3, 4))),
                                        },
                                        new List<PicasaPerson>(
                                                               new[] { new PicasaPerson("123", "Alice"), })));

            // act
            await sut.HandleAsync(DummyFilename, null, CancellationToken.None);

            // assert
            A.CallTo(() => picasaContactsProvider.GetPicasaContacts()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldNotUpdate_WhenOriginalIniIsComplete()
        {
            // arrange
            A.CallTo(() => picasaContactsProvider.GetPicasaContacts())
             .Returns(new List<PicasaPerson>
                      {
                          new PicasaPerson("123", "AliceUpdated"),
                          new PicasaPerson("1234", "Bob"),
                      });

            A.CallTo(() => picasaIniFileProvider.Get(DummyFilename))
             .Returns(new PicasaIniFile(
                                        new List<FileWithPersons>
                                        {
                                            new FileWithPersons(
                                                                "A.jpg",
                                                                new PicasaPersonLocation(
                                                                                         new PicasaPerson("123", "Alice"),
                                                                                         new RelativeRegion(1, 2, 3, 4))),
                                        },
                                        new List<PicasaPerson>(
                                                               new[] { new PicasaPerson("123", "Alice"), })));

            // act
            await sut.HandleAsync(DummyFilename, null, CancellationToken.None);

            // assert
            A.CallTo(picasaIniFileWriter).MustNotHaveHappened();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldUpdate_WhenOriginalIsNotCompleteAndUpdatedFromContacts()
        {
            // arrange
            A.CallTo(() => picasaContactsProvider.GetPicasaContacts())
             .Returns(new List<PicasaPerson>
                      {
                          new PicasaPerson("123", "AliceUpdated"),
                          new PicasaPerson("1234", "Bob"),
                      });

            var picasaIniFile = new PicasaIniFileBuilder()
                                .GetOrAddFile("A.jpg")
                                .AddPerson(new PicasaPerson("123", "Alice"), 1)
                                .AddUnlistedPerson("1234", 2)
                                .Return()
                                .Build();

            A.CallTo(() => picasaIniFileProvider.Get(DummyFilename)).Returns(picasaIniFile);

            // act
            await sut.HandleAsync(DummyFilename, null, CancellationToken.None);

            // assert
            A.CallTo(() => picasaIniFileWriter.Write(DummyFilename, A<PicasaIniFileUpdater>._, picasaIniFile)).MustHaveHappenedOnceExactly();
        }

        private PicasaPersonLocation CreatePicasaPersonLocation(string id, string name, float regionBaseValue)
        {
            return new PicasaPersonLocation(
                                            new PicasaPerson(id, name),
                                            new RelativeRegion(regionBaseValue, regionBaseValue + 1, regionBaseValue + 2, regionBaseValue + 3));
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

            public FileWithPersonsBuilder GetOrAddFile(string filename)
            {
                var item = files.FirstOrDefault(f => f.Filename == filename);
                if (item == null)
                {
                    item = new FileWithPersonsBuilder(filename, this);
                    files.Add(item);
                }

                return item;
            }

            public PicasaIniFileBuilder AddToContacts(in PicasaPerson person)
            {
                contacts.Add(person);
                return this;
            }

            public PicasaIniFile Build()
            {
                return new PicasaIniFile(files.Select(x => x.Build()).ToList(), contacts.ToList());
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

            public FileWithPersonsBuilder AddPerson(PicasaPerson person, float regionBase)
            {
                items.Add(new PicasaPersonLocation(person, new RelativeRegion(regionBase, regionBase + 1, regionBase + 2, regionBase + 3)));
                _ = parent.AddToContacts(person);
                return this;
            }

            public FileWithPersonsBuilder AddUnlistedPerson(string id, float regionBase)
            {
                var person = new PicasaPerson(id, string.Empty);
                items.Add(new PicasaPersonLocation(person, new RelativeRegion(regionBase, regionBase + 1, regionBase + 2, regionBase + 3)));
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
    }
}
