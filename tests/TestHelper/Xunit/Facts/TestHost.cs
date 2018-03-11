namespace EagleEye.TestHelper.Xunit.Facts
{
    using System;

    [Flags]
    public enum TestHost
    {
        Local = 0x01 << 0,
        AppVeyor = 0x01 << 1,
        Travis = 0x01 << 2,
    }

    public enum TestHostMode
    {
        Allow,
        Skip
    }
}