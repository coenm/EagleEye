namespace EagleEye.Picasa.Picasa
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    using Dawn;
    using JetBrains.Annotations;

    public class PicasaContactsXmlWriter
    {
        public void Write([NotNull] List<PicasaContact> contacts, Stream stream)
        {
            Guard.Argument(contacts, nameof(contacts)).NotNull();
            Guard.Argument(stream, nameof(stream)).NotNull();
            if (!stream.CanWrite)
                throw new ApplicationException("Cannot write to stream");

            var xmlDoc = new XmlDocument();

            XmlNode rootNode = xmlDoc.CreateElement("contacts");
            xmlDoc.AppendChild(rootNode);

            XmlAttribute CreateAttribute(string key, string value)
            {
                XmlAttribute attribute = xmlDoc.CreateAttribute(key);
                attribute.Value = value;
                return attribute;
            }

            foreach (var c in contacts)
            {
                XmlNode contactNode = xmlDoc.CreateElement("contact");

                void AddAttribute(string key, string value)
                {
                    if (value == null)
                        return;

                    contactNode.Attributes.Append(CreateAttribute(key, value));
                }

                AddAttribute("id", c.Id);
                AddAttribute("name", c.Name);
                AddAttribute("display", c.DisplayName);
                AddAttribute("modified_time", c.ModifiedDateTime);
                AddAttribute("local_contact", c.IsLocalContact);
                rootNode.AppendChild(contactNode);
            }

            xmlDoc.Save(stream);
        }
    }
}
