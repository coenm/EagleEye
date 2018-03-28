namespace SearchEngine.LuceneNet.Core.Index
{
    using System.IO;

    using Lucene.Net.Analysis;
    using Lucene.Net.Analysis.CharFilters;
    using Lucene.Net.Analysis.Core;
    using Lucene.Net.Analysis.Standard;
    using Lucene.Net.Util;

    internal class HtmlStripAnalyzer : Analyzer
    {
        private readonly LuceneVersion _luceneVersion;

        public HtmlStripAnalyzer(LuceneVersion luceneVersion)
        {
            _luceneVersion = luceneVersion;
        }

        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            StandardTokenizer standardTokenizer = new StandardTokenizer(_luceneVersion, reader);
            TokenStream stream = new StandardFilter(_luceneVersion, standardTokenizer);
            stream = new LowerCaseFilter(_luceneVersion, stream);
            stream = new StopFilter(_luceneVersion, stream, StopAnalyzer.ENGLISH_STOP_WORDS_SET);
            return new TokenStreamComponents(standardTokenizer, stream);
        }

        protected override TextReader InitReader(string fieldName, TextReader reader)
        {
            return base.InitReader(fieldName, new HTMLStripCharFilter(reader));
        }
    }
}