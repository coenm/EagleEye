namespace EagleEye.FileImporter
{
    public class ConnectionStrings
    {
        public static readonly string LuceneInMemory = string.Empty;

        public ConnectionStrings()
        {
            ConnectionStringPhotoDatabase = "InMemory EagleEye";
        }

        public string Similarity { get; set; }

        public string HangFire { get; set; }

        public string FilenameEventStore { get; set; }

        /// <summary>
        /// Lucene storage directory. If <c>null</c> (or empty string) then InMemory index will be used; Relative directories are accepted.
        /// </summary>
        public string LuceneDirectory { get; set; }

        /// <summary>
        /// Connection string to be used in EntityFramework. Cannot be <c>null</c> or empty. Should start with 'InMemory' or with 'Filename='.
        /// </summary>
        public string ConnectionStringPhotoDatabase { get; set; }
    }
}
