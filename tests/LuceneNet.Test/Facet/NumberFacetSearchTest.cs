namespace EagleEye.LuceneNet.Test.Facet
{
    using System.Collections.Generic;
    using System.Linq;

    using Lucene.Net.Analysis;
    using Lucene.Net.Analysis.Standard;
    using Lucene.Net.Documents;
    using Lucene.Net.Facet;
    using Lucene.Net.Facet.Range;
    using Lucene.Net.Index;
    using Lucene.Net.Search;
    using Lucene.Net.Store;

    using Xunit;

    public partial class NumberFacetSearchTest
    {
        private readonly Directory directory;
        private readonly IndexWriterConfig indexWriterConfig;

        public NumberFacetSearchTest()
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
        public void FacetSearchTest()
        {
            // arrange
            IndexStaticDocuments();

            // act
            var result = FacetSearch();

            // assert
            var expected = new List<NumberFacetResult>
            {
                new NumberFacetResult("0-10", 11),
                new NumberFacetResult("10-100", 90),
                new NumberFacetResult("100-1000", 900),
                new NumberFacetResult(">1000", 99000),
            };
            Assert.Equal(expected, result);
        }

        private static IEnumerable<DocumentDto> GetDocuments()
        {
            for (var i = 0; i < 100000; i++)
                yield return new DocumentDto($"item {i}", i);
        }

        private static void IndexDocs(IndexWriter writer, DocumentDto dto)
        {
            var doc = new Document
            {
                new TextField(nameof(dto.Name), dto.Name, Field.Store.YES),
                new NumericDocValuesField(nameof(dto.Price), dto.Price),
            };

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
                writer.UpdateDocument(new Term(nameof(dto.Name), dto.Name), doc);
            }
        }

        private void IndexStaticDocuments()
        {
            using (var writer = new IndexWriter(directory, indexWriterConfig))
            {
                foreach (var item in GetDocuments())
                {
                    IndexDocs(writer, item);
                }

                writer.ForceMerge(1);
            }
        }

        private IEnumerable<NumberFacetResult> FacetSearch()
        {
            using (var reader = DirectoryReader.Open(directory))
            {
                var facetsCollector = new FacetsCollector();
                var searcher = new IndexSearcher(reader);

                searcher.Search(new MatchAllDocsQuery(), facetsCollector);

                Facets facets = new Int64RangeFacetCounts(
                    nameof(DocumentDto.Price),
                    facetsCollector,
                    new Int64Range("0-10", 0L, true, 10L, true),
                    new Int64Range("10-100", 10L, true, 100L, false),
                    new Int64Range("100-1000", 100L, true, 1000L, false),
                    new Int64Range(">1000", 1000L, true, long.MaxValue, true));

                var result = facets.GetTopChildren(10, nameof(DocumentDto.Price));
                return result.LabelValues.Select(item => new NumberFacetResult(item.Label, item.Value));
            }
        }
    }
}
