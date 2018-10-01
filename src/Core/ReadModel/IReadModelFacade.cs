namespace EagleEye.Core.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using EagleEye.Core.ReadModel.EntityFramework;
    using EagleEye.Core.ReadModel.EntityFramework.Dto;

    using JetBrains.Annotations;

    using Newtonsoft.Json;

    public interface IReadModelFacade
    {
        Task<IEnumerable<MediaItemDto>> GetMediaItems();

        Task<MediaItemDto> GetMediaItem(Guid id);
    }

    public class ReadModel : IReadModelFacade
    {
        private readonly IMediaItemRepository _repository;

        public ReadModel([NotNull] IMediaItemRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<MediaItemDto>> GetMediaItems()
        {
            var result = await _repository.GetAllAsync().ConfigureAwait(false);
            return result.Select(item => JsonConvert.DeserializeObject<MediaItemDto>(item.SerializedMediaItemDto));
        }

        public async Task<MediaItemDto> GetMediaItem(Guid id)
        {
            var result = await _repository.GetByIdAsync(id).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<MediaItemDto>(result.SerializedMediaItemDto);
        }
    }
}