namespace EagleEye.FileImporter
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Dawn;
    using EagleEye.Core.Interfaces.Module;
    using JetBrains.Annotations;
    using SimpleInjector;

    public static class Startup
    {
        /// <summary>
        /// Bootstrap the application by setting up the Dependency Container.
        /// </summary>
        /// <param name="connectionStrings">set of connection strings.</param>
        public static Container ConfigureContainer([NotNull] ConnectionStrings connectionStrings)
        {
            Guard.Argument(connectionStrings, nameof(connectionStrings)).NotNull();

            string connectionStringHangFire = connectionStrings.HangFire;
            var userDir = GetUserDirectory();

            var connectionStringSimilarity = CreateSqlLiteFileConnectionString(CreateFullFilename("Similarity.db"));

            var plugins = EagleEye.Bootstrap.Bootstrapper.FindAvailablePlugins();
            var bootstrapper = EagleEye.Bootstrap.Bootstrapper.Initialize(userDir, plugins, connectionStrings.FilenameEventStore);
            bootstrapper.RegisterPhotoDatabaseReadModel("InMemory a");
            bootstrapper.RegisterSearchEngineReadModel("InMemory a");
            bootstrapper.RegisterSimilarityReadModel(connectionStringSimilarity, "InMemory a");
            return bootstrapper.Finalize();

            // var similarityFilename = indexFilename + ".similarity.json";

            // container.RegisterSingleton<IImageDataRepository>(() => new SingleImageDataRepository(new JsonToFileSerializer<List<ImageData>>(indexFilename)));
            // container.RegisterSingleton<ISimilarityRepository>(() => new SingleFileSimilarityRepository(new JsonToFileSerializer<List<SimilarityResultStorage>>(similarityFilename)));
            //
            // // add scoped?!
            // container.Register<IRepository>(() => new Repository(container.GetInstance<IEventStore>()), Lifestyle.Singleton); // Repository has two public constructors (why??)
            // container.RegisterDecorator<IRepository, CacheRepository>(Lifestyle.Singleton);
            // container.Register<ISession, Session>(Lifestyle.Singleton); // check.
        }

        public static string CreateFullFilename([NotNull] string filename)
        {
            Guard.Argument(filename, nameof(filename)).NotNull().NotWhiteSpace();
            return Path.Combine(GetUserDirectory(), filename);
        }

        public static string CreateSqlLiteFileConnectionString([NotNull] string fullFilename)
        {
            Guard.Argument(fullFilename, nameof(fullFilename)).NotNull().NotWhiteSpace();
            return $"Filename={fullFilename};";
        }

        public static void VerifyContainer([NotNull] Container container)
        {
            Guard.Argument(container, nameof(container)).NotNull();
            container.Verify(VerificationOption.VerifyAndDiagnose);
        }

        public static async Task InitializeAllServices([NotNull] Container container)
        {
            Guard.Argument(container, nameof(container)).NotNull();
            var instancesToInitialize = container.GetAllInstances<IEagleEyeInitialize>().ToArray();
            await Task.WhenAll(instancesToInitialize.Select(instance => instance.InitializeAsync())).ConfigureAwait(false);
        }

        public static void StartServices([NotNull] Container container)
        {
            var allInstances = container.GetAllInstances<IEagleEyeProcess>();
            foreach (var eagleEyeProcess in allInstances)
            {
                eagleEyeProcess.Start();
            }
        }

        public static void StopServices([NotNull] Container container)
        {
            var allEagleEyeProcesses = container.GetAllInstances<IEagleEyeProcess>();
            foreach (var eagleEyeProcess in allEagleEyeProcesses)
            {
                eagleEyeProcess.Stop();
            }
        }

        private static string GetUserDirectory()
        {
            var userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(userDir, "EagleEye");
        }
    }
}
