namespace EagleEye.Core.Domain.Commands
{
    using System;

    using CQRSlite.Commands;

    public class CreateMediaItemCommand : ICommand
    {
        public CreateMediaItemCommand(Guid id, string name, string[] tags, string[] persons)
        {
            Id = id;
            Tags = tags;
            Persons = persons;
            Name = name;
        }

        public Guid Id { get; set; }

        public string[] Tags { get; set; }

        public string[] Persons { get; set; }

        public string Name { get; }

        public int ExpectedVersion { get; set; }
    }
}
