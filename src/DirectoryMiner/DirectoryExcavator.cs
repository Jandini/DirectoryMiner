using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using TreeMiner;

namespace DirectoryMiner
{
    internal class DirectoryExcavator : ITreeExcavator<DirectoryArtifact, FileSystemInfo, FileInfo, DirectoryInfo>
    {
        private readonly ILogger<DirectoryExcavator> _logger;
        private readonly List<Exception> _exceptions = new();

        public DirectoryExcavator(ILogger<DirectoryExcavator> logger)
        {
            _logger = logger;
        }

        public IEnumerable<FileSystemInfo> GetArtifacts(DirectoryInfo dirArtifact) => dirArtifact.GetFileSystemInfos();
        public AggregateException GetAggregateException() => new(_exceptions);

        public bool OnDirArtifact(DirectoryArtifact dirArtifact, IEnumerable<FileSystemInfo> dirContent)
        {
            var list = string.Join(';', dirContent.OrderBy(a => a.Name).Select(s => string.Join(',', s.Name, (s as FileInfo)?.Length ?? 0)));
            dirArtifact.Hash = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(list)));
            return true;
        }

        public bool OnException(Exception exception)
        {
            _exceptions.Add(exception);
            return true;
        }

        public bool OnFileArtifact(DirectoryArtifact fileArtifact)
        {
            throw new NotImplementedException();
        }

        
    }
}
