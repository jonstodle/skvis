using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

string[] imageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff"];
var sourcePath = args[0];

var filePaths = (Directory.Exists(sourcePath) ? Directory.GetFiles(sourcePath)
        : File.Exists(sourcePath) ? [sourcePath]
        : throw new DirectoryNotFoundException($"Could not find any file or directory at '{sourcePath}'"))
    .Where(fp => imageExtensions.Contains(Path.GetExtension(fp).ToLower()))
    .ToList();

var imagesDone = 0;

filePaths
    .AsParallel()
    .WithDegreeOfParallelism(3)
    .ForAll(fp =>
    {
        var originalPath = Path.GetFullPath(fp);
        var destinationPath = Path.ChangeExtension(fp, "webp");
        
        Image.Load(originalPath)
            .SaveAsWebp(destinationPath, new WebpEncoder { Quality = 70 });
        
        var numberDone = Interlocked.Increment(ref imagesDone);
        Console.WriteLine($"[{numberDone.ToString().PadLeft(filePaths.Count.ToString().Length)}/{filePaths.Count}] done: {Path.GetFileName(destinationPath)}");
    });
