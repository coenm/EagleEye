namespace SearchEngine.Lucene.Bootstrap
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    using SearchEngine.Interface;
    using SearchEngine.LuceneNet.Core;
    using SearchEngine.LuceneNet.Core.Commands;
    using SearchEngine.LuceneNet.Core.Index;
    using SearchEngine.LuceneNet.Core.Queries;

    using SimpleInjector;

    /// <summary>
    /// This class allows registering all types that are defined in the SearchEngine.Lucene.Core, and are shared across
    /// all applications that use this layer.
    /// Inspired by dotnetjunkie/solidservices
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public static class SearchEngineLuceneBootstrapper
    {
        // for now, we force a memory index
        private const bool UseMemoryIndex = true;

        private static readonly Assembly[] ContractAssemblies = { typeof(ISearchEngineQuery<>).Assembly };
        private static readonly Assembly[] BusinessLayerAssemblies = { typeof(MediaIndex).Assembly }; // Assembly.GetExecutingAssembly()

        public static void Bootstrap(Container container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            container.RegisterSingleton<MediaIndex>();
            RegisterLuceneDirectoryFactory(container, UseMemoryIndex);

            // container.RegisterSingleton<IValidator>(new DataAnnotationsValidator(container));

            container.Register(typeof(ICommandHandler<>), BusinessLayerAssemblies);
            // container.RegisterDecorator(typeof(ICommandHandler<>), typeof(ValidationCommandHandlerDecorator<>));
            // container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AuthorizationCommandHandlerDecorator<>));

            container.Register(typeof(IQueryHandler<,>), BusinessLayerAssemblies);
            // container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(AuthorizationQueryHandlerDecorator<,>));
        }

        public static IEnumerable<Type> GetCommandTypes() =>
            from assembly in ContractAssemblies
            from type in assembly.GetExportedTypes()
            where type.Name.EndsWith("Command")
            select type;

        public static IEnumerable<QueryInfo> GetQueryTypes() =>
            from assembly in ContractAssemblies
            from type in assembly.GetExportedTypes()
            where QueryInfo.IsQuery(type)
            select new QueryInfo(type);

        private static void RegisterLuceneDirectoryFactory(Container container, bool useInMemoryIndex)
        {
            if (useInMemoryIndex)
            {
                container.RegisterSingleton<ILuceneDirectoryFactory, RamLuceneDirectoryFactory>();
            }
            else
            {
                container.RegisterInstance(new FileSystemLuceneDirectorySettings
                                               {
                                                   Directory = "a/b/c",
                                               });

                container.RegisterSingleton<ILuceneDirectoryFactory, FileSystemLuceneDirectoryFactory>();
            }
        }
    }
}
