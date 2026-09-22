using System.Reflection;
using System.Text.RegularExpressions;

namespace IESEViewModel.Tools;

public class ResourceReader
{
    private readonly string? _path;

    public List<string>? Content;

    public ResourceReader()
    {

    }

    public ResourceReader(string file)
    {
        _path = file;
        string text = new StreamReader(GetResourceStream()!).ReadToEnd();
        Content = Regex.Split(text, "\r\n|\r|\n").ToList();
    }

    private Stream? GetResourceStream()
    {
        string? resourcePath = _path;
        Assembly executingAssembly = Assembly.GetExecutingAssembly();
        List<string> source = new(executingAssembly.GetManifestResourceNames());
        resourcePath = source.FirstOrDefault(r => resourcePath != null && r.Contains(resourcePath));
        if (resourcePath == null)
        {
            throw new FileNotFoundException("Resource not found");
        }
        return executingAssembly.GetManifestResourceStream(resourcePath);
    }
}