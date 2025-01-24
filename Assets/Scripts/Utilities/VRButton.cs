using UnityEngine;
using UnityEngine.Events;

public class VRButton : MonoBehaviour
{

    public float delay;
    public UnityEvent onPress;

    public static VRButton focusedButton;

    private Animator anim;
    private float t;
    private int collisionCount;

    private const float MIN_UNSTUCK = 2f;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        Reset();
    }

    private void OnDisable()
    {
        Reset();
    }

    /*
     * Keep track of total amount of collisions (trying to activate this button)
     * If a collision occurs, this button becomes the global 'focused button'.
     * This disallows other buttons from being pressed until this focus state expires, 
     * when either the button is forcefully reset or no more collisions occur.
     */


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GrabProbe"))
            return;
        collisionCount++;
        if (focusedButton == null)
            focusedButton = this;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("GrabProbe"))
            return;
        collisionCount = Mathf.Max(0, collisionCount - 1);
        if (collisionCount == 0 & focusedButton)
            focusedButton = null;
    }

    private void Reset()
    {
        t = 0f;
        collisionCount = 0;
        if (this == focusedButton)
            focusedButton = null;
    }

    private void Update()
    {

        if (this == focusedButton)
        {

            //trigger button when time passes over predefined delay
            bool trigger = t <= delay;
            t += Time.deltaTime;
            trigger &= t >= delay;
            if (trigger)
                onPress.Invoke();

            //unstuck this button in case of a wayward or erroneous colission
            float unstuck = Mathf.Max(MIN_UNSTUCK, 2f * delay);
            if (t >= unstuck)
                Reset();
        }

        //without focus this button immediately resets
        else
            t = 0f;

        //apply animation value; completion circle.
        if (anim != null)
            anim.SetFloat("Progress", Mathf.Clamp01(t / delay));
    }
}
