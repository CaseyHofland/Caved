#nullable enable
using UnityEngine;
using UnityEngine.AI;
using UnityExtras;

[AddComponentMenu("Physics/AI Character")]
[RequireComponent(typeof(CharacterMover))]
[RequireComponent(typeof(AIFollow))]
[DisallowMultipleComponent]
public class AICharacter : MonoBehaviour
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [field: SerializeField, HideInInspector] public CharacterMover characterMover { get; private set; }
    [field: SerializeField, HideInInspector] public AIFollow aiFollow { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public static implicit operator CharacterMover(AICharacter aiCharacter) => aiCharacter.characterMover;

    public NavMeshAgent agent => aiFollow.agent;

    [field: SerializeField] public bool sprint { get; set; }

    private void InitializeComponents()
    {
        characterMover = GetComponent<CharacterMover>();
        aiFollow = GetComponent<AIFollow>();
        agent.updatePosition = false;
    }

    private void Reset()
    {
        InitializeComponents();
    }

    private void Awake()
    {
        InitializeComponents();
    }

    private void Update()
    {
        if (!aiFollow.isFollowing)
        {
            return;
        }

        // Move the character towards the steering target.
        var deltaSteeringTarget = (agent.steeringTarget - transform.position).normalized;
        characterMover.Move(deltaSteeringTarget, sprint);
        characterMover.TurnTowards(deltaSteeringTarget);

        // Pull agent towards character
        agent.nextPosition = transform.position;
    }
}
