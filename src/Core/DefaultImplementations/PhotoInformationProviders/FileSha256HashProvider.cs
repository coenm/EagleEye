namespace EagleEye.Core.DefaultImplementations.PhotoInformationProviders
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.Interfaces.Core;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using JetBrains.Annotations;

    [UsedImplicitly]
    public class FileSha256HashProvider : IFileSha256HashProvider
    {
        [NotNull]
        private readonly IFileService fileService;

        public FileSha256HashProvider([NotNull] IFileService fileService)
        {
            Guard.Argument(fileService, nameof(fileService)).NotNull();
            this.fileService = fileService;
        }

        public string Name => nameof(IPhotoSha256HashProvider);

        public uint Priority => 10;

        public bool CanProvideInformation(string filename)
        {
            return !string.IsNullOrWhiteSpace(filename); // file exists?
        }

        public Task<ReadOnlyMemory<byte>> ProvideAsync(string filename)
        {
            Guard.Argument(filename, nameof(filename)).NotNull().NotEmpty();

            try
            {
                var result = Provide(filename);
                return Task.FromResult(result);
            }
            catch (Exception)
            {
                return Task.FromResult<ReadOnlyMemory<byte>>(null);
            }
        }

        private static byte[] CalculateStreamHash([NotNull] Stream input)
        {
            Guard.Argument(input, nameof(input)).NotNull();
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(input);
        }

        private ReadOnlyMemory<byte> Provide(string filename)
        {
            Guard.Argument(filename, nameof(filename)).NotNull().NotEmpty();

            using var stream = fileService.OpenRead(filename);
            var result = CalculateStreamHash(stream);
            return new ReadOnlyMemory<byte>(result);
        }
    }
}
