namespace EagleEye.Picasa.PhotoProvider
{
    using System;
    using System.Threading.Tasks;

    using EagleEye.Picasa.Picasa;
    using JetBrains.Annotations;

    internal interface IPicasaService : IDisposable
    {
        bool CanProvideData([NotNull] string filename);

        Task<FileWithPersons> GetDataAsync([NotNull] string filename);
    }
}
