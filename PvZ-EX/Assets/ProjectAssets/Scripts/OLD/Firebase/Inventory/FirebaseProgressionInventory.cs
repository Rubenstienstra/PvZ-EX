using Code.Scripts.Firebase.Player;
using Firebase.Database;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class FirebaseProgressionInventory : MonoBehaviour//Replace Progression.
{
    //public UnityEvent OnResyncProgression;

    [Header("Variables")]
    public TMP_Text levelUI;
    public TMP_Text moneyUI;
    public User.Progression localUserProgression;

    [Header("Debug")]
    public bool allowDebug = true;
    public bool resyncedProgression;
    public User.Progression onlineUserProgression;

    [Header("Scripts")]
    public FirebaseInventoryManager firebaseInventory;
    public FirebaseManager firebaseManager;
    public PlayerProgression playerProgression;

    private IEnumerator Start()//Waits until Firebase is ready
    {
        yield return new WaitUntil(() => firebaseManager.initializedFirebase);

        yield return null;
    }
    public async void AddingProgression(bool firstTime)//For Buttons
    {
        if (firstTime)
        {
            localUserProgression.currency = 200;
        }
        await AddingProgressionDataAsync();
    }
    public async void ResyncProgression()//For Buttons
    {
        resyncedProgression = false;
        await ResyncProgressionDataAsync();
        StartCoroutine(playerProgression.AttemptLevelUp());
    }

    private async Task AddingProgressionDataAsync()
    {
        if(localUserProgression.xpRequired == 0)
        {
            playerProgression.experienceRequired = playerProgression.startingExperienceRequired;
            localUserProgression.xpRequired = playerProgression.startingExperienceRequired;
        }
        string jsonInventory = JsonConvert.SerializeObject(localUserProgression);

        try
        {
            await firebaseInventory.SendingData(jsonInventory, "Users", firebaseManager.User.DisplayName, "Inventory", "Progression", null, null);
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }

        resyncedProgression = true;
        if (allowDebug)
        {
            print("ProgressionInventory to firebase updated!");
        }
    }
    private async Task ResyncProgressionDataAsync()
    {
        try
        {
            DatabaseReference rootReference = firebaseManager.Database.RootReference.Child("Users");

            await rootReference.Child(firebaseManager.User.DisplayName).Child("Inventory").Child("Progression").GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot dataSnapshot = task.Result;

                    //Getting Values
                    onlineUserProgression.xp = (long)dataSnapshot.Child("xp").Value;
                    onlineUserProgression.currency = (long)dataSnapshot.Child("currency").Value;
                    onlineUserProgression.level = (long)dataSnapshot.Child("level").Value;
                    onlineUserProgression.xpRequired = (long)dataSnapshot.Child("xpRequired").Value;

                    //Setting Values
                    localUserProgression.xp = onlineUserProgression.xp;
                    localUserProgression.currency = onlineUserProgression.currency;
                    localUserProgression.level = onlineUserProgression.level;
                    localUserProgression.xpRequired = onlineUserProgression.xpRequired;

                    resyncedProgression = true;
                    if (allowDebug)
                    {
                        print("ProgressionInventory from firebase updated!");
                    }
                }
            });
            levelUI.SetText("Level: " + localUserProgression.level);
            moneyUI.SetText("Money: " + localUserProgression.currency);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
    }


}
