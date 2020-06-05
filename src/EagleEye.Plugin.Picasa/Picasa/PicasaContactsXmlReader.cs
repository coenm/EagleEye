namespace EagleEye.Picasa.Picasa
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    using Dawn;
    using EagleEye.Core.Interfaces.Core;
    using JetBrains.Annotations;

    public class PicasaContactsXmlReader
    {
        [NotNull] private readonly IFileService fileService;

        public PicasaContactsXmlReader([NotNull] IFileService fileService)
        {
            Guard.Argument(fileService, nameof(fileService)).NotNull();
            this.fileService = fileService;
        }

        public List<PicasaPerson> GetContactsFromFile([NotNull] string xmlFilename)
        {
            Guard.Argument(xmlFilename, nameof(xmlFilename)).NotNull().NotEmpty();
            if (fileService.FileExists(xmlFilename))
                throw new FileNotFoundException(nameof(xmlFilename));

            using var stream = fileService.OpenRead(xmlFilename);

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(stream);
            XmlNodeList nodes = xmlDoc.SelectNodes("contacts/contact");
            if (nodes == null)
                return new List<PicasaPerson>(0);

            var result = new List<PicasaPerson>(nodes.Count);
            foreach (XmlNode node in nodes)
            {
                var item = Convert(node);
                if (item != null)
                    result.Add(item.Value);
            }

            return result;
        }

        private static PicasaPerson? Convert(XmlNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (node.Attributes == null)
                return null;

            var idAttribute = node.Attributes["id"];
            if (idAttribute == null)
                return null;

            var id = idAttribute.Value;
            if (string.IsNullOrWhiteSpace(id))
                return null;

            var nameAttribute = node.Attributes["name"];
            if (nameAttribute == null)
                return null;

            var name = nameAttribute.Value;
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return new PicasaPerson(id, name);
        }
    }
}
