using System;

namespace RE2REmakeSRT
{
    [Flags]
    public enum ProgramFlags : byte
    {
        None = 0,
        Debug = 1,
        //SkipChecksumCheck = 2,
        NoTitleBar = 4,
        AlwaysOnTop = 8,
        Transparent = 16,
        NoInventory = 32,
        DirectXOverlay = 64,
    }
}
