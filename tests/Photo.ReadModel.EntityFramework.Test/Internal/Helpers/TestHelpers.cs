namespace Photo.ReadModel.EntityFramework.Test.Internal.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using EagleEye.Photo.ReadModel.EntityFramework.Internal.EntityFramework.Models;

    internal static class TestHelpers
    {
        public static Photo CreatePhoto(
            Guid id,
            int version,
            string filename,
            byte[] fileSha,
            DateTimeOffset eventTimestamp,
            string[] tags,
            string[] people)
        {
            return new Photo
            {
                Id = id,
                Version = version,
                Filename = filename,
                FileSha256 = fileSha,
                EventTimestamp = eventTimestamp,
                Tags = CreateTags(tags),
                People = CreatePeoples(people),
                FileMimeType = "image/jpeg",
            };
        }

        public static List<Tag> CreateTags(params string[] tags)
        {
            return tags?.Select(x => new Tag { Value = x }).ToList();
        }

        public static List<Person> CreatePeoples(params string[] people)
        {
            return people?.Select(x => new Person { Value = x }).ToList();
        }
    }
}
