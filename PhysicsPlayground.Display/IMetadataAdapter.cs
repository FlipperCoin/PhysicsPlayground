namespace PhysicsPlayground.Display
{
    internal interface IMetadataAdapter<T>
    {
        string GetMetadata(T metadataSource);
    }
}