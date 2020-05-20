namespace EagleEye.FileImporter.Similarity
{
    public readonly struct FileProcessingProgress
    {
        public FileProcessingProgress(string filename, int step, int totalSteps, string message, ProgressState state)
        {
            Filename = filename;
            Step = step;
            TotalSteps = totalSteps;
            Message = message;
            State = state;
        }

        public string Filename { get; }

        public int Step { get; }

        public int TotalSteps { get; }

        public string Message { get; }

        public ProgressState State { get; }
    }
}
