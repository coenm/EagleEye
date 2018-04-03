namespace SearchEngine.Lucene.Bootstrap
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
    public static class SearchEngineLuceneBootstrapper
    {
        // for now, we force a memory index
        private const bool USE_MEMORY_INDEX = true;

        private static readonly Assembly[] _contractAssemblies = { typeof(ISearchEngineQuery<>).Assembly };
        private static readonly Assembly[] _businessLayerAssemblies = { typeof(MediaIndex).Assembly }; // Assembly.GetExecutingAssembly()

        public static void Bootstrap(Container container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            container.RegisterSingleton<MediaIndex>();
            RegisterLuceneDirectoryFactory(container, USE_MEMORY_INDEX);

            // container.RegisterSingleton<IValidator>(new DataAnnotationsValidator(container));

            container.Register(typeof(ICommandHandler<>), _businessLayerAssemblies);
            // container.RegisterDecorator(typeof(ICommandHandler<>), typeof(ValidationCommandHandlerDecorator<>));
            // container.RegisterDecorator(typeof(ICommandHandler<>), typeof(AuthorizationCommandHandlerDecorator<>));

            container.Register(typeof(IQueryHandler<,>), _businessLayerAssemblies);
            // container.RegisterDecorator(typeof(IQueryHandler<,>), typeof(AuthorizationQueryHandlerDecorator<,>));
        }

        private static void RegisterLuceneDirectoryFactory(Container container, bool usInMemoryIndex)
        {
            if (usInMemoryIndex)
            {
                container.RegisterSingleton<ILuceneDirectoryFactory, RamLuceneDirectoryFactory>();
            }
            else
            {
                container.RegisterInstance(new FileSystemLuceneDirectorySettings
                                               {
                                                   Directory = "a/b/c"
                                               });

                container.RegisterSingleton<ILuceneDirectoryFactory, FileSystemLuceneDirectoryFactory>();
            }
        }

        public static IEnumerable<Type> GetCommandTypes() =>
            from assembly in _contractAssemblies
            from type in assembly.GetExportedTypes()
            where type.Name.EndsWith("Command")
            select type;

        public static IEnumerable<QueryInfo> GetQueryTypes() =>
            from assembly in _contractAssemblies
            from type in assembly.GetExportedTypes()
            where QueryInfo.IsQuery(type)
            select new QueryInfo(type);
    }

    [DebuggerDisplay("{QueryType.Name,nq}")]
    public sealed class QueryInfo
    {
        public readonly Type QueryType;
        public readonly Type ResultType;

        public QueryInfo(Type queryType)
        {
            QueryType = queryType;
            ResultType = DetermineResultTypes(queryType).Single();
        }

        public static bool IsQuery(Type type) => DetermineResultTypes(type).Any();

        private static IEnumerable<Type> DetermineResultTypes(Type type) =>
            from interfaceType in type.GetInterfaces()
            where interfaceType.IsGenericType
            where interfaceType.GetGenericTypeDefinition() == typeof(ISearchEngineQuery<>)
            select interfaceType.GetGenericArguments()[0];
    }
}