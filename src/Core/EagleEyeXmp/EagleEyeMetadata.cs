namespace EagleEye.Core.EagleEyeXmp
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Version 1 of metadata.
    /// </summary>
    public class EagleEyeMetadata
    {
        public EagleEyeMetadata()
        {
            RawImageHash = new List<byte[]>();
        }

        /// <summary>
        /// Generated Id. Should never change. It is possible to have multiple files with the same id due to copying.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The sha256 hash of the original image file (including all original metadata).
        /// </summary>
        public byte[] FileHash { get; set; }

        /// <summary>
        /// Initial timestamp of adding this metadata.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Sha256 hash of the raw image (ie, the bytes containing the visual image). Can add multiple hashes as an image can change due to cropping, gray scaling etc. etc.
        /// </summary>
        public List<byte[]> RawImageHash { get; set; }
    }
}
