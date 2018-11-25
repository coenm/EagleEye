namespace SearchEngine.LuceneNet.ReadModel.Internal.LuceneNet
{
    using System.IO;

    using Lucene.Net.Analysis;
    using Lucene.Net.Analysis.CharFilters;
    using Lucene.Net.Analysis.Core;
    using Lucene.Net.Analysis.Standard;
    using Lucene.Net.Util;

    internal class HtmlStripAnalyzer : Analyzer
    {
        private readonly LuceneVersion luceneVersion;

        public HtmlStripAnalyzer(LuceneVersion luceneVersion)
        {
            this.luceneVersion = luceneVersion;
        }

        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            StandardTokenizer standardTokenizer = new StandardTokenizer(luceneVersion, reader);
            TokenStream stream = new StandardFilter(luceneVersion, standardTokenizer);
            stream = new LowerCaseFilter(luceneVersion, stream);
            stream = new StopFilter(luceneVersion, stream, StopAnalyzer.ENGLISH_STOP_WORDS_SET);
            return new TokenStreamComponents(standardTokenizer, stream);
        }

        protected override TextReader InitReader(string fieldName, TextReader reader)
        {
            return base.InitReader(fieldName, new HTMLStripCharFilter(reader));
        }
    }
}
