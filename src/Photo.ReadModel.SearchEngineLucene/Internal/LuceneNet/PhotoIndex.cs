namespace EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.LuceneNet
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Interface;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.Model;
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
    using Spatial4n.Core.Context;

    using Directory = Lucene.Net.Store.Directory;

    internal class PhotoIndex : IPhotoIndex, IDisposable
    {
        private const string KeyId = "id";
        private const string KeyVersion = "version";
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

        [NotNull] private readonly Analyzer analyzer;
        [NotNull] private readonly IndexWriter indexWriter;
        [NotNull] private readonly SearcherManager searcherManager;
        [NotNull] private readonly QueryParser queryParser;
        [NotNull] private readonly Directory indexDirectory;

        private SpatialContext spatialContext;
        private SpatialStrategy spatialStrategy;

        public PhotoIndex([NotNull] ILuceneDirectoryFactory indexDirectoryFactory)
        {
            Guard.Argument(indexDirectoryFactory, nameof(indexDirectoryFactory)).NotNull();

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
                                                     new[] { KeyLocCity, KeyLocCountry, KeyId, KeyFilename, }, // todo define fields
                                                     analyzer);

            var indexWriterConfig = new IndexWriterConfig(LuceneNetVersion.Version, analyzer)
                                        {
                                            OpenMode = OpenMode.CREATE_OR_APPEND,
                                            RAMBufferSizeMB = 256.0,
                                        };

            indexWriter = new IndexWriter(indexDirectory, indexWriterConfig);

            searcherManager = new SearcherManager(indexWriter, true, null);
        }

        public Task<bool> ReIndexMediaFileAsync([NotNull] Photo data)
        {
            Guard.Argument(data, nameof(data)).NotNull();

            RemoveFromIndexByFilename(data.FileName);
            RemoveFromIndexByGuid(data.Id);

            var doc = new Document
            {
                // file information
                new StringField(KeyId, data.Id.ToString(), Field.Store.YES),

                new NumericDocValuesField(KeyVersion, data.Version),
                new StoredField(KeyVersion, data.Version),

                new StringField(KeyFilename, data.FileName ?? string.Empty, Field.Store.YES),
                new StringField(KeyFileType, data.FileMimeType ?? string.Empty, Field.Store.YES),

                // location data
                new TextField(KeyLocCity, data.LocationCity ?? string.Empty, Field.Store.YES),
                new TextField(KeyLocCountryCode, data.LocationCountryCode ?? string.Empty, Field.Store.YES),
                new TextField(KeyLocCountry, data.LocationCountryName ?? string.Empty, Field.Store.YES),
                new TextField(KeyLocState, data.LocationState ?? string.Empty, Field.Store.YES),
                new TextField(KeyLocSubLocation, data.LocationSubLocation ?? string.Empty, Field.Store.YES),
                new StoredField(KeyLocLongitude, data.LocationLongitude ?? 0),
                new StoredField(KeyLocLatitude, data.LocationLatitude ?? 0),
            };

            // index coordinate
            var x = data.LocationLongitude;
            var y = data.LocationLatitude;

            if (x != null && y != null)
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

            if (data.DateTimeTaken != null)
            {
                var dateString = DateTools.DateToString(
                    data.DateTimeTaken.Value,
                    PrecisionToResolution(data.DateTimeTaken.Precision));

                doc.Add(new StringField(KeyDateTaken, dateString, Field.Store.YES));
            }

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
                indexWriter.UpdateDocument(new Term(KeyId, data.Id.ToString()), doc);
            }

            indexWriter.Flush(true, true);
            indexWriter.Commit();

            // expensive
            // _indexWriter.ForceMerge(1);
            return Task.FromResult(true);
        }

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
            }

            throw new Exception("No items found.");
        }

        public PhotoSearchResult Search(Guid guid)
        {
            if (guid == Guid.Empty)
                return null;

            var term = new Term(KeyId, guid.ToString());
            var query = new TermQuery(term);
            return Search(query, null, out _).SingleOrDefault();
        }

        [NotNull]
        public List<PhotoSearchResult> Search(string queryString, out int totalHits)
        {
            // Parse the query - assuming it's not a single term but an actual query string
            // the QueryParser used is using the same analyzer used for indexing
            var query = queryParser.Parse(queryString);
            return Search(query, null, out totalHits);
        }

        [NotNull]
        public List<PhotoSearchResult> Search([NotNull] Query query, [CanBeNull] Filter filter, out int totalHits)
        {
            Guard.Argument(query, nameof(query)).NotNull();

            var results = new List<PhotoSearchResult>();
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
                    var item = new PhotoSearchResult(result.Score)
                                   {
                                       Id = GetId(doc),
                                       Version = doc.GetField(KeyVersion)?.GetInt32Value() ?? 0,
                                       FileName = doc.GetField(KeyFilename)?.GetStringValue(),
                                       FileMimeType = doc.GetField(KeyFileType)?.GetStringValue(),
                                       LocationCountryName = doc.GetField(KeyLocCountry)?.GetStringValue(),
                                       LocationState = doc.GetField(KeyLocState)?.GetStringValue(),
                                       LocationCity = doc.GetField(KeyLocCity)?.GetStringValue(),
                                       LocationSubLocation = doc.GetField(KeyLocSubLocation)?.GetStringValue(),
                                       LocationCountryCode = doc.GetField(KeyLocCountryCode)?.GetStringValue(),
                                       LocationLatitude = doc.GetField(KeyLocLatitude)?.GetSingleValue() ?? 0,
                                       LocationLongitude = doc.GetField(KeyLocLongitude)?.GetSingleValue() ?? 0,
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
            }

            return results;
        }

        public void Dispose()
        {
            analyzer.Dispose();
            indexWriter.Dispose();
            searcherManager.Dispose();
            indexDirectory.Dispose();
        }

        [CanBeNull]
        private static Model.Timestamp StringToTimestamp(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return null;

            Model.TimestampPrecision precision;

            switch (dateString.Length)
            {
            case 4:
                precision = Model.TimestampPrecision.Year;
                break;
            case 6:
                precision = Model.TimestampPrecision.Month;
                break;
            case 8:
                precision = Model.TimestampPrecision.Day;
                break;
            case 10:
                precision = Model.TimestampPrecision.Hour;
                break;
            case 12:
                precision = Model.TimestampPrecision.Minute;
                break;
            case 14:
                precision = Model.TimestampPrecision.Second;
                break;
            default:
                precision = Model.TimestampPrecision.Second;
                break;
            }

            return new Model.Timestamp(DateTools.StringToDate(dateString), precision);
        }

        private Guid GetId([NotNull] Document doc)
        {
            Guard.Argument(doc, nameof(doc)).NotNull();

            var guidString = doc.GetField(KeyId)?.GetStringValue();
            if (string.IsNullOrWhiteSpace(guidString))
                return Guid.Empty;

            if (Guid.TryParse(guidString, out var id))
                return id;

            return Guid.Empty;
        }

        private void RemoveFromIndexByFilename([NotNull] string filename)
        {
            Guard.Argument(filename, nameof(filename)).NotNull().NotWhiteSpace();
            DeleteByTerm(new Term(KeyFilename, filename));
        }

        private void RemoveFromIndexByGuid(Guid guid)
        {
            if (guid == Guid.Empty)
                return;

            DeleteByTerm(new Term(KeyId, guid.ToString()));
        }

        private void DeleteByTerm([NotNull] Term term)
        {
            Guard.Argument(term, nameof(term)).NotNull();

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

        private DateTools.Resolution PrecisionToResolution(Model.TimestampPrecision precision)
        {
            switch (precision)
            {
                case Model.TimestampPrecision.Year:
                    return DateTools.Resolution.YEAR;
                case Model.TimestampPrecision.Month:
                    return DateTools.Resolution.MONTH;
                case Model.TimestampPrecision.Day:
                    return DateTools.Resolution.DAY;
                case Model.TimestampPrecision.Hour:
                    return DateTools.Resolution.HOUR;
                case Model.TimestampPrecision.Minute:
                    return DateTools.Resolution.MINUTE;
                case Model.TimestampPrecision.Second:
                    return DateTools.Resolution.SECOND;
                default:
                    throw new ArgumentOutOfRangeException(nameof(precision), precision, null);
            }
        }
    }
}
