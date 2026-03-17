using UnityEngine;

public class CursorConfig : MonoBehaviour
{
    void Start()
    {
        // 1. Forces the cursor to be visible
        Cursor.visible = true;
        // 2. Unlocks the cursor so it can move freely around the screen
        Cursor.lockState = CursorLockMode.None; 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
