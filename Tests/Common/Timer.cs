using UnityEngine;

namespace Tests.Common
{
    internal struct Timer
    {
        private float _endTime;

        public Timer(float duration)
        {
            _endTime = Time.time + duration;
        }
        
        public bool IsRunning => Time.time < _endTime;
    }
}