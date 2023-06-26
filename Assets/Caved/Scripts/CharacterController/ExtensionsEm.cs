using UnityEngine;

public static class ExtensionsEm
{
    public static void CrossFadeEm(this Animator animator, CrossFadeSettingsEm settings)
    {
        animator.CrossFade(
            settings.stateName, 
            settings.transitionDuration, 
            settings.layer, 
            settings.timeOffset);
    }
    public static void CrossFadeInFixedTimeEm(this Animator animator, CrossFadeSettingsEm settings)
    {
        animator.CrossFadeInFixedTime(
            settings.stateName,
            settings.transitionDuration,
            settings.layer,
            settings.timeOffset);
    }
}
