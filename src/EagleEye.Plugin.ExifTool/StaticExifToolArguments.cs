namespace EagleEye.ExifTool
{
    using System;
    using System.Collections.Generic;

    using EagleEye.ExifTool.ExifTool;
    using JetBrains.Annotations;

    internal class StaticExifToolArguments : IExifToolArguments
    {
        private readonly string[] arguments;

        public StaticExifToolArguments([CanBeNull] params string[] args)
        {
            arguments = args ?? Array.Empty<string>();
        }

        public static string[] DefaultArguments
        {
            get
            {
                return new[]
                {
                    ExifToolArguments.CommonArgs,
                    ExifToolArguments.JsonOutput,

                    // format coordinates as signed decimals.
                    "-c",
                    "%+.6f",

                    ExifToolArguments.Struct,
                    ExifToolArguments.OutputGroupHeadings(0),
                };
            }
        }

        public IEnumerable<string> Arguments => arguments;
    }
}
