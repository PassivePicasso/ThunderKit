using System;

namespace RainOfStages
{
    [Flags]
    public enum HullMask
    {
        None = 0,
        Human = 1,
        Golem = 2,
        BeetleQueen = 4,
    }
}