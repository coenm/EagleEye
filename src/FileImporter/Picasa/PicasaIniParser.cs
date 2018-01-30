using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileImporter.Picasa.IniParser;

namespace FileImporter.Picasa
{
    public static class PicasaIniParser
    {
        private const string Contacts2Section = "Contacts2";
        private const string FacesKey = "faces";

        public static IEnumerable<FileWithPersons> Parse(Stream stream)
        {
            var iniContent = SimpleIniParser.Parse(stream);

            var contacts = iniContent.SingleOrDefault(x => x.Section == Contacts2Section);
            if (contacts == null)
                throw new Exception($"{Contacts2Section} not found");
            
            var result = new List<FileWithPersons>(iniContent.Count - 1);

            foreach (var item in iniContent.Where(x => x != contacts))
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

                        var singleCoordinateAndKey = faceCoordinateKey.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        
                        // expect only two items
                        if (singleCoordinateAndKey.Length == 2)
                        {
                            var personName = GetName(singleCoordinateAndKey[1], contacts);
                            if (!string.IsNullOrWhiteSpace(personName))
                                fileWithPersons.AddPerson(personName);
                        }
                    }
                }

                result.Add(fileWithPersons);
            }

            return result;
        }

        private static string GetName(string key, IniData contacts)
        {
            if (!contacts.Content.ContainsKey(key))
                return string.Empty;

            var contact =  contacts.Content[key];

            while (contact.EndsWith(';'))
                contact = contact.Substring(0, contact.Length - 1);

            return contact;
        }
    }
}