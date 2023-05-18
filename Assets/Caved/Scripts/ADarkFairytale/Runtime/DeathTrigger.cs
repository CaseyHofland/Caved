using EasyTransition;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityExtras;

public class DeathTrigger : MonoBehaviour
{
    public Transform resetPosition;
    [Tag] public string collisionTag;

    [Header("Transition")]
    public GameObject transitionPrefab;
    public Material multiplyColorMaterial;
    public Material additiveColorMaterial;
    public TransitionSettings transitionSettings;

    private IEnumerator OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(collisionTag))
        {
            yield break;
        }

        if (transitionPrefab != null)
        {
            var transition = Instantiate(transitionPrefab).GetComponent<Transition>();
            transition.transitionSettings = transitionSettings;
            transition.fullSettings = ScriptableObject.CreateInstance<TransitionManagerSettings>();
            transition.fullSettings.multiplyColorMaterial = multiplyColorMaterial;
            transition.fullSettings.addColorMaterial = additiveColorMaterial;

            float transitionTime = transitionSettings.transitionTime;
            if (transitionSettings.autoAdjustTransitionTime && transitionSettings.transitionSpeed != 0f)
            {
                transitionTime /= transitionSettings.transitionSpeed;
            }

            yield return new WaitForSecondsRealtime(transitionTime);

            // THIS DOES NOT TRIGGER A SCENE LOAD!!! It just triggers the fade out transition.
            // Before you say it: I know, I do not agree with this code either.
            transition.OnSceneLoad(gameObject.scene, LoadSceneMode.Single);
        }

        other.transform.SetPositionAndRotation(resetPosition.position, resetPosition.rotation);
    }
}
