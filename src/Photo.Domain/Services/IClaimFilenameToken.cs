namespace EagleEye.Photo.Domain.Services
{
    using System;

    internal interface IClaimFilenameToken : IDisposable
    {
        void Commit();
    }
}
