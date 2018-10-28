namespace EagleEye.Core
{
    using JetBrains.Annotations;

    public class FileInformation
    {
        public FileInformation(string filename)
        {
            Filename = filename;
        }

        public string Filename { get;  }

        public string Type { get; set; }

        public void SetType([NotNull] string type)
        {
            Type = type;
        }
    }
}