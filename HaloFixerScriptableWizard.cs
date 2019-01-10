using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class HaloFixerScriptableWizard : UnityEditor.ScriptableWizard {

    static private bool IsValidHaloInstance(GameObject gameObject) {
        if (gameObject.name.StartsWith("Instances:"))
            return true;
        else
            return false;
    }

    static private List<string> GetHaloGameObjectNameHierarchy(GameObject gameObject) {
        List<string> stringArray = new List<string>();

        string gameObjectName = gameObject.name;
        string[] split = gameObjectName.Split(':');

        if (split.Length == 2) {
            string instanceDecleration = split[0];
            string instanceFullObjectName = split[split.Length - 1];

            Match match = Regex.Match(split[split.Length - 1], @"\(([^)]*)\)");
            if (match.Success) {
                string instanceCategory = match.Groups[1].Value;
                string instanceName = instanceFullObjectName.Replace(match.Groups[0].Value, "");

                stringArray.Add(instanceDecleration);
                stringArray.Add(instanceCategory);
                stringArray.Add(instanceName);
            }
        }

        return stringArray;
    }

    private void SetGameObjectParent(GameObject child, GameObject parent) {
        if (parent == null)  {
            child.transform.parent = null;
        } else {
            child.transform.parent = parent.transform;
        }
    }

    private Transform FindGameObjectChildTransform(GameObject root, string gameObjectName) {
        Transform childTransform = null;

        if (root != null) {
            childTransform = root.transform.Find(gameObjectName);
        } else {
            GameObject[] rootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject currentGameObject in rootGameObjects)
            {
                if (currentGameObject.name == gameObjectName)
                {
                    childTransform = currentGameObject.transform;
                    break;
                }
            }
        }

        return childTransform;
    }

    private GameObject GetOrCreateNamedGameObject(string gameObjectName, GameObject gameObjectParent)
    {
        Transform childTransform = null;

        childTransform = FindGameObjectChildTransform(gameObjectParent, gameObjectName);

        if (childTransform) {
            return childTransform.gameObject;
        } else {
            GameObject emptyGameObject = new GameObject(gameObjectName);
            SetGameObjectParent(emptyGameObject, gameObjectParent);

            return emptyGameObject;
        }
    }

    private List<Transform> GetValidTransforms(GameObject[] rootGameObjects) {
        List<Transform> validTransforms = new List<Transform>();

        foreach (GameObject currentGameObject in rootGameObjects) {
            if (IsValidHaloInstance(currentGameObject)) {
                validTransforms.Add(currentGameObject.transform);
            }
        }

        return validTransforms;
    }

    public void FixHaloInstances(GameObject[] rootGameObjects) {
        List<Transform> validTransforms = GetValidTransforms(rootGameObjects);

        foreach (Transform childTransform in validTransforms) {
            GameObject child = childTransform.gameObject;

            List<string> stringArray = GetHaloGameObjectNameHierarchy(child);
            if (stringArray.Count > 1) {
                GameObject currentGameObject = null;
                for(int i = 0; i < stringArray.Count-1; i++) {
                    currentGameObject = GetOrCreateNamedGameObject(stringArray[i], currentGameObject);
                }

                child.name = stringArray[stringArray.Count - 1];
                child.transform.parent = currentGameObject.transform;
            }
        }
    }


    [UnityEditor.MenuItem("Halo Tools/Sort Halo Objects")]
    static void CreateWizard() {
        UnityEditor.ScriptableWizard.DisplayWizard<HaloFixerScriptableWizard>("Sort Halo Objects", "Apply");
    }

    void OnWizardCreate() {
        FixHaloInstances(UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects());
    }

    void OnWizardUpdate()
    {
        
    }
}
