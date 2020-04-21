using UnityEngine;
using System.Collections;

namespace RainOfStages.Proxy
{
    public interface IProxyObject<T>
    {
        T ResolveProxy();
    }
}