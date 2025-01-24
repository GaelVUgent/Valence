using UnityEngine;

public class DisplayKinectLayover : MonoBehaviour
{
    public KeyCode Key;
    public Camera Camera;
    private bool _isShowing = true;

    private void Start()
    {
        Camera.enabled = _isShowing;
    }

    void Update()
    {
        if (Input.GetKeyUp(Key)) {
            _isShowing = !_isShowing;
            Camera.enabled = _isShowing;
        }
    }
}
