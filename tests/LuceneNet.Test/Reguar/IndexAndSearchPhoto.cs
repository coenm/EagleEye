namespace EagleEye.LuceneNet.Test.Reguar
{
    using System.Collections.Generic;
    using System.Linq;

    using Lucene.Net.Analysis;
    using Lucene.Net.Analysis.Standard;
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.QueryParsers.Classic;
    using Lucene.Net.Search;
    using Lucene.Net.Store;

    using Xunit;

    public class IndexAndSearchPhoto
    {
        private const string PERSON = "person";
        private const string FILENAME = "filename";

        private readonly Directory _directory;
        private readonly Analyzer _analyzer;
        private readonly IndexWriterConfig _indexWriterConfig;

        public IndexAndSearchPhoto()
        {
            _directory = new RAMDirectory();
            _analyzer = new StandardAnalyzer(TestHelper.LUCENE_VERSION);

            _indexWriterConfig = new IndexWriterConfig(TestHelper.LUCENE_VERSION, _analyzer)
            {
                OpenMode = OpenMode.CREATE_OR_APPEND,
                RAMBufferSizeMB = 256.0
            };
        }

        [Theory]
        [InlineData("file*")]
        [InlineData("f*")]
        [InlineData("filename:f*")]
        [InlineData("filename:f*  ")]
        [InlineData("filename:file*")]
        [InlineData("filename:file1 OR filename:file2 OR filename:file3 OR filename:file4 OR file5 OR file6 OR file7 OR file8 OR file9")]
        [InlineData("file1 OR file2 OR file3 OR file4 OR file5 OR file6 OR file7 OR file8 OR file9")]
        public void SearchFilesShouldReturnAllFilesTest(string search)
        {
            // arrange
            IndexStaticDocuments();

            // act
            var result = Search(search).ToArray();

            // assert
            var resultsFilenames = result.Select(x => x.Data.Filename).ToArray();
            var originalFilenames = StaticDocuments.Photos.Select(x => x.Filename).ToArray();
            Assert.Equal(originalFilenames, resultsFilenames);
        }

        [Theory]
        [InlineData("\"Robin van Persie\"", "file5", "file6")]
        [InlineData("\"Robin van Persie\" -ruud", "file6")]
        [InlineData("+person:\"Persie\" -person:ruud", "file6", "file8")]
        public void SearchPersonsShouldReturnAllFilesTest(string search, params string[] expectedFilenames)
        {
            // arrange
            IndexStaticDocuments();

            // act
            var result = SearchPersons(search).ToArray();

            // assert
            Assert.Equal(expectedFilenames, result.Select(x => x.Data.Filename));
        }


        [Fact]
        public void DeleteFromIndexTest()
        {
            // arrange
            IndexStaticDocuments();

            // assume
            var results = SearchPersons("ruud").ToList();
            Assert.NotEmpty(results);

            // act
            DeleteFromIndex(new Term("person", "ruud"));
            results = SearchPersons("ruud").ToList();

            // assert
            Assert.Empty(results);
        }

        private IEnumerable<SearchResults<PhotoMetadataDto>> SearchPersons(string query)
        {
            return Search(query, PERSON);
        }

        private IEnumerable<SearchResults<PhotoMetadataDto>> Search(Query query)
        {
            var results = new List<SearchResults<PhotoMetadataDto>>();
            using (var reader = DirectoryReader.Open(_directory))
            {
                var searcher = new IndexSearcher(reader);

                var hitsFound = searcher.Search(query, 10);

                foreach (var t in hitsFound.ScoreDocs)
                {
                    var doc = searcher.Doc(t.Doc);
                    var filename = doc.Get(FILENAME);
                    var persons = doc.GetValues(PERSON);
                    var score = t.Score;

                    var searchResultDto = new SearchResults<PhotoMetadataDto>(new PhotoMetadataDto(filename, persons), score);

                    results.Add(searchResultDto);
                }
            }

            return results;
        }

        private IEnumerable<SearchResults<PhotoMetadataDto>> Search(string query, string defaultSearchField = FILENAME)
        {
            var parser = new QueryParser(TestHelper.LUCENE_VERSION, defaultSearchField, _analyzer);
            return Search(parser.Parse(query));
        }

        private void IndexStaticDocuments()
        {
            using (var writer = new IndexWriter(_directory, _indexWriterConfig))
            {
                foreach (var photo in StaticDocuments.Photos)
                {
                    IndexDocs(writer, photo);
                }

                writer.ForceMerge(1);
            }
        }


        private void DeleteFromIndex(Term term)
        {
            var indexWriterConfig = new IndexWriterConfig(TestHelper.LUCENE_VERSION, _analyzer)
                                     {
                                         OpenMode = OpenMode.CREATE_OR_APPEND,
                                         RAMBufferSizeMB = 256.0
                                     };

            using (var writer = new IndexWriter(_directory, indexWriterConfig))
            {
                writer.DeleteDocuments(term);
                writer.ForceMerge(1);
            }
        }

        private static void IndexDocs(IndexWriter writer, PhotoMetadataDto photo)
        {
            var doc = new Document();

            Field fieldFilename = new TextField(FILENAME, photo.Filename, Field.Store.YES);
            doc.Add(fieldFilename);

            var persons = photo.Persons
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .ToArray();

            foreach (var person in persons)
            {
                Field fieldPerson = new TextField(PERSON, person, Field.Store.YES);
                doc.Add(fieldPerson);
            }


            if (writer.Config.OpenMode == OpenMode.CREATE)
            {
                // New index, so we just add the document (no old document can be there):
                writer.AddDocument(doc);
            }
            else
            {
                // Existing index (an old copy of this document may have been indexed) so
                // we use updateDocument instead to replace the old one matching the exact
                // path, if present:
                writer.UpdateDocument(new Term(FILENAME, photo.Filename), doc);
            }
        }
    }
}