using UnityEditor;
using UnityEngine;

namespace HyperTween.ECS.Update.Components
{
    [CustomEditor(typeof(TweenHermiteEasingTester))]
    public class TweenHermiteEasingTesterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            TweenHermiteEasingTester tweenHermiteEasingTester = (TweenHermiteEasingTester)target;

            for (var i = 0; i < tweenHermiteEasingTester.Templates.Length; i++)
            {
                var template = tweenHermiteEasingTester.Templates[i];
                
                EditorGUILayout.LabelField($"m0: {template.m0}, m1: {template.m1}");
                Rect curveRect = EditorGUILayout.GetControlRect(GUILayout.Height(300));
                EditorGUI.CurveField(curveRect, "Animation Curve", tweenHermiteEasingTester.AnimationCurves[i]);
            }
        }
    }
    
    [CreateAssetMenu(menuName = "HyperTween/Create TweenHermiteEasingTester", fileName = "TweenHermiteEasingTester", order = 0)]
    public class TweenHermiteEasingTester : ScriptableObject
    {
        public TweenHermiteEasingArgs[] Templates;
        [SerializeField] public AnimationCurve[] AnimationCurves;
        
        [ContextMenu("GenerateTemplates")]
        void GenerateTemplates()
        {
            Templates = new TweenHermiteEasingArgs[4];
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    Templates[i*2+j] = new TweenHermiteEasingArgs()
                    {
                        m0 = i,
                        m1 = j,
                    };
                }
            }
        }
        
        private void OnValidate()
        {
            if (AnimationCurves?.Length != Templates.Length)
            {
                AnimationCurves = new AnimationCurve[Templates.Length];
            }

            for (var index = 0; index < Templates.Length; index++)
            {
                var animationCurve = AnimationCurves[index];
                animationCurve.ClearKeys();
                
                var template = Templates[index];
                var interpolator = new TweenHermiteEasing(template.m0, template.m1);

                for (int i = 0; i <= 100; i++)
                {
                    var param = (float)i / 100;
                    animationCurve.AddKey(param, interpolator.Interpolate(param));
                }
            }
        }
    }
}