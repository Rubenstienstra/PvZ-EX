using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Database;
using UnityEngine.Events;
using System;

public class FirebaseAchievementInventory : MonoBehaviour//Change Achievements.
{
    public UnityEvent OnResyncAchievements;
    [Header("Variables")]
    public List<User.AchievementInfo> localUserAchievements;
    public int maximumAchievements = 20;

    [Header("Debug")]
    public bool allowDebug = true;
    public List<User.AchievementInfo> onlineUserAchievements;

    [Header("Scripts")]
    public FirebaseManager firebaseManager;
    public FirebaseInventoryManager firebaseInventory;

    private IEnumerator Start()//Waits until Firebase is ready
    {
        yield return new WaitUntil(() => firebaseManager.initializedFirebase == true);



        yield return null;
    }
    public async void AddAchievements()
    {
        await AddAchievementDataAsync();
    }
    public async void ResyncAchievements()
    {
        await ResyncAchievementDataAsync();
    }
    public async void SetAllAchievementsLocked()
    {
        localUserAchievements.Clear();
        for (int i = 0; i < maximumAchievements; i++)
        {
            localUserAchievements.Add(new()
            {
                achievementId = 0,
                progress = 0,
                rarity = 0
            });
        }
        await AddAchievementDataAsync();
    }

    private async Task AddAchievementDataAsync()
    {
        string json = JsonConvert.SerializeObject(localUserAchievements);

        await firebaseInventory.SendingData(json, "Users", firebaseManager.User.DisplayName, "Inventory", "Achievements", null, null);

        if (allowDebug)
        {
            print("AchievementInventory to firebase updated!");
        }
    }
    private async Task ResyncAchievementDataAsync()
    {
        try
        {
            var rootReference = firebaseManager.Database.RootReference.Child("Users");
            await rootReference.Child(firebaseManager.User.DisplayName).Child("Inventory").Child("Achievements").GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot dataSnapshot = task.Result;

                    //Getting Values
                    onlineUserAchievements.Clear();
                    for (int i = 0; i < dataSnapshot.ChildrenCount; i++)
                    {
                        onlineUserAchievements.Add(new()
                        {
                            achievementId = (long)dataSnapshot.Child(i.ToString()).Child("achievementId").Value,
                            progress = (long)dataSnapshot.Child(i.ToString()).Child("progress").Value,
                            rarity = (long)dataSnapshot.Child(i.ToString()).Child("rarity").Value
                        });
                    }

                    //Setting Values
                    localUserAchievements.Clear();
                    for (int i = 0; i < onlineUserAchievements.Count; i++)
                    {
                        localUserAchievements.Add(new()
                        {
                            achievementId = onlineUserAchievements[i].achievementId,
                            progress = onlineUserAchievements[i].progress,
                            rarity = onlineUserAchievements[i].rarity
                        });
                    }

                    OnResyncAchievements.Invoke();
                    if (allowDebug)
                    {
                        Debug.Log("AchievementInventory from firebase updated!");
                    }
                    #region Old Code
                    //for (int i = 0; i < onlineUserAchievements.achievementIds.Count; i++)
                    //{
                    //    if (dataSnapshot.Child("achievementIds").ChildrenCount > localUserAchievements.achievementIds.Count)
                    //    {
                    //        localUserAchievements.achievementIds.Add(onlineUserAchievements.achievementIds[i]);
                    //        localUserAchievements.rarities.Add(onlineUserAchievements.rarities[i]);
                    //    }
                    //    else
                    //    {
                    //        localUserAchievements.achievementIds[i] = onlineUserAchievements.achievementIds[i];
                    //        localUserAchievements.rarities[i] = onlineUserAchievements.rarities[i];
                    //    }
                    //}
                    #endregion
                }
            });
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
    }
}