namespace DHTMfs.Models
{
    public class DHTFilePart
    {
        public Dictionary<string,string> FileParts { get; set; }
    }

    public class DHTFile
    {
        public Dictionary<string,string> Files { get; set; }
    }

    public class DHTNode
    {
        public Dictionary<string,string> Nodes { get; set; }
    }
}
