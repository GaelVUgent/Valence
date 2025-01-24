using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Mict.Widgets
{
    public class TimedMessageWidget : MonoBehaviour
    {
        public TMP_Text TMPText;
        public Text Text;

        public event Action<string> OnNewMessageShown;
        public List<string> MessagesToBeShown;

        public bool AutomaticallyShowNextMessage = false;
        public bool ShowFirstMessageOnAwake = true;
        public bool LoopMessages = false;
        public float VisibilityDuration = 2f;

        public UnityStringEvent ActionWhenNewMessageIsShown;

        private int _currentIndex = 0;
        private Coroutine _runSetTimer;


        #region Monobehaviours
        void Start()
        {
            if (ShowFirstMessageOnAwake)
            {
                StartShowingMessages();
            }
        }
        

        private void OnDestroy()
        {
            if (_runSetTimer != null)
            {
                StopCoroutine(_runSetTimer);
            }
        }
        #endregion

        #region Public methods
        public void StartShowingMessages(bool showFirstMessage = true)
        {
            if (showFirstMessage)
            {
                _currentIndex = 0;
            }
            ShowNewMessage(false);
        }

        public void ShowNewMessage()
        {
            ShowNewMessage(true);
        }

        public void SetAutomaticPlay(bool shouldPlayAutomatically) {
            AutomaticallyShowNextMessage = shouldPlayAutomatically;
        }


        private void ShowNewMessage(bool goToNextMessage = true)
        {
            if (!LoopMessages && _currentIndex == MessagesToBeShown.Count - 1)
                return;

            if (goToNextMessage)
            {
                _currentIndex++;
                //This is hardcoded and very specific to the setup you want right now. (read: bad)
                //I added a new function (line 63) so that you can trigger this after a certain step. Linking can then be done e.g. in the Editor with the UnityEvents
                //AutomaticallyShowNextMessage = true; // code jamil
            }

            if (LoopMessages && _currentIndex == MessagesToBeShown.Count)
            {
                _currentIndex = 0;
            }

            if (_runSetTimer != null)
            {
                StopCoroutine(_runSetTimer);
            }

            _runSetTimer = StartCoroutine(RunSetTimer(OnMessageShownAndWaited));

            var text = GetCurrentMessage();
            TMPText?.SetText(text);
            if (Text)
            {
                Text.text = text;
            }
            OnNewMessageShown?.Invoke(text);
            print("invoking");
            ActionWhenNewMessageIsShown?.Invoke(text);

        }

        public string GetCurrentMessage() {
            if (_currentIndex < MessagesToBeShown.Count)
                return MessagesToBeShown[_currentIndex];
            else return "";
        }
        #endregion

        #region Private methods
        private void OnMessageShownAndWaited()
        {
            if (AutomaticallyShowNextMessage)
            {
                ShowNewMessage();
            }
        }

        private IEnumerator RunSetTimer(Action callback)
        {
            yield return new WaitForSeconds(VisibilityDuration);
            callback?.Invoke();
            _runSetTimer = null;
        }
        #endregion
    }

    [System.Serializable]
    public class UnityStringEvent : UnityEvent<string>
    {
    }
}
