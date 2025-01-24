using UnityEngine;

/// <summary>
/// Used to turn off scene helper objects which only exist to help the 
/// developer in the Unity editor, but should be disabled during runtime.
/// </summary>
public class SceneHelper : MonoBehaviour {
    private void Start() {
        gameObject.SetActive(false);
    }
}
