using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DHTMfs.Models
{
    [Table("Nodes")]
    public class Node
    {
        [Key]
        [Required]
        public string NodeHash { get; set; }
        [Required]
        public string Host { get; set; }
        [Required]
        public int Port { get; set; }
        [Required]
        public DateTime LastCheck { get; set; }
        [Required]
        public bool IsOnline { get; set; }
        [Required]
        public bool IsLocal { get; set; }
        public DateTime? LastNodeSync { get; set; }
        public DateTime? LastFileSync { get; set; }
    }
}
