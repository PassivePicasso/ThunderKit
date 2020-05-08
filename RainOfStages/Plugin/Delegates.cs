using System;

namespace RainOfStages.Plugin
{
    public delegate void Hook(Action orig);
    public delegate void Hook<T>(Action<T> orig, T self);
    public delegate void Hook<T, T1>(Action<T, T1> orig, T self, T1 arg0);
    public delegate void Hook<T, T1, T2>(Action<T, T1, T2> orig, T self, T1 arg0, T2 arg1);
    public delegate void Hook<T, T1, T2, T3>(Action<T, T1, T2, T3> orig, T self, T1 arg0, T2 arg1, T3 arg2);
    public delegate void Hook<T, T1, T2, T3, T4>(Action<T, T1, T2, T3, T4> orig, T self, T1 arg0, T2 arg1, T3 arg2, T4 arg3);
    public delegate void Hook<T, T1, T2, T3, T4, T5>(Action<T, T1, T2, T3, T4, T5> orig, T self, T1 arg0, T2 arg1, T3 arg2, T4 arg3);
    public delegate void Hook<T, T1, T2, T3, T4, T5, T6>(Action<T, T1, T2, T3, T4, T5, T6> orig, T self, T1 arg0, T2 arg1, T3 arg2, T4 arg3);
    public delegate void Hook<T, T1, T2, T3, T4, T5, T6, T7>(Action<T, T1, T2, T3, T4, T5, T6, T7> orig, T self, T1 arg0, T2 arg1, T3 arg2, T4 arg3);
    public delegate void Hook<T, T1, T2, T3, T4, T5, T6, T7, T9>(Action<T, T1, T2, T3, T4, T5, T6, T7, T9> orig, T self, T1 arg0, T2 arg1, T3 arg2, T4 arg3);
    public delegate void Hook<T, T1, T2, T3, T4, T5, T6, T7, T9, T10>(Action<T, T1, T2, T3, T4, T5, T6, T7, T9, T10> orig, T self, T1 arg0, T2 arg1, T3 arg2, T4 arg3);

}
