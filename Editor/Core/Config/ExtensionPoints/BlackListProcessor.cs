using System;
using System.Collections.Generic;

namespace ThunderKit.Core.Config
{
    /// <summary>
    /// Modify or provide assembly blacklist of filenames
    /// The blacklist is overriden by the whitelist
    /// </summary>
    [Serializable]
    public abstract class BlacklistProcessor : ImportExtension
    {
        /// <summary>
        /// Modify or provide assembly blacklist of filenames
        /// The blacklist is overriden by the whitelist
        /// </summary>
        /// <param name="blacklist">enumeration of current blacklist before processing</param>
        /// <returns>true if assembly should be blacklisted, otherwise false</returns>
        public abstract IEnumerable<string> Process(IEnumerable<string> blacklist);
    }
}