namespace ModularSaaS.Architecture.Tests.Support;

internal static class RepoPaths
{
    public static string ProjectDir() =>
        Ascend(d => Directory.GetFiles(d, "*Architecture.Tests.csproj").Length > 0)
        ?? throw new InvalidOperationException("Architecture.Tests project directory not found.");

    public static string? Root() =>
        Ascend(d => Directory.Exists(Path.Combine(d, ".git")) ||
                    Directory.GetFiles(d, "*.sln").Length > 0 ||
                    Directory.GetFiles(d, "*.slnx").Length > 0);

    private static string? Ascend(Func<string, bool> found)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (found(dir.FullName))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        return null;
    }
}
