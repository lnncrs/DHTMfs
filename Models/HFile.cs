using System.Collections.Concurrent;

namespace DHTMfs.Models
{
    public class HFile
    {
        public HFile()
        {
            Files = new ConcurrentDictionary<string, File>();
        }

        public ConcurrentDictionary<string, File> Files { get; set; }
    }
}
