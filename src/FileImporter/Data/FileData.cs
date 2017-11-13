namespace FileImporter.Data
{
    public class FileData
    {
        public string FileName { get; set; }

        public long SizeInBytes { get; set; }

        public byte[] Sha256 { get; set; }
    }
}