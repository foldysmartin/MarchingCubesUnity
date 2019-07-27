using Assets.Scripts.Generators.Mesh;
using UnityEngine;

namespace Assets.Scripts.Generators.Colour
{
    [ExecuteInEditMode]
    public class EditorColourGenerator : AbstractColourGenerator
    {
        public AbstractMeshGenerator meshGenerator; 

        void Update () {
            Init ();
            UpdateTexture ();
        
            float boundsY = meshGenerator.boundsSize * meshGenerator.numChunks.y;

            mat.SetFloat ("boundsY", boundsY);
            mat.SetFloat ("normalOffsetWeight", normalOffsetWeight);

            mat.SetTexture ("ramp", texture);
        }
    }
}