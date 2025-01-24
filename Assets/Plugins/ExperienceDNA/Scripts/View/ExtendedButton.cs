namespace UnityEngine.UI
{
    public class ExtendedButton : Button
    {
        public void InvokeClick()
        {
            onClick?.Invoke();
        }

        public void InvokeSafeClick()
        {
            if (interactable)
            {
                onClick?.Invoke();
            }
        }
    }
}
