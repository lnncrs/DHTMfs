using System.Collections.Concurrent;

namespace DHTMfs.Models
{
    public class HNode
    {
        public HNode()
        {
            Nodes = new ConcurrentDictionary<string, Node>();
        }

        public ConcurrentDictionary<string, Node> Nodes { get; set; }
    }
}
