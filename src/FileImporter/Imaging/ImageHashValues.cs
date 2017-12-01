namespace FileImporter.Imaging
{
    public struct ImageHashValues
    {
        public byte[] FileHash { get; set; }
        public byte[] ImageHash { get; set; }
        public ulong AverageHash { get; set; }
        public ulong DifferenceHash { get; set; }
        public ulong PerceptualHash { get; set; }
    }
}