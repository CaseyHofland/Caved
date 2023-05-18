using UnityEngine;
using UnityEngine.Sequences;

namespace EasyTransition
{

    public class DemoLoadScene : MonoBehaviour
    {
        public string transitionID;
        public float loadDelay;
        public EasyTransition.TransitionManager transitionManager;

        public void LoadScene(string _sceneName)
        {
            transitionManager.LoadScene(_sceneName, transitionID, loadDelay);
        }

        public void LoadScene(SceneReference sceneReference) => LoadScene(sceneReference);
    }

}

