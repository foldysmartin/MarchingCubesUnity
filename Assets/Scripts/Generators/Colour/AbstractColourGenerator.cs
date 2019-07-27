using Assets.Scripts.Generators.Mesh;
using UnityEngine;

namespace Assets.Scripts.Generators.Colour
{
    public class AbstractColourGenerator : MonoBehaviour {
        public Material mat;
        public Gradient gradient;
        public float normalOffsetWeight;

        protected Texture2D texture;
        private const int TextureResolution = 50;

        protected void Init()
        {
            if (texture == null || texture.width != TextureResolution)
            {
                texture = new Texture2D(TextureResolution, 1, TextureFormat.RGBA32, false);
            }
        }

        protected void UpdateTexture()
        {
            if (gradient != null)
            {
                Color[] colours = new Color[texture.width];
                for (int i = 0; i < TextureResolution; i++)
                {
                    Color gradientCol = gradient.Evaluate(i / (TextureResolution - 1f));
                    colours[i] = gradientCol;
                }

                texture.SetPixels(colours);
                texture.Apply();
            }
        }
    }
}