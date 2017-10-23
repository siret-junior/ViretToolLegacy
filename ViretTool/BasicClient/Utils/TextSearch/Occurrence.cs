using System;

namespace ViretTool.BasicClient.Utils {
    /// <summary>
    /// Represents one result form AhoCorasick search
    /// </summary>
    struct Occurrence {
        /// <summary>
        /// Found word
        /// </summary>
        public string Word { get; set; }
        /// <summary>
        /// Position of the word in a text
        /// </summary>
        public uint StartsAt { get; set; }

        public override string ToString() {
            return string.Format("{0} ({1})", Word, StartsAt);
        }
    }
}
