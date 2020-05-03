namespace RainOfStages.Plugin
{
    public delegate void MethodCall();
    public delegate void Hook(MethodCall orig);

    public delegate void MethodCall<T>(T self);
    public delegate void Hook<T>(MethodCall<T> orig, T self);

    public delegate void MethodCall<T, T1>(T self, T1 arg0);
    public delegate void Hook<T, T1>(MethodCall<T, T1> orig, T self, T1 arg0);

    public delegate void MethodCall<T, T1, T2>(T self, T1 arg0, T2 arg1);
    public delegate void Hook<T, T1, T2>(MethodCall<T, T1, T2> orig, T self, T1 arg0, T2 arg1);

    public delegate void MethodCall<T, T1, T2, T3>(T self, T1 arg0, T2 arg1, T3 arg2);
    public delegate void Hook<T, T1, T2, T3>(MethodCall<T, T1, T2, T3> orig, T self, T1 arg0, T2 arg1, T3 arg2);

    public delegate void MethodCall<T, T1, T2, T3, T4>(T self, T1 arg0, T2 arg1, T3 arg2, T4 arg3);
    public delegate void Hook<T, T1, T2, T3, T4>(MethodCall<T, T1, T2, T3, T4> orig, T self, T1 arg0, T2 arg1, T3 arg2, T4 arg3);

}
