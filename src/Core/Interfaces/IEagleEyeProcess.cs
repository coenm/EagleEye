namespace EagleEye.Core.Interfaces
{
    using System;

    public interface IEagleEyeProcess : IDisposable
    {
        void Start();

        void Stop();
    }
}
