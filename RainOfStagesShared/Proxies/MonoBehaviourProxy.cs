using UnityEngine;
using System.Linq;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Reflection;
using FieldMap = System.Collections.Generic.Dictionary<string, System.Reflection.FieldInfo>;

namespace RainOfStages.Proxy
{
    using FieldLookup = Dictionary<Type, (FieldMap proxy, FieldMap real)>;

    public abstract class MonoBehaviourProxy<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static FieldLookup ReflectionCache = new FieldLookup();

        private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;

        public bool destroyProxy;

        protected T component;

        public virtual void Awake()
        {
            if (NetworkServer.active)
            {
                component = this.gameObject.AddComponent<T>();

                if (!ReflectionCache.ContainsKey(typeof(T)))
                {
                    var proxyfields = GetType().GetFields(bindingFlags);
                    var realFields = typeof(T).GetFields(bindingFlags);

                    var proxyDictionary = proxyfields.ToDictionary(k => k.Name);
                    var realDictionary = realFields.ToDictionary(k => k.Name);

                    ReflectionCache[typeof(T)] = (proxyDictionary, realDictionary);
                }

                var (proxy, real) = ReflectionCache[typeof(T)];
                foreach (var name in proxy.Keys)
                {
                    if (!real.ContainsKey(name)) continue;

                    var value = proxy[name].GetValue(this);
                    real[name].SetValue(component, Convert.ChangeType(value, real[name].FieldType));
                }

                if (destroyProxy)
                    Destroy(this);
            }
        }
    }
}