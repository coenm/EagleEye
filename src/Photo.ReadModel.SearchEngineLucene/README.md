# Lucene todo's
- Improve analyzers and tokenizers; For now, the default will do.
- Specify MultiFieldQueryParser fields;
- Update /research WriterConfig;
- Index Tags and persons improvement. Ie. Indexing persons {"Michael Jackson", "Elvis Presley "} and searching for "Jackson Elvis" should not match.
- Queries and filter improvement and research;
  - Different types of Queries and Filters;
  - Range query/filter for date taken;
  - Location based on GPS with radius in (kilo)meters;
  - Tags and Person facet search;
- When to commit, flush, ForceMerge the index writer?
- Exception handling and cleaning;


## Links
- [Facet userguide](https://lucene.apache.org/core/4_1_0/facet/org/apache/lucene/facet/doc-files/userguide.html)