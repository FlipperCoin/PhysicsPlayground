using System.Text.Json;

namespace PhysicsPlayground.Display
{
    internal class JsonSerializerMetadataAdapter<T> : IMetadataAdapter<T>
    {
        public string GetMetadata(T metadataSource)
        {
            return JsonSerializer.Serialize(metadataSource, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }
}