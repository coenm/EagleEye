namespace EagleEye.FileImporter.Scenarios.UpdateIndex
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CQRSlite.Commands;
    using Dawn;
    using EagleEye.Core.Interfaces.PhotoInformationProviders;
    using EagleEye.Photo.Domain.Commands;
    using EagleEye.Photo.Domain.Commands.Inner;
    using EagleEye.Photo.ReadModel.SearchEngineLucene.Interface;
    using JetBrains.Annotations;
    using NLog;

    [UsedImplicitly]
    public class UpdateIndexExecutor : IUpdateIndexExecutor
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        [NotNull] private readonly ICommandSender dispatcher;
        [NotNull] private readonly IReadModel readModel;
        [NotNull] private readonly IPhotoMimeTypeProvider mimeTypeProvider;
        [NotNull] private readonly IFileSha256HashProvider fileHashProvider;
        [NotNull] private readonly ImmutableArray<IPhotoPersonProvider> personsProviders;
        [NotNull] private readonly ImmutableArray<IPhotoDateTimeTakenProvider> photoTakenProviders;
        [NotNull] private readonly ImmutableArray<IPhotoHashProvider> photoHashProviders;
        [NotNull] private readonly ImmutableArray<IPhotoTagProvider> tagsProviders;

        public UpdateIndexExecutor(
            [NotNull] ICommandSender dispatcher,
            [NotNull] IReadModel readModel,
            [NotNull] IPhotoMimeTypeProvider mimeTypeProvider,
            [NotNull] IFileSha256HashProvider fileHashProvider,
            [NotNull] IEnumerable<IPhotoTagProvider> photoTagProviders,
            [NotNull] IEnumerable<IPhotoPersonProvider> personsProviders,
            [NotNull] IEnumerable<IPhotoDateTimeTakenProvider> photoDateTimeTakenProviders,
            [NotNull] IEnumerable<IPhotoHashProvider> photoHashProviders)
        {
            Guard.Argument(readModel, nameof(readModel)).NotNull();
            Guard.Argument(mimeTypeProvider, nameof(mimeTypeProvider)).NotNull();
            Guard.Argument(fileHashProvider, nameof(fileHashProvider)).NotNull();
            Guard.Argument(dispatcher, nameof(dispatcher)).NotNull();
            Guard.Argument(photoTagProviders, nameof(photoTagProviders)).NotEmpty();
            Guard.Argument(personsProviders, nameof(personsProviders)).NotEmpty();
            Guard.Argument(photoDateTimeTakenProviders, nameof(photoDateTimeTakenProviders)).NotEmpty();
            Guard.Argument(photoHashProviders, nameof(photoHashProviders)).NotEmpty();

            this.dispatcher = dispatcher;
            this.readModel = readModel;
            this.mimeTypeProvider = mimeTypeProvider;
            this.fileHashProvider = fileHashProvider;
            this.tagsProviders = photoTagProviders.ToImmutableArray();
            this.personsProviders = personsProviders.ToImmutableArray();
            this.photoTakenProviders = photoDateTimeTakenProviders.ToImmutableArray();
            this.photoHashProviders = photoHashProviders.ToImmutableArray();
        }

        public async Task HandleAsync([NotNull] string filename, [CanBeNull] IProgress<FileProcessingProgress> progress, CancellationToken ct = default)
        {
            const int stepCount = 11;
            int currentStep = 1;
            progress?.Report(new FileProcessingProgress(filename, currentStep, stepCount, "Starting", ProgressState.Busy));
            currentStep++;

            // check if file exists
            if (!File.Exists(filename))
            {
                progress?.Report(new FileProcessingProgress(filename, stepCount, stepCount, "File not found", ProgressState.Failure));
                return;
            }

            progress?.Report(new FileProcessingProgress(filename, currentStep, stepCount, "Search in index", ProgressState.Busy));
            currentStep++;

            var f = filename.Replace(":\\", " ").Replace("\\", " ").Replace("/", " ");
            var searchFileQuery = "filename:\"" + f + "\"";
            var count = readModel.Count(searchFileQuery);

            Guid guid;

            if (count > 1)
            {
                progress?.Report(new FileProcessingProgress(filename, stepCount, stepCount, "Multiple indexes found", ProgressState.Failure));
                return;
            }

            if (count == 0)
            {
                // add
                progress?.Report(new FileProcessingProgress(filename, currentStep, stepCount, "Get and update mimetype", ProgressState.Busy));
                currentStep++;

                var mimeType = string.Empty;
                if (mimeTypeProvider.CanProvideInformation(filename))
                    mimeType = await mimeTypeProvider.ProvideAsync(filename).ConfigureAwait(false);

                progress?.Report(new FileProcessingProgress(filename, currentStep, stepCount, "Get and update file hash", ProgressState.Busy));
                currentStep++;

                byte[] hash = new byte[32];
                if (fileHashProvider.CanProvideInformation(filename))
                {
                    var fileHashResult = await fileHashProvider.ProvideAsync(filename).ConfigureAwait(false);
                    hash = fileHashResult.ToArray();
                }

                progress?.Report(new FileProcessingProgress(filename, currentStep, stepCount, "Create Photo", ProgressState.Busy));
                currentStep++;

                var createCommand = new CreatePhotoCommand(filename, hash, mimeType);
                guid = createCommand.Id;
                await dispatcher.Send(createCommand, ct).ConfigureAwait(false);
            }
            else
            {
                progress?.Report(new FileProcessingProgress(filename, currentStep, stepCount, "Full search", ProgressState.Busy));
                currentStep++;
                currentStep++;
                currentStep++;

                // get id
                var result = readModel.Search(searchFileQuery);
                if (result.Count != 1)
                    return;
                guid = result.Single().Id;
            }

            progress?.Report(new FileProcessingProgress(filename, currentStep, stepCount, "Update tags", ProgressState.Busy));
            currentStep++;
            await UpdateTags(filename, ct, guid).ConfigureAwait(false);

            progress?.Report(new FileProcessingProgress(filename, currentStep, stepCount, "Update persons", ProgressState.Busy));
            currentStep++;
            await UpdatePersons(filename, ct, guid).ConfigureAwait(false);

            progress?.Report(new FileProcessingProgress(filename, currentStep, stepCount, "Update datetime taken", ProgressState.Busy));
            currentStep++;
            await UpdatePhotoTaken(filename, ct, guid).ConfigureAwait(false);

            progress?.Report(new FileProcessingProgress(filename, currentStep, stepCount, "Update photo hash", ProgressState.Busy));
            currentStep++;
            await UpdatePhotoHash(filename, ct, guid).ConfigureAwait(false);

            // await Task.WhenAll(
            //                    UpdateTags(filename, ct, guid),
            //                    UpdatePersons(filename, ct, guid),
            //                    UpdatePhotoTaken(filename, ct, guid),
            //                    UpdatePhotoHash(filename, ct, guid))
            //           .ConfigureAwait(false);

            progress?.Report(new FileProcessingProgress(filename, stepCount, stepCount, "Create Photo", ProgressState.Success));
        }

        private async Task UpdateTags(string filename, CancellationToken ct, Guid guid)
        {
            foreach (var tagsProvider in tagsProviders.OrderBy(x => x.Priority))
            {
                if (!tagsProvider.CanProvideInformation(filename))
                    continue;

                var tags = await tagsProvider.ProvideAsync(filename).ConfigureAwait(false);
                if (tags == null || tags.Count == 0)
                    continue;
                var updateTagsCommand = new AddTagsToPhotoCommand(guid, null, tags.ToArray());
                await dispatcher.Send(updateTagsCommand, ct).ConfigureAwait(false);
            }
        }

        private async Task UpdatePhotoHash(string filename, CancellationToken ct, Guid guid)
        {
            foreach (var photoHashProvider in photoHashProviders.OrderBy(x => x.Priority))
            {
                if (!photoHashProvider.CanProvideInformation(filename))
                    continue;

                var hashes = await photoHashProvider.ProvideAsync(filename).ConfigureAwait(false);
                if (hashes == null)
                    continue;

                foreach (var hash in hashes)
                {
                    var updatePhotoHashCommand = new UpdatePhotoHashCommand(guid, null, hash.HashName, hash.Hash);
                    await dispatcher.Send(updatePhotoHashCommand, ct).ConfigureAwait(false);
                }

                // var tasks = hashes.Select(hash =>
                //                           {
                //                               var updatePhotoHashCommand = new UpdatePhotoHashCommand(guid, null, hash.HashName, hash.Hash);
                //                               return dispatcher.Send(updatePhotoHashCommand, ct);
                //                           });
                //
                // await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        private async Task UpdatePhotoTaken(string filename, CancellationToken ct, Guid guid)
        {
            foreach (var photoTakenProvider in photoTakenProviders.OrderBy(x => x.Priority))
            {
                if (!photoTakenProvider.CanProvideInformation(filename))
                    continue;

                var timestamp = await photoTakenProvider.ProvideAsync(filename).ConfigureAwait(false);

                if (timestamp == null)
                    continue;

                var setDateTimeTakenCommand = new SetDateTimeTakenCommand(
                                                                          guid,
                                                                          null,
                                                                          new Timestamp
                                                                          {
                                                                              Year = timestamp.Value.Year,
                                                                              Month = timestamp.Value.Month,
                                                                              Day = timestamp.Value.Day,
                                                                              Hour = timestamp.Value.Hour,
                                                                              Minutes = timestamp.Value.Minute,
                                                                              Seconds = timestamp.Value.Second,
                                                                          });
                await dispatcher.Send(setDateTimeTakenCommand, ct).ConfigureAwait(false);
            }
        }

        private async Task UpdatePersons(string filename, CancellationToken ct, Guid guid)
        {
            foreach (var personProvider in personsProviders.OrderBy(x => x.Priority))
            {
                if (!personProvider.CanProvideInformation(filename))
                    continue;

                var persons = await personProvider.ProvideAsync(filename).ConfigureAwait(false);
                if (persons == null || persons.Count == 0)
                    continue;

                var personsCommand = new AddPersonsToPhotoCommand(guid, null, persons.ToArray());
                await dispatcher.Send(personsCommand, ct).ConfigureAwait(false);
            }
        }
    }
}
