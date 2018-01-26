using System.Collections.Generic;

namespace LuceneNet.Test.Facet
{
    internal static class StaticDocuments
    {
        public static IEnumerable<DocumentDto> Items
        {
            get
            {
                yield return new DocumentDto("radio", 50);
                yield return new DocumentDto("television", 600);
                yield return new DocumentDto("house", 600000);
                yield return new DocumentDto("beer", 2);
                yield return new DocumentDto("cola", 3);
                yield return new DocumentDto("car", 20000);
                yield return new DocumentDto("laptop", 650);
            }
        }
    }
}