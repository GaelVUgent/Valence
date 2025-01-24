using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace MICT.eDNA.Helpers
{
    public class Timer : MonoBehaviour
    {
        public bool StartOnAwake = false;
        public bool StartOnEnable = false;
        public float BeginValue = 0;
        public float EndValue = 1;
        public bool ShouldLoop = false;

        public UnityEventWithFloat OnTimerChanged;
        public UnityEventWithFloat OnNormalisedTimerChanged;
        public UnityEvent OnTimerStarted;
        public UnityEvent OnTimerEnded;

        protected float _timer;
        protected Coroutine _run;

        private void Start()
        {
            if (StartOnAwake)
            {
                StartTimer();
            }
        }

        private void OnEnable()
        {
            if (StartOnEnable)
            {
                StartTimer();
            }
        }

        public void StartTimer()
        {
            StopTimer();
            OnTimerStarted?.Invoke();
            _run = StartCoroutine(RunTimer());
        }

        public void StopTimer()
        {
            if (_run != null)
            {
                StopCoroutine(_run);
                _run = null;
            }
        }

        protected virtual IEnumerator RunTimer()
        {
            _timer = BeginValue;
            while (_timer <= EndValue)
            {
                _timer += Time.deltaTime;
                OnTimerChanged?.Invoke(_timer);
                OnNormalisedTimerChanged?.Invoke(_timer / (EndValue - BeginValue));
                yield return null;

            }
            _timer = EndValue;
            OnTimerEnded?.Invoke();
            if (ShouldLoop)
            {
                yield return RunTimer();
            }
        }
    }
}
