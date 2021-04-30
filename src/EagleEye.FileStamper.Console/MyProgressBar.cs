namespace EagleEye.FileStamper.Console
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using ShellProgressBar;

    public class MyProgressBar : IDisposable
    {
        private readonly Dictionary<string, ChildProgressBar> spawnedFiles = new Dictionary<string, ChildProgressBar>();

        private readonly object spawnLock = new object();

        private readonly ProgressBar inner;

        private readonly ConcurrentDictionary<string, ChildProgressBar> progressBars = new ConcurrentDictionary<string, ChildProgressBar>();

        private static readonly ProgressBarOptions ProgressOptions = new ProgressBarOptions
        {
            ProgressCharacter = '─',
            ForegroundColor = ConsoleColor.Yellow,
            BackgroundColor = ConsoleColor.DarkYellow,
            EnableTaskBarProgress = true,
        };

        private static readonly ProgressBarOptions ChildOptions = new ProgressBarOptions
        {
            ForegroundColor = ConsoleColor.Green,
            BackgroundColor = ConsoleColor.DarkGreen,
            ProgressCharacter = '─',
            CollapseWhenFinished = true,
        };

        public MyProgressBar(int maxTicks, string message)
        {
            inner = new ProgressBar(maxTicks, message, ProgressOptions);
        }

        public void Update(FileProcessingProgress data)
        {
            progressBars.AddOrUpdate(
                data.Filename,
                filename => SpawnChildProgressBar(inner, data, filename),
                (_, childProgressBar) => UpdateProgress(inner, data, childProgressBar));
        }

        public void Dispose()
        {
            foreach (var item in progressBars)
                item.Value?.Dispose();

            inner?.Dispose();
        }

        private static ChildProgressBar UpdateProgress(ProgressBar parentProgressBar, FileProcessingProgress fileProcessingProgress, ChildProgressBar childProgressBar)
        {
            if (childProgressBar == null)
                return null;

            if (fileProcessingProgress.Step == fileProcessingProgress.TotalSteps)
            {
                childProgressBar.Tick(int.MaxValue);
                parentProgressBar.Tick();
                return childProgressBar;
            }
            else if (fileProcessingProgress.Step == 0)
            {
                childProgressBar.Tick(0);
                return childProgressBar;
            }
            else
            {
                decimal totalSteps = (decimal)fileProcessingProgress.Step / fileProcessingProgress.TotalSteps;
                var step = (int)Math.Floor(totalSteps * int.MaxValue);
                childProgressBar.Tick(step);
                return childProgressBar;
            }
        }

        private ChildProgressBar SpawnChildProgressBar(ProgressBar parent, FileProcessingProgress fileProcessingProgress, string message)
        {
            ChildProgressBar bar;

            lock (spawnLock)
            {
                if (!spawnedFiles.ContainsKey(fileProcessingProgress.Filename))
                {
                    bar = parent.Spawn(int.MaxValue, message, ChildOptions);
                    spawnedFiles.Add(fileProcessingProgress.Filename, bar);
                }
                else
                {
                    bar = spawnedFiles[fileProcessingProgress.Filename];
                }
            }

            if (fileProcessingProgress.Step == 0)
                return bar;

            if (fileProcessingProgress.Step == fileProcessingProgress.TotalSteps)
            {
                bar.Tick(int.MaxValue);
                return bar;
            }

            decimal totalSteps = (decimal)fileProcessingProgress.Step / fileProcessingProgress.TotalSteps;
            var step = (int)Math.Floor(totalSteps * int.MaxValue);
            bar.Tick(step);
            return bar;
        }
    }
}
