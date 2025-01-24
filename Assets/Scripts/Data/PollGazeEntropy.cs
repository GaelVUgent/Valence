using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PollGazeEntropy : MonoBehaviour
{

    public float pollInterval = 1f;
    public GazeEntropyUpdate onGazeEntropyUpdate;

    private Coroutine poll;

    [Serializable]
    public class GazeEntropyUpdate : UnityEvent<float> { }

    private void OnEnable() {
        poll = StartCoroutine(Poll());
    }

    private void OnDisable() {
        StopCoroutine(poll);
    }

    private IEnumerator Poll() {
        while(true) {
            yield return new WaitForSeconds(pollInterval);

            //Psuedo random dummy implementation
            float gazeEntropy = Mathf.PerlinNoise(0f, .3f * Time.time);
            onGazeEntropyUpdate.Invoke(gazeEntropy);
        }
    }
}
