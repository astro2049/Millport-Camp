using Unity.AI.Navigation;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Texture2D cursorTexture;
    
    private NavMeshSurface m_navMeshSurface;

    // Start is called before the first frame update
    private void Start()
    {
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        
        m_navMeshSurface = GetComponent<NavMeshSurface>();
        
        // Build nav mesh
        // m_navMeshSurface.BuildNavMesh();
    }

    // Update is called once per frame
    private void Update()
    {

    }
}
