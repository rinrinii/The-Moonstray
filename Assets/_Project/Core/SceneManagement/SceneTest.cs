using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTest : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SceneManager.LoadScene("Snow Courtyard");   // replace with scene name to test, then attach script temporarily to GameStateManager
        }
    }
}