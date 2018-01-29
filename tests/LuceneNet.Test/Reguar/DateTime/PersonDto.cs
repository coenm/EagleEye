namespace LuceneNet.Test.Reguar.DateTime
{
    using System;

    public class PersonDto
    {
        public PersonDto()
        {
        }

        public PersonDto(string name, DateTime dateOfBirth)
        {
            Name = name;
            DateOfBirth = dateOfBirth;
        }

        public string Name { get; set; }

        public DateTime DateOfBirth { get; set; }
    }
}