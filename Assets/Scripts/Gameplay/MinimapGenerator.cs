using System.IO;
using Managers;
using UnityEngine;

namespace Gameplay
{
    public class MinimapGenerator : MonoBehaviour
    {
        [SerializeField] private string pngName = "minimap.png";
        [SerializeField] private int pngResolution = 1080;
        [SerializeField] private Camera mapCamera;

        [SerializeField] private UIManager uiManager;

        // Reference: https://discussions.unity.com/t/rendering-screenshot-larger-than-screen-resolution/542549/6
        public void StreamWorldToMinimap(int worldSize)
        {
            // Adjust bird's eye view camera's orthographic size (half of vertical/world size)
            mapCamera.orthographicSize = worldSize / 2f;

            // Set up render texture and texture 2D (for later output to png)
            RenderTexture mapRenderTexture = new RenderTexture(pngResolution, pngResolution, 24, RenderTextureFormat.ARGB32);
            mapCamera.targetTexture = mapRenderTexture;
            Texture2D mapTexture2D = new Texture2D(pngResolution, pngResolution, TextureFormat.RGB24, false);

            // Render camera image to render texture
            mapCamera.Render();
            // Read the image from tender texture to texture 2D
            RenderTexture.active = mapRenderTexture;
            mapTexture2D.ReadPixels(new Rect(0, 0, pngResolution, pngResolution), 0, 0);
            mapTexture2D.Apply();

            // Destroy render texture
            mapCamera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(mapRenderTexture);

            // Write this texture 2D png to disk
            byte[] bytes = mapTexture2D.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(Application.dataPath, pngName), bytes);

            uiManager.RenderMapUIs(mapTexture2D);
        }
    }
}
