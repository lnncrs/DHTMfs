using System.ComponentModel.DataAnnotations;

namespace DHTMfs.Models
{
    public class FilePart
    {
        [Key]
        public string FilePartHash { get; set; }
        public string FileHash { get; set; }
        public string NodeHash { get; set; }
        public int PartIndex { get; set; }
    }
}
