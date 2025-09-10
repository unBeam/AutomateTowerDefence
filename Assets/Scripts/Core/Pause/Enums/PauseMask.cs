using System;

[Flags]
public enum PauseMask : uint
{
    None     = 0,
    Gameplay = 1 << 0,
    Meta     = 1 << 1,
    UI       = 1 << 2,
    All      = 0xFFFFFFFF
}