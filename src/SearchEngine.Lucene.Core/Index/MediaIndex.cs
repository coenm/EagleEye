namespace SearchEngine.LuceneNet.Core.Index
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
                              // fileinformation
                              new StringField("filename", data.FileInformation?.Filename ?? string.Empty, Field.Store.YES),
                              new StringField("filetype", data.FileInformation?.Type ?? string.Empty, Field.Store.YES),

                              // location data
                              // todo GPS
                              new TextField("city", data.Location?.City ?? string.Empty, Field.Store.YES),
                              new TextField("countrycode", data.Location?.CountryCode ?? string.Empty, Field.Store.YES),
                              new TextField("country", data.Location?.CountryName ?? string.Empty, Field.Store.YES),
                              new TextField("state", data.Location?.State ?? string.Empty, Field.Store.YES),
                              new TextField("sublocation", data.Location?.SubLocation ?? string.Empty, Field.Store.YES),
                          };

            var dateString = DateTools.DateToString(data.DateTimeTaken.Value, PrecisionToResolution(data.DateTimeTaken.Precision));
            doc.Add(new StringField("date", dateString, Field.Store.YES));

            foreach (var person in data.Persons ?? new List<string>())
            {
                doc.Add(new TextField("person", person, Field.Store.YES));
            }

            foreach (var tag in data.Tags ?? new List<string>())
            {
                doc.Add(new TextField("tag", tag, Field.Store.YES));
            }

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
                    var item = new MediaResult(result.Score)
                                   {
                                       FileInformation = new FileInformation
                                                             {
                                                                 Filename = doc.GetField("filename")?.GetStringValue(),
                                                                 Type = doc.GetField("filetype")?.GetStringValue()
                                                             },
                                       Location =
                                           {
                                               CountryName = doc.GetField("country")?.GetStringValue(),
                                               State = doc.GetField("state")?.GetStringValue(),
                                               City = doc.GetField("city")?.GetStringValue(),
                                               SubLocation = doc.GetField("sublocation")?.GetStringValue(),
                                               CountryCode = doc.GetField("countrycode")?.GetStringValue(),

                                               // todo GPS
                                           },
                                       DateTimeTaken = new Timestamp
                                                           {
                                                               Precision = TimestampPrecision.Day, //todo
                                                               Value = DateTools.StringToDate(doc.Get("date")),
                                                           },
                                       Persons = doc.GetValues("person")?.ToList() ?? new List<string>(),
                                       Tags = doc.GetValues("tag")?.ToList() ?? new List<string>(),
                                   };

                    results.Add(item);
                }

                // var dateString = DateTools.DateToString(data.DateTimeTaken.Value, PrecisionToResolution(data.DateTimeTaken.Precision));
                // doc.Add(new StringField("date", dateString, Field.Store.YES));
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

        private DateTools.Resolution PrecisionToResolution(TimestampPrecision precision)
        {
            switch (precision)
            {
                case TimestampPrecision.Year:
                    return DateTools.Resolution.YEAR;
                case TimestampPrecision.Month:
                    return DateTools.Resolution.MONTH;
                case TimestampPrecision.Day:
                    return DateTools.Resolution.DAY;
                case TimestampPrecision.Hour:
                    return DateTools.Resolution.HOUR;
                case TimestampPrecision.Minute:
                    return DateTools.Resolution.MINUTE;
                case TimestampPrecision.Second:
                    return DateTools.Resolution.SECOND;
                default:
                    throw new ArgumentOutOfRangeException(nameof(precision), precision, null);
            }
        }
    }
}