using Assets.Scripts.Generators.Mesh;

namespace Assets.Scripts.Generators.Density
{
    public class EditorNoiseDensityGenerator : NoiseDensityGenerator
    {
        public EditorMeshGenerator editorMeshGenerator;
        void OnValidate()
        {
            editorMeshGenerator.RequestMeshUpdate();
        }
    }
}
