namespace EagleEye.Start.Console
{
    using System;
    using System.IO;
    using System.Linq;

    using EagleEye.Core;

    using SimpleInjector;

    public static class Program
    {
        private static Container container;

        public static void Main(string[] args)
        {
            var availablePlugins = EagleEye.Bootstrap.Bootstrapper.FindAvailablePlugins().ToList();

            var bootstrapper = EagleEye.Bootstrap.Bootstrapper.Initialize(string.Empty, availablePlugins);

            var container = bootstrapper.Finalize();

            var processor = container.GetInstance<PhotoProcessor>();
        }

        private static string GetUserDirectory()
        {
            var userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(userDir, "EagleEye");
        }
    }
}
