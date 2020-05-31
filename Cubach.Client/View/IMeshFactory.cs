namespace Cubach.View
{
    public interface IMeshFactory
    {
        IMesh<TVertex> Create<TVertex>(TVertex[] data, MeshUsageHint usage = MeshUsageHint.Static) where TVertex : struct, IVertex;
    }
}
