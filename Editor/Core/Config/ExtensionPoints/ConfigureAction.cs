using System;

namespace ThunderKit.Core.Config
{
    [Serializable]
    public abstract class ConfigureAction : ImportExtension<ConfigureAction>
    {
        /// <summary>
        /// Executed after the last files and data have been imported into the project, but before 
        /// </summary>
        public abstract void Execute();
    }
}