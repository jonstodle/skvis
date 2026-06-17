using System.CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

var imageExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" };

RootCommand app = new("Skvis");

Option<int> qualityOption = new("--quality", "-q") { Description = "Output quality", DefaultValueFactory = _ => 70 };
app.Options.Add(qualityOption);

Option<int> maxBoundOption = new("--max", "-x") { Description = "Max width and height", DefaultValueFactory = _ => 0 };
app.Options.Add(maxBoundOption);

Argument<List<FileSystemInfo>> entriesArgument = new("paths")
{
    Description = "Paths to image files or directories containing image files",
    Arity = ArgumentArity.OneOrMore
};
app.Arguments.Add(entriesArgument);

app.SetAction(parseResult =>
{
    var filePaths = parseResult.GetRequiredValue(entriesArgument)
        .SelectMany(entry => Directory.Exists(entry.FullName) ? Directory.GetFiles(entry.FullName)
            : File.Exists(entry.FullName) ? [entry.FullName]
            : throw new DirectoryNotFoundException($"Could not find any file or directory at '{entry}'"))
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

            var image = Image.Load(originalPath);

            if (parseResult.GetValue(maxBoundOption) is var maxBound && maxBound > 0)
            {
                var newSize = image.Width > image.Height
                    ? new Size(maxBound, 0)
                    : new Size(0, maxBound);
                image.Mutate(it => it.Resize(newSize));
            }

            image.SaveAsWebp(
                destinationPath,
                new WebpEncoder { Quality = parseResult.GetRequiredValue(qualityOption) });

            var numberDone = Interlocked.Increment(ref imagesDone);
            Console.WriteLine(
                $"[{numberDone.ToString().PadLeft(filePaths.Count.ToString().Length)}/{filePaths.Count}] done: {Path.GetFileName(destinationPath)}");
        });
});

app.Parse(args).Invoke();
