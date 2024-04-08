using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LogoutToLogin : MonoBehaviour
{
    public UnityEvent SuccesfullyLoggedOut;

    [Header("Variables")]
    public bool goToScene = true;
    public string sceneName;

    [Header("Debug")]
    public bool allowDebug = true;
    public bool dontWaitForLogout;

    [Header("Scripts")]
    public FirebaseManager firebaseManager;

    public void Logout()
    {
        StartCoroutine(LogoutAsync());
    }

    public IEnumerator LogoutAsync()
    {
        yield return new WaitUntil(() => firebaseManager.initializedFirebase);

        firebaseManager.Auth.SignOut();
        if (allowDebug)
        {
            print("Is Logging Out...");
        }

        if (!dontWaitForLogout)
        {
            yield return new WaitUntil(() => firebaseManager.Auth == null || firebaseManager.User == null);
        }
        if (allowDebug)
        {
            print("Succesfully logged out");
        }

        SuccesfullyLoggedOut.Invoke();
        if (goToScene)
        {
            SceneManager.LoadScene(sceneName);
        }
        yield return null;
    }

    
}
