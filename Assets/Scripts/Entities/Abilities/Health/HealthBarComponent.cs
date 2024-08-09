using UnityEngine;
using UnityEngine.UI;

namespace Entities.Abilities.Health
{
    public class HealthBarComponent : MonoBehaviour
    {
        public Canvas canvas;
        public Image greenBar;

        // Start is called before the first frame update
        private void Start()
        {

        }

        // Update is called once per frame
        private void Update()
        {

        }

        private void OnDisable()
        {
            canvas.enabled = false;
        }
    }
}
