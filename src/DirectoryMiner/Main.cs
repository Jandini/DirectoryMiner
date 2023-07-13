using DirectoryMiner;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TreeMiner;

internal class Main
{
    private readonly ILogger<Main> _logger;
    private readonly DirectoryExcavator _excavator;

    const string EMPTY_DIR_HASH = "D41D8CD98F00B204E9800998ECF8427E";
    const string EMPTY_TREE_HASH = "80404D0C6D24E87F650FF7D1985CD762";

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
        var treeLookup = artifacts.ToLookup(n => n.ParentId);


        _logger.LogInformation("Computing tree hash...");

        foreach (var artifact in artifacts)
        {

            var descendants = artifact.Hash + string.Join("", GetDescendants(treeLookup, artifact).OrderBy(a => a.Hash).Select(a => a.Hash));

            artifact.TreePath = string.Join(Path.DirectorySeparatorChar, GetDescendants(treeLookup, artifact).Select(a => a.Name));

            // Create path backward !!!
            artifact.TreeHash = Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(descendants)));
        }





        _logger.LogInformation("Finding leaf artifacts...");
        var leafNodes = artifacts.Where(n => !treeLookup.Contains(n.Id)).ToArray();


        var artifactDictionary = new Dictionary<string, List<DirectoryArtifact>>();


        _logger.LogInformation("Building artifact dictionary...");
        foreach (var artifact in artifacts) {

            if (!artifactDictionary.ContainsKey(artifact.TreeHash))
                artifactDictionary[artifact.TreeHash] = new List<DirectoryArtifact> { artifact };
            else
            {
                artifactDictionary[artifact.TreeHash].Add(artifact);                
            }
        }
        

        _logger.LogInformation("Sorting duplicates by path...");
        foreach (var artifact in artifactDictionary.Values)
        {
            artifact.Sort(new DirectoryArtifactComparer());
        }

        _logger.LogInformation("Saving artifacts...");
        File.WriteAllText($"{rootDir.Name}.json", JsonSerializer.Serialize(artifacts.OrderBy(a => a.TreeHash).ThenBy(a => a.Level), new JsonSerializerOptions { WriteIndented = true }));
        

        _logger.LogInformation("Saving unique artifacts without empty folders...");
        File.WriteAllText($"{rootDir.Name}_unique.json", JsonSerializer.Serialize(artifactDictionary.Where(a => a.Key != EMPTY_TREE_HASH).OrderBy(a => a.Value.Count), new JsonSerializerOptions { WriteIndented = true }));
        File.WriteAllText($"{rootDir.Name}_unique_empty.json", JsonSerializer.Serialize(artifactDictionary.OrderBy(a => a.Value.Count), new JsonSerializerOptions { WriteIndented = true }));


        _logger.LogInformation("Calculating statistics...");

        var distinct = artifacts.GroupBy(a => a.TreeHash);

        var lowest = distinct
            .Select(g => g.OrderBy(d => d.Level));

        var empty = artifacts.Where(a => a.TreeHash == EMPTY_TREE_HASH);

        _logger.LogInformation("Found {dirs} directories | {count} errors | {unique} unique | {empty} empty ", artifacts.Count(), _excavator.GetAggregateException().InnerExceptions.Count, distinct.Count(), empty.Count());

    }



    public IEnumerable<DirectoryArtifact> GetDescendants(ILookup<Guid, DirectoryArtifact> treeLookup, DirectoryArtifact artifact)
    {        
        if (treeLookup.Contains(artifact.Id))
        {
            foreach (var childArtifact in treeLookup[artifact.Id])
            {
                yield return childArtifact;

                foreach (var descendant in GetDescendants(treeLookup, childArtifact))
                    yield return descendant;

            }
        }        
    }

}
