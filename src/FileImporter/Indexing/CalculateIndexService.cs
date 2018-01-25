﻿using System;
using System.Collections.Generic;
using FileImporter.Imaging;

namespace FileImporter.Indexing
{
    public class CalculateIndexService
    {
        private readonly IContentResolver _contentResolver;

        public CalculateIndexService(IContentResolver contentResolver)
        {
            _contentResolver = contentResolver ?? throw new ArgumentNullException(nameof(contentResolver));
        }
        
        public IEnumerable<ImageData> CalculateIndex(IReadOnlyList<string> fileIdentifiers)
        {
            if (fileIdentifiers == null)
                throw new ArgumentNullException(nameof(fileIdentifiers));

            var result = new ImageData[fileIdentifiers.Count];

            for (var index = 0; index < fileIdentifiers.Count; index++)
            {
                using (var stream = _contentResolver.Read(fileIdentifiers[index]))
                {
                    result[index] = new ImageData(fileIdentifiers[index])
                    {
                        Hashes = ImageHashing.Calculate(stream)
                    };
                }
            }

            return result;
        }
    }
}