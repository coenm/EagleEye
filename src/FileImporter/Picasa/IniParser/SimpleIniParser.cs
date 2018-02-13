using System;
using System.Collections.Generic;
using System.IO;

namespace EagleEye.FileImporter.Picasa.IniParser
{
    public static class SimpleIniParser
    {
        public static List<IniData> Parse(Stream input)
        {
            var content = GetContent(input);

            return ParseContent(content);
        }

        private static string[] GetContent(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            try
            {
                using (var reader = new StreamReader(stream))
                {
                    var content = reader.ReadToEnd();
                    return  content.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);
                }
            }
            catch (Exception)
            {
                throw new Exception("Could not parse stream");
            }
        }

        private static List<IniData> ParseContent(string[] content)
        {
            var result = new List<IniData>();
            var currentSection = new IniData("dummy");

            var count = content.Length;

            for (int i = 0; i < count; i++)
            {
                var line = content[i];
                if (TryGetIniSection(line, out var iniSection))
                {
                    currentSection = new IniData(iniSection);
                    result.Add(currentSection);
                    continue;
                }

                (string key, string value) = GetKeyValueFromIni(line);
                currentSection.AddContentLine(key, value);
            }

            return result;
        }



        private static bool TryGetIniSection(string line, out string iniSection)
        {
            iniSection = "";

            if (string.IsNullOrWhiteSpace(line))
                return false;

            line = line.Trim();

            if (!line.StartsWith('['))
                return false;

            if (!line.EndsWith(']'))
                return false;

            iniSection = line.Substring(1, line.Length - 2);
            return true;
        }

        private static (string key, string value) GetKeyValueFromIni(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                throw new ArgumentNullException();

            line = line.Trim();

            var result = line.Split('=', StringSplitOptions.RemoveEmptyEntries);

            if (result.Length != 2)
                throw new ArgumentException($"Cannot parse {line}");
            return (result[0].Trim(), result[1].Trim());
        }
    }
}