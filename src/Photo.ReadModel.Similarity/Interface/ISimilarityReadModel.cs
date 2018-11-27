namespace Photo.ReadModel.Similarity.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using Photo.ReadModel.Similarity.Interface.Model;

    [PublicAPI]
    public interface ISimilarityReadModel
    {
        [ItemCanBeNull]
        Task<IEnumerable<Photo>> GetAllPhotosAsync();

        [CanBeNull]
        [ItemCanBeNull]
        Task<Photo> GetPhotoByGuidAsync(Guid id);
    }
}
