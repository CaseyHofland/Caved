using FMOD;
using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;
using Input = UnityExtras.InputSystem.Input;

public class BeatStomper : MonoBehaviour
{
    public StudioEventEmitter emitter;
    [field: SerializeField] public int completionCount { get; set; } = 3;
    [field: SerializeField] public int msMargin { get; set; } = 200;
    [field: SerializeField, Range(0f, 1f)] public float audibility { get; set; } = 0.9f;
    [Range(0f, 0.1f)] public float rumblePersistance = 0.075f;

    [Header("Left")]
    public string leftSignature = "Left";
    public Input leftStompInput;
    [Range(0f, 1f)] public float leftLowFrequency = 0.25f;
    [Range(0f, 1f)] public float leftHighFrequency = 0.5f;

    [Header("Right")]
    public string rightSignature = "Right";
    public Input rightStompInput;
    [Range(0f, 1f)] public float rightLowFrequency = 0.5f;
    [Range(0f, 1f)] public float rightHighFrequency = 1f;

    [Header("On Destroy")]
    public StudioEventEmitter completionSound;
    public GameObject reward;
    [Range(0f, 1f)] public float completedLowFrequency = 1f;
    [Range(0f, 1f)] public float completedHighFrequency = 1f;

    public record Marker
    {
        public string name;
        public int position;

        public Marker(string name, int position)
        {
            this.name = name;
            this.position = position;
        }
    }

    public Queue<Marker> markers = new();
    public Queue<Marker> inputs = new();
    private bool loopCompleted;
    private int currentCompletionCount;
    private float currentRumbleTimer;

    private GCHandle userDataHandle;

    #region Unity Events
    private void Awake()
    {
        userDataHandle = GCHandle.Alloc(this);

        StartCoroutine(Coroutine());

        IEnumerator Coroutine()
        {
            while (!emitter.EventDescription.isValid())
            {
                yield return null;
            }

            var userDataPtr = GCHandle.ToIntPtr(userDataHandle);

            emitter.EventDescription.setCallback(OnFmodEvent, EVENT_CALLBACK_TYPE.TIMELINE_MARKER | EVENT_CALLBACK_TYPE.SOUND_STOPPED);
            emitter.EventDescription.setUserData(userDataPtr);

            if (emitter.EventInstance.isValid())
            {
                emitter.EventInstance.setCallback(OnFmodEvent, EVENT_CALLBACK_TYPE.TIMELINE_MARKER | EVENT_CALLBACK_TYPE.SOUND_STOPPED);
                emitter.EventInstance.setUserData(userDataPtr);
            }
        }
    }

    private void OnDestroy()
    {
        userDataHandle.Free();

        if (currentCompletionCount < completionCount)
        {
            foreach (var gamepad in Gamepad.all)
            {
                gamepad.SetMotorSpeeds(0f, 0f);
            }
        }
    }

    private void OnEnable()
    {
        if (leftStompInput.action != null
            && rightStompInput.action != null)
        {
            leftStompInput.action.performed += OnLeftStomp;
            rightStompInput.action.performed += OnRightStomp;
        }
    }

    private void OnDisable()
    {
        if (leftStompInput.action != null
            && rightStompInput.action != null)
        {
            leftStompInput.action.performed -= OnLeftStomp;
            rightStompInput.action.performed -= OnRightStomp;
        }
    }

    private void Update()
    {
        if (currentCompletionCount >= completionCount)
        {
            Complete();
            return;
        }

        if (currentRumbleTimer <= 0f)
        {
            UpdateMotorSpeeds();
        }
        currentRumbleTimer -= Time.deltaTime;

        // Checks if a marker corresponds to an input.
        if (markers.TryPeek(out var marker)
            && inputs.TryPeek(out var input))
        {
            if (input.name != marker.name)
            {
                loopCompleted = false;
                currentCompletionCount = 0;
            }

            markers.Dequeue();
            inputs.Dequeue();
        }

        // Cleans markers, see next comment for more information.
        emitter.EventInstance.getTimelinePosition(out var timelinePosition);
        emitter.EventDescription.getLength(out var length);

        CleanMarkers(markers);
        CleanMarkers(inputs);

        // Removes markers that have been enqueued for longer than the msMargin allows.
        void CleanMarkers(Queue<Marker> markers)
        {
            while (markers.TryPeek(out var marker))
            {
                var positionDelta = timelinePosition - marker.position;
                if (Mathf.Repeat(positionDelta, length) <= msMargin)
                {
                    break;
                }

                markers.Dequeue();
                loopCompleted = false;
                currentCompletionCount = 0;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (emitter)
        {
            Gizmos.color = Color.red;

            var radius = Mathf.Lerp(emitter.OverrideMaxDistance, emitter.OverrideMinDistance, audibility);
            Gizmos.DrawWireSphere(emitter.transform.position, radius);
        }
    }
    #endregion

    #region Custom Methods
    [ContextMenu("Complete")]
    private void Complete()
    {
        foreach (var gamepad in Gamepad.all)
        {
            gamepad.SetMotorSpeeds(completedLowFrequency, completedHighFrequency);
        }

        completionSound.Play();
        ResetMotorSpeeds();

        var player = GameObject.FindWithTag("Player");
        Instantiate(reward, player.transform.position + player.transform.forward * 0.875f, player.transform.rotation);

        Destroy(gameObject);
    }

    private async void ResetMotorSpeeds()
    {
        completionSound.EventDescription.getLength(out var length);
        await Task.Delay(length);

        foreach (var gamepad in Gamepad.all)
        {
            gamepad.SetMotorSpeeds(0f, 0f);
        }
    }

    private void UpdateMotorSpeeds(string signature = null)
    {
        float lowFrequency, highFrequency;
        if (signature == leftSignature)
        {
            lowFrequency = leftLowFrequency;
            highFrequency = leftHighFrequency;
        }
        else if (signature == rightSignature)
        {
            lowFrequency = rightLowFrequency;
            highFrequency = rightHighFrequency;
        }
        else
        {
            lowFrequency = 0f;
            highFrequency = 0f;
        }

        if (!emitter.EventInstance.isValid())
        {
            lowFrequency = 0f;
            highFrequency = 0f;
        }

        emitter.EventInstance.getChannelGroup(out var channelGroup);
        channelGroup.getAudibility(out var audibility);
        if (audibility < this.audibility)
        {
            lowFrequency = 0f;
            highFrequency = 0f;
        }

        foreach (var gamepad in Gamepad.all)
        {
            gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
        }
    }
    #endregion

    #region Input Callbacks
    private void OnLeftStomp(InputAction.CallbackContext context) => OnStomp(context, leftSignature);
    private void OnRightStomp(InputAction.CallbackContext context) => OnStomp(context, rightSignature);

    private void OnStomp(InputAction.CallbackContext context, string signature)
    {
        if (!emitter.EventInstance.isValid())
        {
            return;
        }

        emitter.EventInstance.getChannelGroup(out var channelGroup);
        channelGroup.getAudibility(out var audibility);
        if (audibility < this.audibility)
        {
            return;
        }

        emitter.EventInstance.getTimelinePosition(out var timelinePosition);
        inputs.Enqueue(new(signature, timelinePosition));



        var player = GameObject.FindWithTag("Player");
        var animator = player.GetComponent<Animator>();
        animator.SetTrigger($"{signature}Stomp");
    }
    #endregion

    #region Fmod Callbacks
    [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
    private static RESULT OnFmodEvent(EVENT_CALLBACK_TYPE callbackType, IntPtr instancePtr, IntPtr parameterPtr)
    {
        var instance = new EventInstance(instancePtr);
        if (!instance.TryGetUserData(out BeatStomper beatStomper))
        {
            return RESULT.OK;
        }

        return callbackType switch
        {
            EVENT_CALLBACK_TYPE.TIMELINE_MARKER => OnMarkerPassed(instance, beatStomper, parameterPtr),
            EVENT_CALLBACK_TYPE.SOUND_STOPPED => OnSoundStopped(instance, beatStomper, parameterPtr),
            _ => RESULT.ERR_UNIMPLEMENTED
        };
    }

    [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
    private static RESULT OnMarkerPassed(EventInstance instance, BeatStomper beatStomper, IntPtr parameterPtr)
    {
        var marker = (TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(TIMELINE_MARKER_PROPERTIES));
        beatStomper.markers.Enqueue(new(marker.name, marker.position));

        beatStomper.UpdateMotorSpeeds(marker.name);
        beatStomper.currentRumbleTimer = beatStomper.rumblePersistance;

        return RESULT.OK;
    }

    [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
    private static RESULT OnSoundStopped(EventInstance instance, BeatStomper beatStomper, IntPtr parameterPtr)
    {
        if (beatStomper.loopCompleted)
        {
            beatStomper.currentCompletionCount++;
        }
        beatStomper.loopCompleted = true;

        return RESULT.OK;
    }
    #endregion
}
