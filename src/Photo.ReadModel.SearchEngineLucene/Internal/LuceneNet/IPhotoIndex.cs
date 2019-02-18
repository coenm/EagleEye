namespace EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.LuceneNet
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EagleEye.Photo.ReadModel.SearchEngineLucene.Internal.Model;
    using JetBrains.Annotations;
    using Lucene.Net.Search;

    internal interface IPhotoIndex
    {
        Task<bool> ReIndexMediaFileAsync([NotNull] Photo data);

        int Count([CanBeNull] Query query = null, [CanBeNull] Filter filter = null);

        PhotoSearchResult Search(Guid guid);

        List<PhotoSearchResult> Search(string queryString, out int totalHits);

        List<PhotoSearchResult> Search([NotNull] Query query, [CanBeNull] Filter filter, out int totalHits);
    }
}
