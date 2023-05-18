#nullable enable
using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityExtras;

[CreateAssetMenu(fileName = nameof(HitFmodAnimationEvent), menuName = nameof(ScriptableAnimationEvent) + "/" + nameof(HitFmodAnimationEvent))]
public class HitFmodAnimationEvent : ScriptableAnimationEvent, IDataReceiver<RaycastHit>, IDataReceiver<Texture?>
{
    [Serializable]
    public struct Wrapper
    {
        public Texture texture;
        public EventReference sound;
    }

    public List<Wrapper> textureSounds = new();
    public EventReference defaultSound;

    RaycastHit IDataReceiver<RaycastHit>.value { set => hit = value; }
    Texture? IDataReceiver<Texture?>.value { set => hitTexture = value; }

    private RaycastHit hit;
    private Texture? hitTexture;

    public override void Play(ScriptableAnimationEventListener listener, AnimationEvent animationEvent)
    {
        if (hit.collider != null)
        {
            var wrapperIndex = textureSounds.FindIndex(wrapper => wrapper.texture == hitTexture);
            RuntimeManager.PlayOneShot(wrapperIndex == -1 ? defaultSound : textureSounds[wrapperIndex].sound, hit.point);
        }

        base.Play(listener, animationEvent);
    }
}
