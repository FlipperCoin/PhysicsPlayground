using System;
using PhysicsPlayground.Runtime;

namespace PhysicsPlayground.Display
{
    class MetadataProvider : IMetadataProvider
    {
        private readonly Func<string> _getMetadata;
        public string Metadata => _getMetadata();

        public MetadataProvider(Func<string> getMetadata)
        {
            _getMetadata = getMetadata;
        }

        public static MetadataProvider CreateInstance<T>(IObjectProvider<T> objectProvider,
            IMetadataAdapter<T> metadataAdapter)
        {
            return new MetadataProvider(() => metadataAdapter.GetMetadata(objectProvider.GetObject()));
        }
    }
}