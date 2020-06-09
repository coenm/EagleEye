namespace EagleEye.FileImporter.Scenarios.UpdatePicasaIni
{
    using EagleEye.Picasa.Picasa;

    public interface IPicasaIniFileWriter
    {
        void Write(string filename, PicasaIniFileUpdater updated, PicasaIniFile original);
    }
}
