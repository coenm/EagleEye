namespace EagleEye.Photo.Domain.Services
{
    using JetBrains.Annotations;

    internal interface IUniqueFilenameService
    {
        [CanBeNull]
        IClaimFilenameToken Claim(string filename);
    }
}
