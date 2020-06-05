namespace EagleEye.FileImporter.Scenarios.UpdatePicasaIni
{
    using System.Collections.Generic;

    using EagleEye.Picasa.Picasa;

    public interface IPicasaIniFileProvider
    {
        IEnumerable<PicasaIniFile> GetBackups(string originalFilename);

        PicasaIniFile Get(string filename);
    }
}