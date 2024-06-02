using Unity.AI.Navigation;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Texture2D cursorTexture;
    
    private NavMeshSurface m_navMeshSurface;

    // Start is called before the first frame update
    private void Start()
    {
        // https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Cursor.SetCursor.html
        Cursor.SetCursor(cursorTexture, new Vector2(cursorTexture.width / 2f, cursorTexture.height / 2f), CursorMode.Auto);
        
        m_navMeshSurface = GetComponent<NavMeshSurface>();
        
        // Build nav mesh
        // m_navMeshSurface.BuildNavMesh();
    }

    // Update is called once per frame
    private void Update()
    {

    }
}
