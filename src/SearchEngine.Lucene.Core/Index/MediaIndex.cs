namespace SearchEngine.LuceneNet.Core.Index
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using Helpers.Guards;
    using JetBrains.Annotations;

    using Lucene.Net.Analysis;
    using Lucene.Net.Analysis.Standard;
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.QueryParsers.Classic;
    using Lucene.Net.Search;
    using Lucene.Net.Spatial;
    using Lucene.Net.Spatial.Prefix;
    using Lucene.Net.Spatial.Prefix.Tree;

    using SearchEngine.Interface.Commands.ParameterObjects;
    using SearchEngine.LuceneNet.Core.Internals;

    using Spatial4n.Core.Context;

    using Directory = Lucene.Net.Store.Directory;

    public class MediaIndex : IDisposable
    {
        private const string KeyFilename = "filename";
        private const string KeyFileType = "filetype";
        private const string KeyLocCity = "city";
        private const string KeyLocCountryCode = "countrycode";
        private const string KeyLocCountry = "country";
        private const string KeyLocState = "state";
        private const string KeyLocSubLocation = "sublocation";
        private const string KeyLocLongitude = "longitude";
        private const string KeyLocLatitude = "latitude";
        private const string KeyDateTaken = "date";
        private const string KeyPerson = "person";
        private const string KeyTag = "tag";
        private const string KeyLocGps = "gps";

        private readonly Analyzer analyzer;
        private readonly IndexWriter indexWriter;
        private readonly SearcherManager searcherManager;
        private readonly QueryParser queryParser;
        private readonly Directory indexDirectory;

        private SpatialContext spatialContext;
        private SpatialStrategy spatialStrategy;

        public MediaIndex([NotNull] ILuceneDirectoryFactory indexDirectoryFactory)
        {
            indexDirectory = indexDirectoryFactory.Create();

            InitSpatial();
            analyzer = new StandardAnalyzer(LuceneNetVersion.Version);
/*
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
*/

            queryParser = new MultiFieldQueryParser(
                                                     LuceneNetVersion.Version,
                                                     new[] { KeyLocCity, KeyLocCountry }, // todo define fields
                                                     analyzer);

            var indexWriterConfig = new IndexWriterConfig(LuceneNetVersion.Version, analyzer)
                                        {
                                            OpenMode = OpenMode.CREATE_OR_APPEND,
                                            RAMBufferSizeMB = 256.0,
                                        };

            indexWriter = new IndexWriter(indexDirectory, indexWriterConfig);

            searcherManager = new SearcherManager(indexWriter, true, null);
        }

        [PublicAPI]
        public Task<bool> IndexMediaFileAsync([NotNull] MediaObject data)
        {
            Guard.NotNull(data, nameof(data));
            Guard.NotNull(data.FileInformation, nameof(data.FileInformation)); // todo verification of data object
            Guard.NotNullOrWhiteSpace(data.FileInformation.Filename, nameof(data.FileInformation.Filename));

            RemoveFromIndexByFilename(data.FileInformation.Filename);

            var doc = new Document
            {
                // file information
                new StringField(KeyFilename, data.FileInformation?.Filename ?? string.Empty, Field.Store.YES),
                new StringField(KeyFileType, data.FileInformation?.Type ?? string.Empty, Field.Store.YES),

                // location data
                new TextField(KeyLocCity, data.Location?.City ?? string.Empty, Field.Store.YES),
                new TextField(KeyLocCountryCode, data.Location?.CountryCode ?? string.Empty, Field.Store.YES),
                new TextField(KeyLocCountry, data.Location?.CountryName ?? string.Empty, Field.Store.YES),
                new TextField(KeyLocState, data.Location?.State ?? string.Empty, Field.Store.YES),
                new TextField(KeyLocSubLocation, data.Location?.SubLocation ?? string.Empty, Field.Store.YES),
                new StoredField(KeyLocLongitude, data.Location?.Coordinate?.Longitude ?? 0),
                new StoredField(KeyLocLatitude, data.Location?.Coordinate?.Latitude ?? 0),
            };

            // index coordinate
            var x = data.Location?.Coordinate?.Longitude;
            var y = data.Location?.Coordinate?.Latitude;

            if (x != null)
            {
                var p = spatialContext.MakePoint(x.Value, y.Value);

                foreach (var shape in new[] { p })
                {
                    foreach (var field in spatialStrategy.CreateIndexableFields(shape))
                    {
                        doc.Add(field);
                    }

                    // var pt = (IPoint)shape;
                    // doc.Add(new StoredField(_strategy.FieldName, pt.X.ToString(CultureInfo.InvariantCulture) + " " + pt.Y.ToString(CultureInfo.InvariantCulture)));
                }
            }

            var dateString = DateTools.DateToString(
                data.DateTimeTaken.Value,
                PrecisionToResolution(data.DateTimeTaken.Precision));
            doc.Add(new StringField(KeyDateTaken, dateString, Field.Store.YES));

            foreach (var person in data.Persons ?? new List<string>())
            {
                doc.Add(new TextField(KeyPerson, person, Field.Store.YES));
            }

            foreach (var tag in data.Tags ?? new List<string>())
            {
                doc.Add(new TextField(KeyTag, tag, Field.Store.YES));
            }

            if (indexWriter.Config.OpenMode == OpenMode.CREATE)
            {
                // New index, so we just add the document (no old document can be there):
                indexWriter.AddDocument(doc);
            }
            else
            {
                // Existing index (an old copy of this document may have been indexed) so
                // we use updateDocument instead to replace the old one matching the exact
                // path, if present:
                indexWriter.UpdateDocument(new Term(KeyFilename, data.FileInformation.Filename), doc);
            }

            indexWriter.Flush(true, true);
            indexWriter.Commit();

            // expensive
            // _indexWriter.ForceMerge(1);
            return Task.FromResult(true);
        }

        [PublicAPI]
        public int Count([CanBeNull] Query query = null, [CanBeNull] Filter filter = null)
        {
            // Execute the search with a fresh indexSearcher
            searcherManager.MaybeRefreshBlocking();

            var searcher = searcherManager.Acquire();

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
                searcherManager.Release(searcher);
                searcher = null; // Don't use searcher after this point!
            }

            throw new Exception("No items found.");
        }

        [PublicAPI]
        public List<MediaResult> Search(string queryString, out int totalHits)
        {
            // Parse the query - assuming it's not a single term but an actual query string
            // the QueryParser used is using the same analyzer used for indexing
            var query = queryParser.Parse(queryString);
            return Search(query, null, out totalHits);
        }

        [PublicAPI]
        public List<MediaResult> Search([NotNull] Query query, [CanBeNull] Filter filter, out int totalHits)
        {
            var results = new List<MediaResult>();
            totalHits = 0;

            // Execute the search with a fresh indexSearcher
            searcherManager.MaybeRefreshBlocking();

            var searcher = searcherManager.Acquire();

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
                                                                 Filename = doc.GetField(KeyFilename)?.GetStringValue(),
                                                                 Type = doc.GetField(KeyFileType)?.GetStringValue(),
                                                             },
                                       Location = new Location
                                           {
                                               CountryName = doc.GetField(KeyLocCountry)?.GetStringValue(),
                                               State = doc.GetField(KeyLocState)?.GetStringValue(),
                                               City = doc.GetField(KeyLocCity)?.GetStringValue(),
                                               SubLocation = doc.GetField(KeyLocSubLocation)?.GetStringValue(),
                                               CountryCode = doc.GetField(KeyLocCountryCode)?.GetStringValue(),
                                               Coordinate = new Coordinate
                                                                {
                                                                    Latitude = doc.GetField(KeyLocLatitude)?.GetSingleValue() ?? 0,
                                                                    Longitude = doc.GetField(KeyLocLongitude)?.GetSingleValue() ?? 0,
                                                                },
                                           },
                                       DateTimeTaken = StringToTimestamp(doc.Get(KeyDateTaken)),
                                       Persons = doc.GetValues(KeyPerson)?.ToList() ?? new List<string>(),
                                       Tags = doc.GetValues(KeyTag)?.ToList() ?? new List<string>(),
                                   };

                    results.Add(item);
                }
            }
            catch (Exception e)
            {
                // do nothing
                Debug.Fail("Should not happen" + e.Message);
            }
            finally
            {
                searcherManager.Release(searcher);
                searcher = null; // Don't use searcher after this point!
            }

            return results;
        }

        public void Dispose()
        {
            analyzer?.Dispose();
            indexWriter?.Dispose();
            searcherManager?.Dispose();
            indexDirectory?.Dispose();
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
            DebugGuard.NotNullOrWhiteSpace(filename, nameof(filename));

            var term = new Term(KeyFilename, filename);

            try
            {
                indexWriter.DeleteDocuments(term);
                indexWriter.Flush(true, true);
                indexWriter.Commit();
            }
            catch (OutOfMemoryException)
            {
                indexWriter.Dispose();
                throw;
            }
        }

        private void InitSpatial()
        {
            spatialContext = SpatialContext.GEO;

            // Results in sub-meter precision for geohash
            var maxLevels = 11;

            // This can also be constructed from SpatialPrefixTreeFactory
            SpatialPrefixTree grid = new GeohashPrefixTree(spatialContext, maxLevels);

            spatialStrategy = new RecursivePrefixTreeStrategy(grid, KeyLocGps);
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
