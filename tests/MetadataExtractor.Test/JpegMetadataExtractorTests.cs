namespace MetadataExtractor.Test
{
    using System.IO;
    using System.Linq;

    using EagleEye.TestHelper;
    using FluentAssertions;
    using MetadataExtractor.Formats.Iptc;
    using Xunit;
    using Xunit.Categories;

    public class JpegMetadataExtractorTests
    {
        private readonly string imageFilename;

        public JpegMetadataExtractorTests()
        {
            imageFilename = System.IO.Directory
                .GetFiles(TestImages.InputImagesDirectoryFullPath, "1.jpg", SearchOption.AllDirectories)
                .SingleOrDefault();

            imageFilename.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Exploratory]
        public void ReadMetadata_OfPreparedJpg_ShouldContainCityMetadata()
        {
            var directories = ImageMetadataReader.ReadMetadata(imageFilename);
            directories.Should().NotBeNullOrEmpty("File should contain multiple groups of tags.");

            var directory = directories.OfType<IptcDirectory>().FirstOrDefault();
            directory.Should().NotBeNull("File should contain Iptc group.");

            var des = new IptcDescriptor(directory);
            var city = des.GetCityDescription();
            city.Should().Be("New York");
        }
    }
}
