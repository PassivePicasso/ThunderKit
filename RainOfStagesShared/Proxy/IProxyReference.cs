namespace RainOfStages.Proxy
{
    internal interface IProxyReference<T> where T : UnityEngine.Object
    {
        T ResolveProxy();
    }
}