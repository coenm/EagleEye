namespace EagleEye.FileImporter.Scenarios.UpdatePicasaIni
{
    using EagleEye.Picasa.Picasa;

    public interface IPicaseIniFileWriter
    {
        void Write(string filename, PicasaIniFileUpdater updated, PicasaIniFile original);
    }
}