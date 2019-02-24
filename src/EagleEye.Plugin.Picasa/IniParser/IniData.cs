namespace EagleEye.Picasa.IniParser
{
    using System.Collections.Generic;

    public class IniData
    {
        public IniData(string section)
        {
            Section = section;
            Content = new Dictionary<string, string>();
        }

        public string Section { get; }

        public Dictionary<string, string> Content { get; }

        public void AddContentLine(string key, string value)
        {
            Content.Add(key, value);
        }
    }
}