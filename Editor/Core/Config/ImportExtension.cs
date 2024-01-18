using System;
using UnityEditor;
using UnityEngine;

namespace ThunderKit.Core.Config
{
    /// <summary>
    /// Base ImportExtension for building GameImport extension points
    /// </summary>
    [Serializable]
    public abstract class ImportExtension : ScriptableObject
    {
        private string extensionName;

        /// <summary>
        /// Name of ImportExtension, Displayed on the Import Configuration page
        /// </summary>
        public virtual string Name => string.IsNullOrEmpty(extensionName) ? extensionName = ObjectNames.NicifyVariableName(GetType().Name) : extensionName;

        /// <summary>
        /// Integer which indicates the priority at which this extension will 
        /// run. Import Extensions are ordered by their priority in descending 
        /// order.
        /// </summary>
        public abstract int Priority { get; }

    }
}