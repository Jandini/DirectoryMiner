namespace DirectoryMiner
{
    internal class DirectoryArtifactComparer : IComparer<DirectoryArtifact>
    {
        public int Compare(DirectoryArtifact x, DirectoryArtifact y)
        {
            return string.Compare(x.Path, y.Path, StringComparison.OrdinalIgnoreCase);
        }
    }
}
