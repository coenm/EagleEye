namespace EagleEye.LuceneNet.Test.Regular.DateTime
{
    public class PersonDto
    {
        public PersonDto()
        {
        }

        public PersonDto(string name, System.DateTime dateOfBirth)
        {
            Name = name;
            DateOfBirth = dateOfBirth;
        }

        public string Name { get; set; }

        public System.DateTime DateOfBirth { get; set; }
    }
}
