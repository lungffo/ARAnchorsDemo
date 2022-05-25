using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneSelector : MonoBehaviour
{
    public void OnPlaceAnchorsPressed ()
    {
        SceneManager.LoadScene("PlaceAnchorsExample");
    }

    public void OnDisplayAnchorsPressed ()
    {
        SceneManager.LoadScene("DisplayAnchorsExample");
    }

    public void OnUserGeneratedAnchorsPressed ()
    {
        SceneManager.LoadScene("UserGeneratedAnchorsExample");
    }
}
