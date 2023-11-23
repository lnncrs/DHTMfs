using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DHTMfs.Models
{
    [Table("Files")]
    public class File
    {
        [Key]
        [Required]
        public string FileHash { get; set; }
        [Required]
        public string OriginalName { get; set; }
        [Required]
        public string Extension { get; set; }
        [Required]
        public long Size { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string NodeHash { get; set; }
    }
}
