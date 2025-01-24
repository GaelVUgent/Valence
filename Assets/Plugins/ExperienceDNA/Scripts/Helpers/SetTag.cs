using System.Collections;
using UnityEngine;

public class SetTag : MonoBehaviour
{
    public string Tag;
    public float WaitDuration = 0f;
    IEnumerator Start()
    {

        yield return new WaitForSeconds(WaitDuration);
        if(!string.IsNullOrEmpty(Tag))
        gameObject.tag = Tag;
    }
}
