using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Texture2D cursorTexture;

    // Start is called before the first frame update
    private void Start()
    {
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
    }

    // Update is called once per frame
    private void Update()
    {

    }
}
