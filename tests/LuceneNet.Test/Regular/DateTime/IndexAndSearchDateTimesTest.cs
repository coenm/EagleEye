namespace EagleEye.LuceneNet.Test.Regular.DateTime
{
    using System.Collections.Generic;
    using System.Linq;

    using Helpers.Guards;
    using Lucene.Net.Analysis;
    using Lucene.Net.Analysis.Standard;
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.Queries;
    using Lucene.Net.Search;
    using Lucene.Net.Store;
    using Xunit;

    public class IndexAndSearchDateTimesTest
    {
        private readonly Directory directory;
        private readonly IndexWriterConfig indexWriterConfig;

        public IndexAndSearchDateTimesTest()
        {
            directory = new RAMDirectory();

            Analyzer analyzer = new StandardAnalyzer(TestHelper.LuceneVersion);

            indexWriterConfig = new IndexWriterConfig(TestHelper.LuceneVersion, analyzer)
            {
                OpenMode = OpenMode.CREATE_OR_APPEND,
                RAMBufferSizeMB = 256.0,
            };
        }

        [Fact]
        public void DocumentsWithinDateRangeTest()
        {
            // arrange
            var filter = TermRangeFilter.NewStringRange(nameof(PersonDto.DateOfBirth), "2000", "2002", true, true);

            IndexStaticDocuments();

            // act
            var result = Filter(filter);

            // assert
            var names = result.Select(x => x.Data.Name).ToList();
            Assert.Equal(new[] { "alice", "eve" }, names);
        }

        [Fact]
        public void ChainedDateRangeFilterTest()
        {
            // arrange
            // query all persons born in 2000, 2001, or 2004 .. 2007-07-01
            var filterA = TermRangeFilter.NewStringRange(nameof(PersonDto.DateOfBirth), "2000", "2002", true, true);
            var filterB = TermRangeFilter.NewStringRange(nameof(PersonDto.DateOfBirth), "2004", "20070701", true, true);
            var filter = new ChainedFilter(new Filter[] { filterA, filterB }, ChainedFilter.OR);

            IndexStaticDocuments();

            // act
            var result = Filter(filter);

            // assert
            var names = result.Select(x => x.Data.Name).ToList();
            Assert.Equal(new[] { "alice", "calvin", "eve" }, names);
        }

        private static IEnumerable<PersonDto> Persons
        {
            get
            {
                yield return new PersonDto("alice", new System.DateTime(2000, 1, 2));
                yield return new PersonDto("bob", new System.DateTime(2002, 1, 12));
                yield return new PersonDto("calvin", new System.DateTime(2007, 6, 2));
                yield return new PersonDto("dwane", new System.DateTime(2007, 11, 2));
                yield return new PersonDto("eve", new System.DateTime(2001, 6, 2));
                yield return new PersonDto("fred", new System.DateTime(1999, 12, 2));
            }
        }

        private static void IndexDocs(IndexWriter writer, PersonDto person)
        {
            var doc = new Document();

            var fieldFilename = new TextField(nameof(PersonDto.Name), person.Name, Field.Store.YES);
            doc.Add(fieldFilename);

            var fieldDateOfBirth = new StringField(
                nameof(PersonDto.DateOfBirth),
                DateTools.DateToString(person.DateOfBirth, DateTools.Resolution.DAY),
                Field.Store.YES);

            doc.Add(fieldDateOfBirth);

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
                writer.UpdateDocument(new Term(nameof(PersonDto.Name), person.Name), doc);
            }
        }

        private void IndexStaticDocuments()
        {
            using (var writer = new IndexWriter(directory, indexWriterConfig))
            {
                foreach (var person in Persons)
                {
                    IndexDocs(writer, person);
                }

                writer.ForceMerge(1);
            }
        }

        private IEnumerable<SearchResults<PersonDto>> Filter(Filter filter)
        {
            return Search(new MatchAllDocsQuery(), filter);
        }

        private IEnumerable<SearchResults<PersonDto>> Search(Query query, Filter filter)
        {
            Guard.NotNull(query, nameof(query));

            var results = new List<SearchResults<PersonDto>>();

            using (var reader = DirectoryReader.Open(directory))
            {
                var searcher = new IndexSearcher(reader);

                var hitsFound = (filter == null)
                    ? searcher.Search(query, 1000)
                    : searcher.Search(query, filter, 1000);

                foreach (var t in hitsFound.ScoreDocs)
                {
                    var doc = searcher.Doc(t.Doc);
                    var name = doc.Get(nameof(PersonDto.Name));
                    var dateOfBirth = DateTools.StringToDate(doc.Get(nameof(PersonDto.DateOfBirth)));

                    var score = t.Score;

                    var searchResultDto = new SearchResults<PersonDto>(new PersonDto(name, dateOfBirth), score);

                    results.Add(searchResultDto);
                }
            }

            return results;
        }
    }
}
