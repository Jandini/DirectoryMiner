using Microsoft.Extensions.Logging;
using System.Drawing;
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
            string content = string.Empty;
            if (dirContent.Any()) {
                content = string.Join(';', dirContent.OrderBy(a => a.Name).Select(s => string.Join(',', s.Name, (s as FileInfo)?.Length ?? 0)));
            }
            else
            {
                // content = (dirArtifact.Info as FileSystemInfo).Name;
                content = string.Empty;
            }

            dirArtifact.Hash = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(content)));
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
