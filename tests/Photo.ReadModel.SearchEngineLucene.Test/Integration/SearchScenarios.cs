namespace Photo.ReadModel.SearchEngineLucene.Test.Integration
{
    using Xunit;

    internal class SearchScenarios : TheoryData<string, ModuleTestFixture.PhotoPersonItem[]>
    {
        public SearchScenarios()
        {
            var bobNotCalvin = new[] { ModuleTestFixture.Photo2, ModuleTestFixture.Photo3, };
            Add("person:Bob -person:ca*", bobNotCalvin);
            Add("person:Bob -person:calvin", bobNotCalvin);
            Add("person:B?b -person:calvin", bobNotCalvin);
            Add("person:b*b -person:calvin", bobNotCalvin);
            Add("person:bOb AND -person:c*", bobNotCalvin);

            var onlyBobJackson = new[] { ModuleTestFixture.Photo2, };
            Add("person:\"Bob Jackson\" ", onlyBobJackson);
            Add("person:\"Bob       Jackson\" ", onlyBobJackson);
            Add("+person:Bob +person:Jackson ", onlyBobJackson);

            // x = "\"b\\\\c.jpg\"";
            // string query = "filename:" + x;
            // todo more tests for filename search.

            // Photo1: "a/b/x/c.jpg",
            // Photo2: "a/b/dexter.jpg",
            // Photo3: "a/bobby.jpg",
            // Photo4: "b/x/red bulls/TeamPhoto.jpg",
            Add("filename:x", new[] { ModuleTestFixture.Photo1, ModuleTestFixture.Photo4, });

            // filename with b (not expected to find Photo3 (although it contains a 'b'))
            Add("filename:b", new[] { ModuleTestFixture.Photo1, ModuleTestFixture.Photo2, ModuleTestFixture.Photo4, });

            // Add("filename:\"a b\" ", new[] { ModuleTestFixture.Photo1, ModuleTestFixture.Photo2, });
            Add("filename:\"b/x/red bulls/TeamPhoto.jpg\" ", new[] { ModuleTestFixture.Photo4, });
        }
    }
}
