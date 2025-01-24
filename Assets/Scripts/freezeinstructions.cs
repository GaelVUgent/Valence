using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class freezeinstructions : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject instruction1;
    public GameObject instruction2;
    void Start()
    {
        StartCoroutine(ShowAndHide(3.0f));
    }
    IEnumerator ShowAndHide(float delay)
    {
        instruction1.SetActive(true);
        instruction2.SetActive(false);
        yield return new WaitForSeconds(delay);
        instruction1.SetActive(false);
        instruction2.SetActive(true);
    }


}
