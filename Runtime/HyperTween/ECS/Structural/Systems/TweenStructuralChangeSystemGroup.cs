//#define DEBUG_ITERATIONS

#if DEBUG_ITERATIONS
using System.Globalization;
using System.Linq;
using UnityEngine;
#endif

using Unity.Entities;
using Unity.Profiling;
using UnityEngine.Scripting;

namespace HyperTween.ECS.Structural.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class TweenStructuralChangeSystemGroup : ComponentSystemGroup, IRateManager
    {
        private bool _isDirty;
        
#if DEBUG_ITERATIONS
        private int _numIterations, _numUpdates;
        private readonly int[] _iterationHistogram = new int[100];
#endif
        
        // TODO: This should use a Singleton/System version checks
        public void MarkDirty()
        {
            _isDirty = true;
        }
            
        // TODO: This should use a Singleton/System version checks
        public void MarkClean()
        {
            _isDirty = false;
        }
        
        /// <summary>
        /// Default constructor
        /// </summary>
        [Preserve]
        public TweenStructuralChangeSystemGroup()
        {
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            RateManager = this;
        }

        protected override void OnUpdate()
        {
            // We always need to play at least once, because we might be using EntityManager to play Tweens
            // There is potential to avoid this, by _always_ playing Tweens via PreTweenStructuralChangeECBSystem
            MarkDirty();
            
            using (new ProfilerMarker("TweenStructuralChangeSystemGroup").Auto())
            {
                base.OnUpdate();
            }
            
#if DEBUG_ITERATIONS
            _iterationHistogram[_numIterations]++;
            _numIterations = 0;
            _numUpdates++;
#endif
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

#if DEBUG_ITERATIONS
            var output = string.Join(", ", _iterationHistogram
                .Select(i => (float)i/_numUpdates)
                .Select(i => i.ToString(CultureInfo.InvariantCulture)));
            Debug.Log(output);
#endif
        }

        public bool ShouldGroupUpdate(ComponentSystemGroup group)
        {
#if DEBUG_ITERATIONS
            if (_isDirty)
            {
                _numIterations++;
            }
#endif
            return _isDirty;
        }

        public float Timestep { get; set; }
    }
}