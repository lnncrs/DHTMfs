using System.ComponentModel.DataAnnotations;

namespace DHTMfs.Models
{
    public class Node
    {
        [Key]
        public string NodeHash { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public DateTime LastCheck { get; set; }
        public bool IsOnline { get; set; }
        public bool IsLocal { get; set; }
    }
}
