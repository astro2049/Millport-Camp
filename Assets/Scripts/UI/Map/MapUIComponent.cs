using Gameplay;
using PCG;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Map
{
    public class MapUIComponent : MonoBehaviour
    {
        [SerializeField] private Image mapImage;
        [SerializeField] private Image playerIcon;

        private float mapUnit; // in world unit, which is m

        public void Initialize(Texture2D mapTexture2D)
        {
            // Set map image's sprite
            Sprite mapSprite = Sprite.Create(mapTexture2D, new Rect(0, 0, mapTexture2D.width, mapTexture2D.height), new Vector2(0.5f, 0.5f));
            mapImage.sprite = mapSprite;

            // Precalculate map unit 
            mapUnit = mapImage.transform.GetComponent<RectTransform>().rect.width / (WorldConfigurations.s_worldGridSize * WorldConfigurations.c_chunkSize);
        }

        public void UpdatePlayerLocation(Transform playerTransform)
        {
            // Set player icon position
            playerIcon.transform.localPosition = new Vector3(playerTransform.position.x, playerTransform.position.z, 0) * mapUnit;

            // Set player icon rotation
            playerIcon.transform.localRotation = Quaternion.Euler(0, 0, -playerTransform.rotation.eulerAngles.y - 45f);
        }
    }
}
