using System.IO;
using System.Reflection;

namespace JsonPatch.Json.Net.Tests.Helpers
{
    internal static class ResourceLoader
    {
        public static string LoadJson(string resourceFullPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceFullPath))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
