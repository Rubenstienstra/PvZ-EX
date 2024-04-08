using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirebaseDeleteAccount : MonoBehaviour
{
    public string sceneToGo = "FirebaseDevelopment";

    [Header("Scripts")]
    public FirebaseManager firebaseManager;

    public async void DeleteAccount()
    {
        await DeleteAccountAsync();
    }

    public async Task DeleteAccountAsync()
    {
        try
        {
            DatabaseReference rootReference = firebaseManager.Database.RootReference.Child("Users");

            await rootReference.Child(firebaseManager.User.DisplayName).RemoveValueAsync();// Deletes PlayerData

            await firebaseManager.User.DeleteAsync();// Deletes Player Authentication
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
        SceneManager.LoadScene(sceneToGo);
    }
}
