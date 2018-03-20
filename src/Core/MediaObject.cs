namespace EagleEye.Core
{
    using System;
    using System.Collections.Generic;

    using JetBrains.Annotations;

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
            _persons = new List<string>();
            _tags = new List<string>();
        }

        public string Filename { get; }

        public IReadOnlyList<string> Persons => _persons;

        public IReadOnlyList<string> Tags => _tags;

        public Location Location { get; }

        [CanBeNull]
        public Timestamp DateTimeTaken { get; private set; }

        public void SetDateTimeTaken(int year, int month = -1, int day = -1, int hour = -1, int minutes = -1, int seconds = -1)
        {
            DateTimeTaken = new Timestamp(year, month, day, hour, minutes, seconds);
        }

        public void SetDateTimeTaken([NotNull] Timestamp value)
        {
            DateTimeTaken = value;
        }

        public void ClearDateTimeTaken()
        {
            DateTimeTaken = null;
        }

        public void AddPersons([NotNull] IEnumerable<string> persons)
        {
            foreach (var person in persons)
                AddPerson(person);
        }

        public void AddPerson(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            name = name.Trim();

            if (_tags.Contains(name))
                return;

            _persons.Add(name);
        }

        public void AddTags([NotNull] IEnumerable<string> tags)
        {
            foreach (var tag in tags)
                AddTag(tag);
        }

        public void AddTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return;

            tag = tag.Trim();

            if (_tags.Contains(tag))
                return;

            _tags.Add(tag);
        }
    }
}