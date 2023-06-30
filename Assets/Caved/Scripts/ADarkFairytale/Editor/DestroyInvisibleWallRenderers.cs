#nullable enable
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InvisibleWallProcessor : IProcessSceneWithReport
{
    public int callbackOrder => default;

    private List<GameObject> gameObjects = new();

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        /*scene.GetRootGameObjects(gameObjects);
        foreach (var gameObject in gameObjects)
        {
            DestroyInvisibleWallRendererInChildren(gameObject.transform);

            static void DestroyInvisibleWallRendererInChildren(Transform transform)
            {
                if (transform.CompareTag("InvisibleWall"))
                {
                    //Object.DestroyImmediate(transform.GetComponent<Renderer>());
                    Debug.Log("Off");
                }
                foreach (Transform child in transform)
                {
                    DestroyInvisibleWallRendererInChildren(child);
                }
            }
        }*/
    }

}
