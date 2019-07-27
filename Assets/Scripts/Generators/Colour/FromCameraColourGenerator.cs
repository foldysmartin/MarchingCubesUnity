using Assets.Scripts.Generators.Mesh;
using UnityEngine;

namespace Assets.Scripts.Generators.Colour
{
    public class FromCameraColourGenerator : AbstractColourGenerator
    {
        
        [Range (0, 1)]
        public float fogDstMultiplier = 1;
        public Vector4 shaderParams;
        public ContinuesMeshGenerator meshGenerator;
        public Camera camera;

        void Update () {
            Init ();
            UpdateTexture ();
        
            mat.SetTexture ("ramp", texture);
            mat.SetVector("params",shaderParams);

            RenderSettings.fogColor = camera.backgroundColor;

            //Todo put view distance in config?
            RenderSettings.fogEndDistance = meshGenerator.viewDistance * fogDstMultiplier;
            Debug.Log(RenderSettings.fogEndDistance);
        }
    }
}