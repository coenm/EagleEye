namespace EagleEye.Core
{
    using System;
    using System.Collections.Generic;

    public class MediaObject
    {
        private readonly List<string> _persons;
        private readonly List<string> _tags;

        public MediaObject(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            Filename = filename;
            Location = new Location();
            Camera = new Camera();
            _persons = new List<string>();
            _tags = new List<string>();
        }

        public string Filename { get; }

        public IReadOnlyList<string> Persons => _persons;

        public IReadOnlyList<string> Tags => _tags;

        public Location Location { get; }

        public Camera Camera { get; }

        public void AddPerson(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            // todo check if name is already in there.

            _persons.Add(name.Trim());
        }

        public void AddTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentNullException(nameof(tag));

            // todo check if name is already in there.

            _tags.Add(tag.Trim());
        }
    }
}