// namespace EagleEye.PhotoInformationProvider.MetadataExtractor
// {
//     using System.Collections.Generic;
//     using System.Linq;
//     using System.Threading.Tasks;
//
//     using EagleEye.Core.Data;
//     using EagleEye.Core.Interfaces.PhotoInformationProviders;
//
//     using Helpers.Guards;
//     using JetBrains.Annotations;
//
//     public class MetadataExtractorPersonProvider : IPhotoPersonProvider
//     {
//         public string Name => nameof(MetadataExtractorPersonProvider);
//
//         public uint Priority { get; } = 10;
//
//         public bool CanProvideInformation(string filename) => !string.IsNullOrWhiteSpace(filename);
//
//         public async Task<List<string>> ProvideAsync(string filename)
//         {
//             var directories = ImageMetadataReader.ReadMetadata(filename);
//             if (directories == null)
//                 return new List<string>(0);
//
//             var directory = directories.OfType<XmpDirectory>().FirstOrDefault();
//             if (directory == null)
//                 return new List<string>(0);
//
//             var des = new XmpDescriptor(directory);
//
//
//
//
//
//             var result = await picasaService.GetDataAsync(filename).ConfigureAwait(false);
//
//             if (result == null)
//                 return new List<string>();
//
//             return result.Persons.ToList();
//         }
//
//         public async Task ProvideAsync(string filename, MediaObject media)
//         {
//             var result = await ProvideAsync(filename).ConfigureAwait(false);
//
//             foreach (var person in result)
//                 media.AddPerson(person);
//         }
//     }
// }
