using UnityEngine;
using HyperTween.API;
using HyperTween.Modules.InvokeAction.API;
using HyperTween.Modules.LocalTransform.API;
using HyperTween.Modules.Transform.API;
using HyperTween.TweenBuilders;

public class PlaySoundNode : MonoBehaviour
{
    public PlaySoundNode[] subsequentNodes;
    public AudioSource audioSourcePrefab;
    public string note = "C3";
    public float speed = 1f;
    public float triggerAferDelay;
    public float hue;
    
    private float _pitch;

    private void Start()
    {
        _pitch = PitchUtils.ConvertNoteToPitchMultiplier(note);

        if (triggerAferDelay >= 0)
        {
            HyperTweenFactory.CreateTween()
                .WithName("TriggerAferDelay")
                .WithDuration(triggerAferDelay)
                .InvokeActionOnStop(context => Trigger(context.EntityCommandBuffer.CreateTweenFactory(), context.TweenDurationOverflow.Value))
                .Play();
        }
        
    }

    public void Trigger<T>(TweenFactory<T> tweenFactory, float skipDuration) where T : unmanaged, ITweenBuilder
    {
        foreach (var node in subsequentNodes)
        {
            var audioSourceInstance = Instantiate(audioSourcePrefab, transform.position, transform.rotation);
            audioSourceInstance.pitch = _pitch;

            var clipLength = audioSourceInstance.clip.length / _pitch;
            var travelDuration = Vector3.Distance(transform.position, node.transform.position) / speed;

            audioSourceInstance.PlayScheduled(AudioSettings.dspTime - skipDuration);
            
            Destroy(audioSourceInstance.gameObject, Mathf.Max(clipLength, travelDuration) + 2f);

            var particleSystems = audioSourceInstance.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                var main = ps.main;
                main.startColor = Color.HSVToRGB(hue, 1f, 1f);
            }

            tweenFactory.CreateTween()
                .WithDuration(travelDuration)
                .WithTransform(audioSourceInstance.transform)
                .WithLocalTransform()
                .WithLocalPositionOutput(node.transform.position)
                .WithEaseOut()
                .InvokeActionOnStop(context  =>
                {
                    node.Trigger(context.EntityCommandBuffer.CreateTweenFactory(), context.TweenDurationOverflow.Value);
                })
                .Play(skipDuration);
        }
    }
}
