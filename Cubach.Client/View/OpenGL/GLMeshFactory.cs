namespace Cubach.View.OpenGL
{
    public class GLMeshFactory : IMeshFactory
    {
        public IMesh<TVertex> Create<TVertex>(TVertex[] data, MeshUsageHint usage = MeshUsageHint.Static) where TVertex : struct, IVertex
        {
            var mesh = new GLMesh<TVertex>();
            mesh.SetData(data, usage);
            return mesh;
        }
    }
}
