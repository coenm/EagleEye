namespace SearchEngine.LuceneNet.Core.Index
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using Lucene.Net.Analysis;
    using Lucene.Net.Analysis.Core;
    using Lucene.Net.Analysis.Miscellaneous;
    using Lucene.Net.Analysis.Standard;
    using Lucene.Net.Analysis.Util;
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.QueryParsers.Classic;
    using Lucene.Net.Search;

    using SearchEngine.LuceneNet.Core.Commands.UpdateIndex;
    using SearchEngine.LuceneNet.Core.Internals;

    using Directory = Lucene.Net.Store.Directory;

    public class MediaIndex : IDisposable
    {
        private readonly Analyzer _analyzer;
        private readonly IndexWriter _indexWriter;
        private readonly SearcherManager _searcherManager;
        private readonly QueryParser _queryParser;
        private readonly Directory _indexDirectory;

        public MediaIndex([NotNull] ILuceneDirectoryFactory indexDirectoryFactory)
        {
            _indexDirectory = indexDirectoryFactory.Create();

            _analyzer = new PerFieldAnalyzerWrapper(
                                                    new HtmlStripAnalyzer(LuceneNetVersion.VERSION),
                                                    new Dictionary<string, Analyzer>
                                                        {
                                                            {
                                                                "owner",
                                                                Analyzer.NewAnonymous((fieldName, reader) =>
                                                                                      {
                                                                                          var source = new KeywordTokenizer(reader);
                                                                                          TokenStream result = new ASCIIFoldingFilter(source);
                                                                                          result = new LowerCaseFilter(LuceneNetVersion.VERSION, result);
                                                                                          return new TokenStreamComponents(source, result);
                                                                                      })
                                                            },
                                                            {
                                                                "name",
                                                                Analyzer.NewAnonymous((fieldName, reader) =>
                                                                                      {
                                                                                          var source = new StandardTokenizer(LuceneNetVersion.VERSION, reader);
                                                                                          TokenStream result = new WordDelimiterFilter(LuceneNetVersion.VERSION, source, ~WordDelimiterFlags.STEM_ENGLISH_POSSESSIVE, CharArraySet.EMPTY_SET);
                                                                                          result = new ASCIIFoldingFilter(result);
                                                                                          result = new LowerCaseFilter(LuceneNetVersion.VERSION, result);
                                                                                          return new TokenStreamComponents(source, result);
                                                                                      })
                                                            }
                                                        });

            _queryParser = new MultiFieldQueryParser(
                                                     LuceneNetVersion.VERSION,
                                                     new[] { "name", "description", "readme" },
                                                     _analyzer);

            var indexWriterConfig = new IndexWriterConfig(LuceneNetVersion.VERSION, _analyzer)
                                        {
                                            OpenMode = OpenMode.CREATE_OR_APPEND,
                                            RAMBufferSizeMB = 256.0
                                        };

            _indexWriter = new IndexWriter(_indexDirectory, indexWriterConfig);

            _searcherManager = new SearcherManager(_indexWriter, true, null);
        }

        [PublicAPI]
        public Task<bool> IndexMediaFileAsync([NotNull] MediaObject data)
        {
            var doc = new Document
                          {
                              new StringField("url", string.Empty, Field.Store.YES),
                              new TextField("name", string.Empty, Field.Store.YES),
                              new TextField("description", string.Empty, Field.Store.YES),
                              new TextField("readme", string.Empty, Field.Store.NO),
                              new TextField("owner", string.Empty, Field.Store.YES),
                          };


            if (_indexWriter.Config.OpenMode == OpenMode.CREATE)
            {
                // New index, so we just add the document (no old document can be there):
                _indexWriter.AddDocument(doc);
            }
            else
            {
                // Existing index (an old copy of this document may have been indexed) so
                // we use updateDocument instead to replace the old one matching the exact
                // path, if present:
                _indexWriter.UpdateDocument(new Term(nameof(data.FileInformation.Filename), data.FileInformation.Filename), doc);
            }

            _indexWriter.Flush(true, true);
            _indexWriter.Commit();

            // expensive
            // _indexWriter.ForceMerge(1);

            return Task.FromResult(true);
        }

        [PublicAPI]
        public List<SearchResultBase> Search(string queryString, out int totalHits)
        {
            // Parse the query - assuming it's not a single term but an actual query string
            // the QueryParser used is using the same analyzer used for indexing
            var query = _queryParser.Parse(queryString);
            return Search(query, null, out totalHits);
        }

        [PublicAPI]
        public List<SearchResultBase> Search([NotNull] Query query, [CanBeNull] Filter filter, out int totalHits)
        {
            var results = new List<SearchResultBase>();
            totalHits = 0;

            // Execute the search with a fresh indexSearcher
            _searcherManager.MaybeRefreshBlocking();

            var searcher = _searcherManager.Acquire();

            try
            {
                var topDocs = filter == null
                                  ? searcher.Search(query, 1000)
                                  : searcher.Search(query, filter, 1000);

                totalHits = topDocs.TotalHits;

                foreach (var result in topDocs.ScoreDocs)
                {
                    var doc = searcher.Doc(result.Doc);

                    // Results are automatically sorted by relevance
                    results.Add(new MediaResult(result.Score)
                                    {
                                        DateTimeTaken = new Timestamp(),
                                        Persons = new List<string>
                                                      {
                                                          doc.GetField("persons")?.GetStringValue()
                                                      },

                                        // Name = doc.GetField("name")?.GetStringValue(),
                                    });
                }
            }
            catch (Exception)
            {
                // do nothing
            }
            finally
            {
                _searcherManager.Release(searcher);
                searcher = null; // Don't use searcher after this point!
            }

            return results;
        }

        public void Dispose()
        {
            _analyzer?.Dispose();
            _indexWriter?.Dispose();
            _searcherManager?.Dispose();
            _indexDirectory?.Dispose();
        }
    }
}