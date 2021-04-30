namespace EagleEye.FileStamper.Console
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using ShellProgressBar;

    public class MyProgressBar : IDisposable
    {
        private readonly Dictionary<string, ChildProgressBar> _spawnedFiles = new Dictionary<string, ChildProgressBar>();
        private readonly object _spawnLock = new object();
        private readonly ProgressBar _inner;
        private ConcurrentDictionary<string, ChildProgressBar> progressBars = new ConcurrentDictionary<string, ChildProgressBar>();

        public static readonly ProgressBarOptions ProgressOptions = new ProgressBarOptions
        {
            ProgressCharacter = '─',
            ForegroundColor = ConsoleColor.Yellow,
            BackgroundColor = ConsoleColor.DarkYellow,
            EnableTaskBarProgress = true,
        };

        public static readonly ProgressBarOptions ChildOptions = new ProgressBarOptions
        {
            ForegroundColor = ConsoleColor.Green,
            BackgroundColor = ConsoleColor.DarkGreen,
            ProgressCharacter = '─',
            CollapseWhenFinished = true,
        };

        public MyProgressBar(int maxTicks, string message)
        {
            _inner = new ProgressBar(maxTicks, message, ProgressOptions);
        }

        public void Update(FileProcessingProgress data)
        {
            progressBars.AddOrUpdate(
                data.Filename,
                filename => SpawnChildProgressBar(_inner, data, filename),
                (_, childProgressBar) => UpdateProgress(_inner, data, childProgressBar));
        }

        public void Dispose()
        {
            foreach (var item in progressBars)
                item.Value?.Dispose();

            _inner?.Dispose();
        }

        private static ChildProgressBar UpdateProgress(ProgressBar parentProgressBar, FileProcessingProgress fileProcessingProgress, ChildProgressBar childProgressBar)
        {
            if (childProgressBar == null)
                return null;

            if (fileProcessingProgress.Step == fileProcessingProgress.TotalSteps)
            {
                childProgressBar.Tick(int.MaxValue);
                /*childProgressBar.Dispose();*/
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
                decimal decimalStep = Math.Floor(totalSteps * int.MaxValue);
                var step = (int)decimalStep;
                childProgressBar.Tick(step);
                return childProgressBar;
            }
        }

        private ChildProgressBar SpawnChildProgressBar(ProgressBar parent, FileProcessingProgress fileProcessingProgress, string message)
        {
            ChildProgressBar bar;

            lock (_spawnLock)
            {
                if (!_spawnedFiles.ContainsKey(fileProcessingProgress.Filename))
                {
                    bar = parent.Spawn(int.MaxValue, message, ChildOptions);
                    _spawnedFiles.Add(fileProcessingProgress.Filename, bar);
                }
                else
                {
                    bar = _spawnedFiles[fileProcessingProgress.Filename];
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
            var x = Math.Floor(totalSteps * int.MaxValue);
            var step = (int)x;
            bar.Tick(step);
            return bar;
        }
    }
}
