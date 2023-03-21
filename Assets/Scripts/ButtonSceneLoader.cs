using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonSceneLoader : MonoBehaviour
{
    public string sceneToLoad;

    public void OnButtonPress()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
