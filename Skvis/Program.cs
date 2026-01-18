using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

string[] imageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff"];
var sourcePath = args[0];

var filePaths = Directory.Exists(sourcePath) ? Directory.GetFiles(sourcePath)
    : File.Exists(sourcePath) ? [sourcePath]
    : throw new DirectoryNotFoundException($"Could not find any file or directory at '{sourcePath}'");

filePaths
    .Where(fp => imageExtensions.Contains(Path.GetExtension(fp).ToLower()))
    .AsParallel()
    .WithDegreeOfParallelism(3)
    .ForAll(fp =>
    {
        var originalPath = Path.GetFullPath(fp);
        var destinationPath = Path.ChangeExtension(fp, "webp");
        Image.Load(originalPath)
            .SaveAsWebp(destinationPath, new WebpEncoder { Quality = 70 });
    });
