namespace EagleEye.Picasa.Picasa
{
    using System.Collections.Generic;

    public class PicasaIniFile
    {
        public PicasaIniFile(List<FileWithPersons> files, List<PicasaPerson> persons)
        {
            Files = files ?? new List<FileWithPersons>();
            Persons = persons ?? new List<PicasaPerson>();
        }

        public List<FileWithPersons> Files { get; }

        public List<PicasaPerson> Persons { get; }
    }
}
