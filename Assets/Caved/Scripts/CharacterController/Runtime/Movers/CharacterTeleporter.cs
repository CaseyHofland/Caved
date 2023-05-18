#nullable enable
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.Events;

public class CharacterTeleporter : MonoBehaviour
{
    public CharacterTeleporter? teleportTo;
    public UnityAction<KinematicCharacterMotor>? onCharacterTeleport;
    public bool isBeingTeleportedTo { get; set; }

    private void OnTriggerEnter(Collider other)
    {
        if (!isBeingTeleportedTo)
        {
            var motor = other.GetComponent<KinematicCharacterMotor>();
            if (motor && teleportTo != null)
            {
                teleportTo.isBeingTeleportedTo = true;
                motor.SetPositionAndRotation(teleportTo.transform.position, teleportTo.transform.rotation);
                onCharacterTeleport?.Invoke(motor);
            }
        }

        isBeingTeleportedTo = false;
    }
}
