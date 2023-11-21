using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DHTMfs.Models
{
    [Table("Files")]
    public class File
    {
        [Key]
        public string FileHash { get; set; }
        public string OriginalName { get; set; }
        public string Extension { get; set; }
        public int Size { get; set; }
        public string Date { get; set; }
    }
}
