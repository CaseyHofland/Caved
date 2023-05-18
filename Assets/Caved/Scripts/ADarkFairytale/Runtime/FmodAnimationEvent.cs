#nullable enable
using FMODUnity;
using UnityEngine;
using UnityExtras;

[CreateAssetMenu(fileName = nameof(FmodAnimationEvent), menuName = nameof(ScriptableAnimationEvent) + "/" + nameof(FmodAnimationEvent))]
public class FmodAnimationEvent : ScriptableAnimationEvent
{
    public EventReference sound;
    public Vector3 rootOffset;

    public override void Play(ScriptableAnimationEventListener listener, AnimationEvent animationEvent)
    {
        RuntimeManager.PlayOneShot(sound, listener.transform.position + rootOffset);
        base.Play(listener, animationEvent);
    }
}
