namespace EagleEye.Photo.Domain.Services
{
    using System.Collections.Generic;

    using Helpers.Guards; using Dawn;
    using JetBrains.Annotations;

    internal class UniqueFilenameService : IUniqueFilenameService
    {
        [NotNull] private readonly object syncLock = new object();
        [NotNull] private readonly IFilenameRepository repository;
        [NotNull] private readonly List<string> claimedFileNames;

        public UniqueFilenameService([NotNull] IFilenameRepository repository)
        {
            Helpers.Guards.Guard.NotNull(repository, nameof(repository));
            this.repository = repository;
            claimedFileNames = new List<string>();
        }

        [CanBeNull]
        public IClaimFilenameToken Claim([NotNull] string filename)
        {
            Helpers.Guards.Guard.NotNullOrWhiteSpace(filename, nameof(filename));

            lock (syncLock)
            {
                if (repository.Contains(filename))
                    return null;

                if (claimedFileNames.Contains(filename))
                    return null;

                claimedFileNames.Add(filename);

                return new ClaimFilenameToken(this, filename);
            }
        }

        private void RemoveClaim([NotNull] string filename)
        {
            DebugHelpers.Guards.Guard.NotNull(filename, nameof(filename));

            lock (syncLock)
            {
                claimedFileNames.Remove(filename);
            }
        }

        private void CommitClaim([NotNull] string filename)
        {
            DebugHelpers.Guards.Guard.NotNull(filename, nameof(filename));

            lock (syncLock)
            {
                repository.Add(filename);
            }
        }

        private class ClaimFilenameToken : IClaimFilenameToken
        {
            [NotNull] private readonly UniqueFilenameService parent;
            [NotNull] private readonly string filename;

            public ClaimFilenameToken([NotNull] UniqueFilenameService parent, [NotNull] string filename)
            {
                DebugHelpers.Guards.Guard.NotNull(parent, nameof(parent));
                DebugHelpers.Guards.Guard.NotNull(filename, nameof(filename));
                this.parent = parent;
                this.filename = filename;
            }

            public void Commit() => parent.CommitClaim(filename);

            public void Dispose() => parent.RemoveClaim(filename);
        }
    }
}
