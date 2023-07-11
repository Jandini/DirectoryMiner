using DirectoryMiner;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using TreeMiner;

internal class Main
{
    private readonly ILogger<Main> _logger;
    private readonly DirectoryExcavator _excavator;

    const string EMPTY_DIR_HASH = "D41D8CD98F00B204E9800998ECF8427E";

    public Main(ILogger<Main> logger, DirectoryExcavator excavator)
    {
        _logger = logger;
        _excavator = excavator;
    }

    public void Run(string path)
    {        

        var rootDir = new DirectoryInfo(path);
        var fileSystemMiner = new GenericTreeMiner<DirectoryArtifact, FileSystemInfo, FileInfo, DirectoryInfo>();
        var rootArtifact = fileSystemMiner.GetRootArtifact(rootDir, -1);
        var dirArtifacts = fileSystemMiner.GetArtifacts(rootArtifact, _excavator, new ArtifactOptions() { ArtifactType = ArtifactType.Directories });

        var artifacts = dirArtifacts
            .GetProgress(1024, (count, item) => { _logger.LogInformation("Found {dir} directories in {path:l}", count, rootDir.Name); Console.Title = (item.Info as FileSystemInfo).FullName; })
            .ToList();

        _logger.LogInformation("Creating parent-child lookup...");
        var parentChild = artifacts.ToLookup(n => n.ParentId, n => n.Id);


        var rootArtifacts = artifacts.Where(a => a.Level == 0).ToList();

        // Compute TreeHash - foreach artifact, traverse forward and compute TreeHash from all directory artifact hashes



        //foreach (var rootArtifact in rootArtifacts) {
        //    TraverseForward(parentChild, rootArtifact);
        //}




        _logger.LogInformation("Finding leaf artifacts...");
        var leafNodes = artifacts.Where(n => !parentChild.Contains(n.Id)).ToArray();


        var artifactDictionary = new Dictionary<string, List<DirectoryArtifact>>();


        _logger.LogInformation("Building artifact dictionary...");
        foreach (var artifact in artifacts) {

            if (!artifactDictionary.ContainsKey(artifact.Hash))
                artifactDictionary[artifact.Hash] = new List<DirectoryArtifact> { artifact };
            else
            {
                artifactDictionary[artifact.Hash].Add(artifact);                
            }
        }
        

        _logger.LogInformation("Sorting duplicates by path...");
        foreach (var artifact in artifactDictionary.Values)
        {
            artifact.Sort(new DirectoryArtifactComparer());
        }

        _logger.LogInformation("Saving artifacts...");
        File.WriteAllText($"{rootDir.Name}.json", JsonSerializer.Serialize(artifacts.OrderBy(a => a.Hash).ThenBy(a => a.Level), new JsonSerializerOptions { WriteIndented = true }));
        

        _logger.LogInformation("Saving unique artifacts without empty folders...");
        File.WriteAllText($"{rootDir.Name}_unique.json", JsonSerializer.Serialize(artifactDictionary.Where(a=>a.Key != EMPTY_DIR_HASH).OrderByDescending(a => a.Value.Count), new JsonSerializerOptions { WriteIndented = true }));


        _logger.LogInformation("Calculating statistics...");

        var distinct = artifacts.GroupBy(a => a.Hash);

        var lowest = distinct
            .Select(g => g.OrderBy(d => d.Level));

        var empty = artifacts.Where(a => a.Hash == EMPTY_DIR_HASH);

        _logger.LogInformation("Found {dirs} directories | {count} errors | {unique} unique | {empty} empty ", artifacts.Count(), _excavator.GetAggregateException().InnerExceptions.Count, distinct.Count(), empty.Count());

    }

    public void TraverseBackward(Dictionary<Guid, Guid> childParentDict, Guid currentNode)
    {
        Console.WriteLine("Current Node: " + currentNode);

        if (childParentDict.ContainsKey(currentNode))
        {
            Guid parentNode = childParentDict[currentNode];
            TraverseBackward(childParentDict, parentNode);
        }
    }

    public void TraverseForward(ILookup<Guid, Guid> parentChildDict, DirectoryArtifact artifact)
    {
        if (parentChildDict.Contains(artifact.Id))
        {
            var childNodes = parentChildDict[artifact.Id];

            foreach (var childNode in childNodes)
            {
                TraverseForward(parentChildDict, parentChildDict[ childNode);
            }
        }
    }
}
