using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Facet;
using Lucene.Net.Facet.Range;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Xunit;

namespace LuceneNet.Test.Facet
{
    public class NumberFacetSearchTest
    {
        private readonly Directory _directory;
        private readonly Analyzer _analyzer;
        private readonly IndexWriterConfig _indexWriterConfig;

        public NumberFacetSearchTest()
        {
            _directory = new RAMDirectory();
            // _directory = FSDirectory.Open(PathIndex);

            _analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);

            _indexWriterConfig = new IndexWriterConfig(LuceneVersion.LUCENE_48, _analyzer)
            {
                OpenMode = OpenMode.CREATE_OR_APPEND,
                RAMBufferSizeMB = 256.0
            };
        }


        [Fact]
        public void FacetSearchTest()
        {
            // arrange
            IndexStaticDocuments();

            // act
            var fc = new FacetsCollector();
            using (var reader = DirectoryReader.Open(_directory))
            {
                var searcher = new IndexSearcher(reader);

                searcher.Search(new MatchAllDocsQuery(), fc);

                Facets facets = new Int64RangeFacetCounts(
                    nameof(DocumentDto.Price),
                    fc,
                    new Int64Range("0-10", 0L, true, 10L, true),
                    new Int64Range("10-100", 10L, true, 100L, false),
                    new Int64Range("100-1000", 100L, true, 1000L, false),
                    new Int64Range(">1000", 1000L, true, long.MaxValue, true));

                var result = facets.GetTopChildren(10, nameof(DocumentDto.Price));

                // assert
                const string expectedResult = "dim=Price path=[] value=7 childCount=4\n  0-10 (2)\n  10-100 (1)\n  100-1000 (2)\n  >1000 (2)\n";
                Assert.Equal(expectedResult, result.ToString());
            }
        }


        private void IndexStaticDocuments()
        {
            using (var writer = new IndexWriter(_directory, _indexWriterConfig))
            {
                foreach (var item in StaticDocuments.Items)
                {
                    IndexDocs(writer, item);
                }

                writer.ForceMerge(1);
            }
        }

        private static void IndexDocs(IndexWriter writer, DocumentDto dto)
        {
            var doc = new Document
            {
                new TextField(nameof(dto.Name), dto.Name, Field.Store.YES),
                new NumericDocValuesField(nameof(dto.Price), dto.Price)
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
    }
}