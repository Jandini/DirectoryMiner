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

        /// <summary>
        /// Hash of all artifacts in current directory artifact.
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Hash of all descendant artifacts.
        /// </summary>
        public string TreeHash { get; set; }

        /// <summary>
        /// Full path to the directory artifact.
        /// </summary>
        public string Path { get => (Info as FileSystemInfo)?.FullName; }
    }
}
