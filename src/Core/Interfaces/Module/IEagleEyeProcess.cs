namespace EagleEye.Core.Interfaces.Module
{
    using System;

    public interface IEagleEyeProcess : IDisposable
    {
        void Start();

        void Stop();
    }
}
