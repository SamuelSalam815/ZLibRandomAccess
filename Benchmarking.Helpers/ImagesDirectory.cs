namespace Benchmarking.Helpers;

public static class ImagesDirectory
{
    /// <summary>
    /// The directory benchmark graphs are written to: the first command line argument when given, otherwise the
    /// <c>images</c> directory at the repository root (found by looking for the solution file above the executable).
    /// </summary>
    public static string Resolve(string[] args)
    {
        if (args.Length > 0)
        {
            return Path.GetFullPath(args[0]);
        }

        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (directory.EnumerateFiles("*.slnx").Any())
            {
                return Path.Combine(directory.FullName, "images");
            }
        }

        throw new InvalidOperationException(
            "Could not find the repository root. Pass the output directory as the first argument.");
    }
}
