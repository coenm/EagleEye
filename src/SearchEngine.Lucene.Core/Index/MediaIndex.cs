namespace SearchEngine.LuceneNet.Core.Index
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
    using Lucene.Net.Spatial;
    using Lucene.Net.Spatial.Prefix;
    using Lucene.Net.Spatial.Prefix.Tree;

    using SearchEngine.LuceneNet.Core.Commands.UpdateIndex;
    using SearchEngine.LuceneNet.Core.Internals;

    using Spatial4n.Core.Context;

    using Directory = Lucene.Net.Store.Directory;

    public class MediaIndex : IDisposable
    {
        private readonly Analyzer _analyzer;
        private readonly IndexWriter _indexWriter;
        private readonly SearcherManager _searcherManager;
        private readonly QueryParser _queryParser;
        private readonly Directory _indexDirectory;

        private SpatialContext _spatialContext;
        private SpatialStrategy _spatialStrategy;

        private const string KEY_FILENAME = "filename";
        private const string KEY_FILETYPE = "filetype";
        private const string KEY_LOC_CITY = "city";
        private const string KEY_LOC_COUNTRY_CODE = "countrycode";
        private const string KEY_LOC_COUNTRY = "country";
        private const string KEY_LOC_STATE = "state";
        private const string KEY_LOC_SUBLOCATION = "sublocation";
        private const string KEY_LOC_LONGITUDE = "longitude";
        private const string KEY_LOC_LATITUDE = "latitude";

        private const string KEY_DATE_TAKEN = "date";
        private const string KEY_PERSON = "person";
        private const string KEY_TAG = "tag";

        private const string KEY_LOC_GPS = "gps";


        public MediaIndex([NotNull] ILuceneDirectoryFactory indexDirectoryFactory)
        {
            _indexDirectory = indexDirectoryFactory.Create();

            InitSpatial();

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
            // todo verification of data object
            if (data.FileInformation == null)
                throw new ArgumentNullException(nameof(data.FileInformation));

            if (string.IsNullOrWhiteSpace(data.FileInformation.Filename))
                throw new ArgumentNullException(nameof(data.FileInformation.Filename));

            // todo lock?!

            RemoveFromIndexByFilename(data.FileInformation.Filename);
            var doc = new Document
                          {
                              // fileinformation
                              new StringField(KEY_FILENAME, data.FileInformation?.Filename ?? string.Empty, Field.Store.YES),
                              new StringField(KEY_FILETYPE, data.FileInformation?.Type ?? string.Empty, Field.Store.YES),

                              // location data
                              new TextField(KEY_LOC_CITY, data.Location?.City ?? string.Empty, Field.Store.YES),
                              new TextField(KEY_LOC_COUNTRY_CODE, data.Location?.CountryCode ?? string.Empty, Field.Store.YES),
                              new TextField(KEY_LOC_COUNTRY, data.Location?.CountryName ?? string.Empty, Field.Store.YES),
                              new TextField(KEY_LOC_STATE, data.Location?.State ?? string.Empty, Field.Store.YES),
                              new TextField(KEY_LOC_SUBLOCATION, data.Location?.SubLocation ?? string.Empty, Field.Store.YES),

                              new StoredField(KEY_LOC_LONGITUDE, data.Location?.Coordinate?.Longitude ?? 0),
                              new StoredField(KEY_LOC_LATITUDE, data.Location?.Coordinate?.Latitude ?? 0),
                          };

            // index coordinate
            {
                var x = data.Location?.Coordinate?.Longitude;
                var y = data.Location?.Coordinate?.Latitude;

                if (x != null)
                {
                    var p = _spatialContext.MakePoint(x.Value, y.Value);

                    foreach (var shape in new[] { p })
                    {
                        foreach (var field in _spatialStrategy.CreateIndexableFields(shape))
                        {
                            doc.Add(field);
                        }
                        // var pt = (IPoint)shape;
                        // doc.Add(new StoredField(_strategy.FieldName, pt.X.ToString(CultureInfo.InvariantCulture) + " " + pt.Y.ToString(CultureInfo.InvariantCulture)));
                    }
                }
            }


            var dateString = DateTools.DateToString(data.DateTimeTaken.Value, PrecisionToResolution(data.DateTimeTaken.Precision));
            doc.Add(new StringField(KEY_DATE_TAKEN, dateString, Field.Store.YES));

            foreach (var person in data.Persons ?? new List<string>())
            {
                doc.Add(new TextField(KEY_PERSON, person, Field.Store.YES));
            }

            foreach (var tag in data.Tags ?? new List<string>())
            {
                doc.Add(new TextField(KEY_TAG, tag, Field.Store.YES));
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
        public int Count([CanBeNull] Query query = null, [CanBeNull] Filter filter = null)
        {
            // Execute the search with a fresh indexSearcher
            _searcherManager.MaybeRefreshBlocking();

            var searcher = _searcherManager.Acquire();

            try
            {
                if (query == null)
                    query = new MatchAllDocsQuery();

                var topDocs = filter == null
                                  ? searcher.Search(query, 1)
                                  : searcher.Search(query, filter, 1);

                return topDocs.TotalHits;
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

            throw new Exception("No items found.");
        }

        [PublicAPI]
        public List<MediaResult> Search(string queryString, out int totalHits)
        {
            // Parse the query - assuming it's not a single term but an actual query string
            // the QueryParser used is using the same analyzer used for indexing
            var query = _queryParser.Parse(queryString);
            return Search(query, null, out totalHits);
        }

        [PublicAPI]
        public List<MediaResult> Search([NotNull] Query query, [CanBeNull] Filter filter, out int totalHits)
        {
            var results = new List<MediaResult>();
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
                                                                 Filename = doc.GetField(KEY_FILENAME)?.GetStringValue(),
                                                                 Type = doc.GetField(KEY_FILETYPE)?.GetStringValue()
                                                             },
                                       Location = new Location
                                           {
                                               CountryName = doc.GetField(KEY_LOC_COUNTRY)?.GetStringValue(),
                                               State = doc.GetField(KEY_LOC_STATE)?.GetStringValue(),
                                               City = doc.GetField(KEY_LOC_CITY)?.GetStringValue(),
                                               SubLocation = doc.GetField(KEY_LOC_SUBLOCATION)?.GetStringValue(),
                                               CountryCode = doc.GetField(KEY_LOC_COUNTRY_CODE)?.GetStringValue(),
                                               Coordinate = new Coordinate
                                                                {
                                                                    Latitude = doc.GetField(KEY_LOC_LATITUDE)?.GetSingleValue() ?? 0,
                                                                    Longitude = doc.GetField(KEY_LOC_LONGITUDE)?.GetSingleValue() ?? 0,
                                                                }
                                            },
                                       DateTimeTaken = StringToTimestamp(doc.Get(KEY_DATE_TAKEN)),
                                       Persons = doc.GetValues(KEY_PERSON)?.ToList() ?? new List<string>(),
                                       Tags = doc.GetValues(KEY_TAG)?.ToList() ?? new List<string>(),
                                   };

                    results.Add(item);
                }

                // var dateString = DateTools.DateToString(data.DateTimeTaken.Value, PrecisionToResolution(data.DateTimeTaken.Precision));
                // doc.Add(new StringField("date", dateString, Field.Store.YES));
            }
            catch (Exception e)
            {
                Debug.Fail("Should not happen" + e.Message);
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

        private static Timestamp StringToTimestamp(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return new Timestamp();

            var result = new Timestamp
                             {
                                 Value = DateTools.StringToDate(dateString),
                             };

            switch (dateString.Length)
            {
                case 4:
                    result.Precision = TimestampPrecision.Year;
                    break;
                case 6:
                    result.Precision = TimestampPrecision.Month;
                    break;
                case 8:
                    result.Precision = TimestampPrecision.Day;
                    break;
                case 10:
                    result.Precision = TimestampPrecision.Hour;
                    break;
                case 12:
                    result.Precision = TimestampPrecision.Minute;
                    break;
                case 14:
                    result.Precision = TimestampPrecision.Second;
                    break;
                default:
                    result.Precision = TimestampPrecision.Second;
                    break;
            }

            return result;
        }

        private void RemoveFromIndexByFilename([NotNull] string filename)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(filename), "Filename should be filled in.");

            var term = new Term(KEY_FILENAME, filename);

            try
            {
                _indexWriter.DeleteDocuments(term);
                _indexWriter.Flush(true, true);
                _indexWriter.Commit();
            }
            catch (OutOfMemoryException)
            {
                _indexWriter.Dispose();
                throw;
            }
        }

        private void InitSpatial()
        {
            _spatialContext = SpatialContext.GEO;

            // Results in sub-meter precision for geohash
            int maxLevels = 11;

            // This can also be constructed from SpatialPrefixTreeFactory
            SpatialPrefixTree grid = new GeohashPrefixTree(_spatialContext, maxLevels);

            _spatialStrategy = new RecursivePrefixTreeStrategy(grid, KEY_LOC_GPS);
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