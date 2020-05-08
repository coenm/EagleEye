namespace EagleEye.ExifTool.EagleEyeXmp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CoenM.Encoding;
    using Dawn;
    using EagleEye.Core.EagleEyeXmp;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using JetBrains.Annotations;

    internal class EagleEyeMetadataWriter : IEagleEyeMetadataWriter
    {
        private const string Prefix = "-xmp-CoenmEagleEye:EagleEye";
        private readonly IExifToolWriter exiftool;

        public EagleEyeMetadataWriter([NotNull] IExifToolWriter exiftool)
        {
            Guard.Argument(exiftool, nameof(exiftool)).NotNull();
            this.exiftool = exiftool;
        }

        public async Task WriteAsync(string filename, EagleEyeMetadata metadata, CancellationToken ct = default)
        {
            Guard.Argument(filename, nameof(filename)).NotNull().NotEmpty();
            Guard.Argument(metadata, nameof(metadata)).NotNull();

            ct.ThrowIfCancellationRequested();

            var args = new List<string>
                {
                    $"{Prefix}Version=1",
                    $"{Prefix}Id={ConvertBytes(metadata.Id.ToByteArray())}",
                    $"{Prefix}Timestamp={metadata.Timestamp:yyyy:MM:dd HH:mm:sszzz}",
                };

            if (metadata.FileHash == null)
                args.Add($"{Prefix}FileHash=");
            else
                args.Add($"{Prefix}FileHash=" + ConvertBytes(metadata.FileHash));

            foreach (var bytes in metadata.RawImageHash ?? Enumerable.Empty<byte[]>())
            {
                var z85Bytes = ConvertBytes(bytes);
                args.Add($"{Prefix}RawImageHash-=" + z85Bytes);
                args.Add($"{Prefix}RawImageHash+=" + z85Bytes);
            }

            try
            {
                await exiftool.WriteAsync(filename, args, ct).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                // todo coenm
                Console.WriteLine(filename + "  " + e.Message);
            }
        }

        private static string ConvertBytes(byte[] bytes)
        {
            return Z85Extended.Encode(bytes);
        }
    }
}
