namespace EagleEye.FileImporter.Similarity
{
    public readonly struct FilenameProgressData
    {
        public FilenameProgressData(int current, int total, string filename)
        {
            Current = current;
            Total = total;
            Filename = filename;
        }

        public string Filename { get; }

        public int Current { get; }

        public int Total { get; }
    }
}
