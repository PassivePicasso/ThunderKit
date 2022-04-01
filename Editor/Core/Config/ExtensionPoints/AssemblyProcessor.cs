using System;

namespace ThunderKit.Core.Config
{
    /// <summary>
    /// Redirect where assemblies source path
    /// </summary>
    [Serializable]
    public abstract class AssemblyProcessor : ImportExtension<AssemblyProcessor>
    {
        /// <summary>
        /// Redirect where an assembly should be imported from
        /// </summary>
        /// <param name="path">Path to assembly currently being imported</param>
        /// <returns>new path that replaces the path passed as parameter</returns>
        public virtual string Process(string path) => path;
    }
}