using UnityEngine;

public class DropdownToggle : MonoBehaviour
{
    public GameObject panel;
    public GameObject openIndicator, closedIndicator;
    public bool DefaultStateShouldBeOpen = false;
    
    private void Start()
    {
        panel.SetActive(DefaultStateShouldBeOpen);
        openIndicator.SetActive(DefaultStateShouldBeOpen);
        closedIndicator.SetActive(!DefaultStateShouldBeOpen);
    }

    public void Toggle()
    {
        panel.SetActive(!panel.activeSelf);
        openIndicator.SetActive(panel.activeSelf);
        closedIndicator.SetActive(!panel.activeSelf);
    }
}
