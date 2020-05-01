namespace EagleEye.ExifTool.ExifTool
{
    public static class ExifToolArguments
    {
        public const string Version = "-ver";
        public const string IgnoreMinorErrorsAndWarnings = "-m";
        public const string JsonOutput = "-j";
        public const string Quiet = "-q";
        public const string Fast = "-fast";
        public const string Superfast = "-fast2";
        public const string StayOpen = "-stay_open";
        public const string BoolTrue = "True";
        public const string BoolFalse = "False";
        public const string CommonArgs = "-common_args";

        /// <summary>
        /// Output structured XMP information instead of flattening to individual tags. This option works well when combined with the XML (-X) and JSON (-j) output formats. For other output formats, XMP structures and lists are serialized into the same format as when writing structured information (see https://exiftool.org/struct.html for details). When copying, structured tags are copied by default unless --struct is used to disable this feature (although flattened tags may still be copied by specifying them individually unless -struct is used). These options have no effect when assigning new values since both flattened and structured tags may always be used when writing.
        /// </summary>
        public const string Struct = "-struct";

        /// <summary>
        /// Organize output by tag group. Family numbers may be added wherever -g is mentioned in the documentation. Multiple families may be specified by separating them with colons. By default the resulting group name is simplified by removing any leading Main: and collapsing adjacent identical group names, but this can be avoided by placing a colon before the first family number (eg. -g:3:1). Use the -listg option to list group names for a specified family. The SavePath and SaveFormat API options are automatically enabled if the respective family 5 or 6 group names are requested. See the API GetGroup documentation for more information.
        /// </summary>
        /// <param name="num">Specifies a group family number, and may be 0 (general location), 1 (specific location), 2 (category), 3 (document number), 4 (instance number), 5 (metadata path) or 6 (EXIF/TIFF format). Defaults to 0.</param>
        /// <returns>Exiftool argument.</returns>
        public static string OutputGroupHeadings(int num)
        {
            if (num < 0)
                num = 0;
            if (num > 6)
                num = 0;

            return $"-g{num}";
        }
    }
}
