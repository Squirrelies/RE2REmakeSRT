using System;

namespace RE2REmakeSRT
{
    [Flags]
    public enum ProgramFlags : byte
    {
        None = 0x00,
        SkipChecksumCheck = 0x01,
        Debug = SkipChecksumCheck
    }
}
