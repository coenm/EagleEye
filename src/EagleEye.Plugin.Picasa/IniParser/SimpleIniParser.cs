namespace EagleEye.Picasa.IniParser
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Dawn;
    using JetBrains.Annotations;

    public static class SimpleIniParser
    {
        private static readonly string[] KeyValueSeparator;

        static SimpleIniParser()
        {
            KeyValueSeparator = new[] { "=" };
        }

        public static List<IniData> Parse([NotNull] Stream input)
        {
            Guard.Argument(input, nameof(input)).NotNull();

            var content = GetContent(input);
            return ParseContent(content);
        }

        private static string[] GetContent([NotNull] Stream stream)
        {
            Guard.Argument(stream, nameof(stream)).NotNull();

            try
            {
                using var reader = new StreamReader(stream);
                var content = reader.ReadToEnd();
                return content
                       .Replace("\r\n", "\n")
                       .Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
            catch (Exception e)
            {
                throw new Exception("Could not parse stream", e);
            }
        }

        private static List<IniData> ParseContent(string[] content)
        {
            var result = new List<IniData>();
            var currentSection = new IniData("dummy");

            var count = content.Length;

            for (var i = 0; i < count; i++)
            {
                var line = content[i];

                if (IsCommentLine(line))
                    continue;

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

        private static bool IsCommentLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return true;

            if (line.StartsWith(";"))
                return true;

            return false;
        }

        private static bool TryGetIniSection([NotNull] string line, out string iniSection)
        {
            Guard.Argument(line, nameof(line)).NotNull();

            iniSection = string.Empty;

            line = line.Trim();

            if (!line.StartsWith("["))
                return false;

            if (!line.EndsWith("]"))
                return false;

            iniSection = line.Substring(1, line.Length - 2);
            return true;
        }

        private static (string key, string value) GetKeyValueFromIni(string line)
        {
            Guard.Argument(line, nameof(line)).NotNull().NotWhiteSpace();

            line = line.Trim();

            var result = line.Split(KeyValueSeparator, 2, StringSplitOptions.RemoveEmptyEntries);

            if (result.Length != 2)
                throw new ArgumentException($"Cannot parse {line}");

            return (result[0].Trim(), result[1].Trim());
        }
    }
}
