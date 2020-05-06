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
                    $"{Prefix}Id={ConvertBytes(metadata.Id.ToByteArray())}",
                };

            if (metadata.FileHash == null)
                args.Add($"{Prefix}FileHash=");
            else
                args.Add($"{Prefix}FileHash=" + ConvertBytes(metadata.FileHash));

            var ts = metadata.Timestamp.ToString("yyyy:MM:dd HH:mm:ss");
            args.Add($"{Prefix}Timestamp=\"" + ts + "\"");

            foreach (var bytes in metadata.RawImageHash ?? Enumerable.Empty<byte[]>())
            {
                var z85Bytes = ConvertBytes(bytes);
                args.Add($"{Prefix}Timestamp-=" + z85Bytes);
                args.Add($"{Prefix}Timestamp+=" + z85Bytes);
            }

            try
            {
                await exiftool.WriteAsync(filename, args, ct).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                int x = e.Message.Length;
            }
        }

        private static string ConvertBytes(byte[] bytes)
        {
            return "\"" + Z85Extended.Encode(bytes) + "\"";
        }
    }
}
