namespace EagleEye.Picasa.Picasa
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using EagleEye.Picasa.IniParser;
    using JetBrains.Annotations;

    /// <summary>
    /// Picasa ini parser to obtain faces.
    /// </summary>
    /// <remarks>See <see href="https://gist.github.com/fbuchinger/1073823" /> for more information about reading picasa.ini files.</remarks>
    public static class PicasaIniParser
    {
        private const string Contacts2Section = "Contacts2";
        private const string FacesKey = "faces";
        private static readonly string[] CoordinatePersonSeparator = { "," };

        [CanBeNull]
        public static PicasaIniFile Parse(Stream stream)
        {
            var iniContent = SimpleIniParser.Parse(stream);

            if (iniContent == null || iniContent.Count == 0)
                return null;

            var iniContacts = iniContent.SingleOrDefault(x => x.Section == Contacts2Section);
            var contacts = ParseIniContacts(iniContacts).ToList();

            var result = new List<FileWithPersons>(iniContent.Count);

            foreach (var item in iniContent.Where(x => x != iniContacts))
            {
                var fileWithPersons = new FileWithPersons(item.Section);
                var facesList = item.Content.Where(x => x.Key == FacesKey).ToList();
                if (facesList.Count == 1)
                {
                    var facesString = facesList.Single().Value;

                    // rect64(9ee42f2ee2e49bfa),4759b81b11610b7a;rect64(9ee42f2ee2e49bfa),4759b81b11610b7a
                    // first split on ';'
                    var facesCoordinateKey = facesString.Split(';');
                    foreach (var faceCoordinateKey in facesCoordinateKey)
                    {
                        // like: rect64(9ee42f2ee2e49bfa),4759b81b11610b7a
                        // means: <coordinate>,<person key>
                        var singleCoordinateAndKey = faceCoordinateKey.Split(CoordinatePersonSeparator, StringSplitOptions.RemoveEmptyEntries);

                        // expect only two items
                        if (singleCoordinateAndKey.Length != 2)
                            continue;

                        var coordinate = DecodeRect64ToRelativeCoordinates(ref singleCoordinateAndKey[0]);
                        var contact = GetOrCreateContact(ref singleCoordinateAndKey[1], contacts);
                        fileWithPersons.AddPerson(new PicasaPersonLocation(contact, coordinate));
                    }
                }

                result.Add(fileWithPersons);
            }

            return new PicasaIniFile(result, contacts);
        }

        private static RelativeRegion? DecodeRect64ToRelativeCoordinates(ref string rect64)
        {
            const int expectedLength = 7 + 16 + 1;
            if (rect64 == null)
                return null;
            if (rect64.Length != expectedLength)
                return null;
            if (!rect64.StartsWith("rect64("))
                return null;
            if (!rect64.EndsWith(")"))
                return null;

            const int rect64StringLength = 7; // length of "rect64("

            var left = FromString(ref rect64, 0 + rect64StringLength);
            var top = FromString(ref rect64, 4 + rect64StringLength);
            var right = FromString(ref rect64, 8 + rect64StringLength);
            var bottom = FromString(ref rect64, 12 + rect64StringLength);

            var relativeLeft = (float)left / (float)ushort.MaxValue;
            var relativeTop = (float)top / (float)ushort.MaxValue;
            var relativeRight = (float)right / (float)ushort.MaxValue;
            var relativeBottom = (float)bottom / (float)ushort.MaxValue;

            return new RelativeRegion(relativeLeft, relativeTop, relativeRight, relativeBottom);
        }

        private static ushort FromString(ref string input, int startIndex)
        {
            var bytes = StringToByteArray(input.Substring(startIndex, 4));

            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();

            return BitConverter.ToUInt16(bytes, 0);
        }

        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        private static IEnumerable<PicasaPerson> ParseIniContacts([CanBeNull] IniData iniContacts)
        {
            if (iniContacts == null)
                yield break;

            if (iniContacts.Section != Contacts2Section)
                yield break;

            foreach (var item in iniContacts.Content)
            {
                var name = item.Value;
                while (name.EndsWith(";"))
                    name = name.Substring(0, name.Length - 1);
                yield return new PicasaPerson(item.Key, name);
            }

        }

        private static PicasaPerson GetOrCreateContact(ref string key, IEnumerable<PicasaPerson> contacts)
        {
            foreach (var c in contacts)
            {
                if (c.Id == key)
                    return c;
            }

            return new PicasaPerson(key, string.Empty);
        }
    }
}
