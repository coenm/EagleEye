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
    using NLog;

    internal class EagleEyeMetadataWriter : IEagleEyeMetadataWriter
    {
        private const string Prefix = "-xmp-CoenmEagleEye:EagleEye";
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IExifToolWriter exiftool;

        public EagleEyeMetadataWriter([NotNull] IExifToolWriter exiftool)
        {
            Guard.Argument(exiftool, nameof(exiftool)).NotNull();
            this.exiftool = exiftool;
        }

        public async Task WriteAsync(string filename, EagleEyeMetadata metadata, bool overwriteOriginal, CancellationToken ct = default)
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

            if (overwriteOriginal)
                args.Add("-overwrite_original");

            try
            {
                await exiftool.WriteAsync(filename, args, ct).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.Error($"Error writing metadata to media '{filename}'. {e.Message}");
            }
        }

        private static string ConvertBytes(byte[] bytes)
        {
            return Z85Extended.Encode(bytes);
        }
    }
}
