namespace EagleEye.Core.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EagleEye.Core.ReadModel.EntityFramework;
    using EagleEye.Core.ReadModel.EntityFramework.Models;
    using JetBrains.Annotations;

    using Newtonsoft.Json;

    public interface IReadModelFacade
    {
        Task<IEnumerable<Photo>> GetMediaItems();

        Task<Photo> GetMediaItem(Guid id);
    }

    public class ReadModel : IReadModelFacade
    {
        private readonly IEagleEyeRepository repository;

        public ReadModel([NotNull] IEagleEyeRepository repository)
        {
            this.repository = repository;
        }

        public async Task<IEnumerable<Photo>> GetMediaItems()
        {
            var result = await repository.GetAllAsync().ConfigureAwait(false);
            return result;
        }

        public async Task<Photo> GetMediaItem(Guid id)
        {
            var result = await repository.GetByIdAsync(id).ConfigureAwait(false);
            return result;
        }
    }
}
