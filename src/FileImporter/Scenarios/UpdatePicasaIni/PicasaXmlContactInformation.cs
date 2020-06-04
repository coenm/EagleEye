namespace EagleEye.FileImporter.Scenarios.UpdatePicasaIni
{
    public readonly struct PicasaXmlContactInformation
    {
        public PicasaXmlContactInformation(string id, string fullName)
        {
            Id = id;
            FullName = fullName;
        }

        public string Id { get; }

        public string FullName { get; }
    }
}
