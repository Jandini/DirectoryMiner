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

        public DateTime LastWriteTimeUtc { get => (Info as FileSystemInfo).LastWriteTimeUtc; }

        /// <summary>
        /// Hash of all artifacts in current directory artifact.
        /// </summary>
        public string Hash { get; set; }

        /// <summary>
        /// Hash of all descendant artifacts.
        /// </summary>
        public string TreeHash { get; set; }

        public string Name { get => (Info as FileSystemInfo)?.Name; }


        public string FullPath { get => (Info as FileSystemInfo)?.FullName; }


        public string RootPath { get => string.Join(Path.DirectorySeparatorChar, (Info as FileSystemInfo)?.FullName.Split(Path.DirectorySeparatorChar).Take(Level)); }

        /// <summary>
        /// Full path to the directory artifact.
        /// </summary>
        //public string TreePath { get => string.Join(Path.DirectorySeparatorChar, (Info as FileSystemInfo)?.FullName.Split(Path.DirectorySeparatorChar).Skip(Level)); }
        public string TreePath { get; set; }
    }
}
