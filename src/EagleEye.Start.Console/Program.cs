﻿namespace EagleEye.Start.Console
{
    using SimpleInjector;

    public static class Program
    {
        private static Container container;

        public static void Main(string[] args)
        {
            container = new Container();
        }
    }
}