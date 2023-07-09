using DirectoryMiner;
using Microsoft.Extensions.Logging;
using TreeMiner;

internal class Main
{
    private readonly ILogger<Main> _logger;
    private readonly DirectoryExcavator _excavator;

    public Main(ILogger<Main> logger, DirectoryExcavator excavator)
    {
        _logger = logger;
        _excavator = excavator;
    }

    public void Run(string path)
    {
        var rootDir = new DirectoryInfo(path);
        var fileSystemMiner = new GenericTreeMiner<DirectoryArtifact, FileSystemInfo, FileInfo, DirectoryInfo>();
        var rootArtifact = fileSystemMiner.GetRootArtifact(rootDir);
        var dirArtifacts = fileSystemMiner.GetArtifacts(rootArtifact, _excavator, new ArtifactOptions() { ArtifactType = ArtifactType.Directories });


        var artifacts = dirArtifacts
            .GetProgress(1024, (count, item) => { _logger.LogInformation("Found {dir} directories in {path:l}", count, rootDir.Name); Console.Title = (item.Info as FileSystemInfo).FullName; })
            .OrderBy(a => a.Hash);

        _logger.LogInformation("Found {dirs} directories | {count} errors ", artifacts.Count(), _excavator.GetAggregateException().InnerExceptions.Count);

    }
}
