namespace EagleEye.Photo.Domain.Services
{
    using JetBrains.Annotations;

    internal interface IUniqueFilenameService
    {
        /// <summary>
        /// Claim filename to be used as it is unique.
        /// </summary>
        /// <param name="filename">string, cannot be null.</param>
        /// <returns><c>null</c> when filename cannot be claimed, or token instance to use filename. </returns>
        [CanBeNull]
        IClaimFilenameToken Claim(string filename);
    }
}
