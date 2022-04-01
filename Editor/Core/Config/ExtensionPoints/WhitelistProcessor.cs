using System;
using System.Collections.Generic;

namespace ThunderKit.Core.Config
{
    [Serializable]
    public abstract class WhitelistProcessor : ImportExtension<WhitelistProcessor>
    {
        /// <summary>
        /// Modify or provide assembly whitelist of filenames
        /// The blacklist is overriden by the whitelist
        /// </summary>
        /// <param name="whitelist">enumeration of current whitelist before processing</param>
        /// <returns>true if assembly should be whitelisted, otherwise false</returns>
        public abstract IEnumerable<string> Process(IEnumerable<string> whitelist);
    }
}