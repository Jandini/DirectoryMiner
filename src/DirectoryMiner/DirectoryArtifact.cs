using System.Text.Json.Serialization;
using TreeMiner;

namespace DirectoryMiner
{
    internal class DirectoryArtifact : ITreeArtifact
    {
        public Guid Id { get; set; }
        public Guid ParentId { get; set; }
        public int Level { get; set; }
        
        [JsonIgnore]
        public object Info { get; set; }
        public string Hash { get; set; }

        public string Path { get => (Info as FileSystemInfo)?.FullName; }
    }
}
