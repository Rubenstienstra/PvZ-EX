using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Unity.VisualScripting;
using Firebase.Database;
using TMPro;
using System.Linq;
using System;

public class FirebaseInventoryManager : MonoBehaviour
{
    [Header("Debug"), Space]
    public bool allowDebug;

    [Header("Scripts")]
    public FirebaseManager firebaseManager;
    public FirebaseCharacterClothingInventory firebaseClothingInventory;
    public FirebaseProgressionInventory firebaseProgressionInventory;
    public FirebaseProfileInventory firebaseProfileInventory;
    public FirebaseAchievementInventory firebaseAchievementInventory;

    private IEnumerator Start()//Waits until Firebase is ready
    {
        yield return new WaitUntil(() => firebaseManager.initializedFirebase);
        
        yield return null;
    }
    #region OldCode
    //public async void CheckForNewPlayer()
    //{
    //    await CheckForNewPlayerAsync();
    //}

    //private async Task CheckForNewPlayerAsync()
    //{
    //    var rootReference = firebaseManager.Database.RootReference.Child("Users").Child(firebaseManager.User.DisplayName).Child("FirebaseSettings").Child("Newplayer");
    //    await rootReference.GetValueAsync().ContinueWith(async task =>
    //    {
    //        if (task.IsCompleted)
    //        {
    //            var dataSnapshot = task.Result;

    //            if ((bool)dataSnapshot.Value)//If player is new player
    //            {
    //                if (allowDebug)
    //                {
    //                    print("Player is New: true");
    //                }
    //                AddingAllInventoryData(true);//True, FirstTime

    //                bool firstTime = false;
    //                var jsonFirstTime = JsonConvert.SerializeObject(firstTime);

    //                await rootReference.SetRawJsonValueAsync(jsonFirstTime);

    //            }
    //            else
    //            {
    //                if (allowDebug)
    //                {
    //                    print("Player is New: false");
    //                }
    //                ResyncAllInventoryData();
    //            }
    //        }
    //    });
    //}
    #endregion

    public void AddingAllInventoryData(bool firstTime)
    {
        firebaseClothingInventory.AddingClothes(firstTime);
        firebaseClothingInventory.AddingCharacter(firstTime);
        firebaseProgressionInventory.AddingProgression(firstTime);
        firebaseProfileInventory.AddingProfile();
        AddAchievements(firstTime);
    }

    public void ResyncAllInventoryData()
    {
        firebaseClothingInventory.ResyncClothes();
        firebaseClothingInventory.ResyncCharacter();
        firebaseProgressionInventory.ResyncProgression();
        firebaseProfileInventory.ResyncProfile();
        ResyncAchievements();
    }

    #region Clothing & Character Discarded
    //public void AddingClothes()
    //{
    //    firebaseClothingInventory.AddingClothes(false);
    //}
    //public void ResyncClothes()
    //{
    //    firebaseClothingInventory.ResyncClothes();
    //}

    //public void AddingCharacter()
    //{
    //    firebaseClothingInventory.AddingCharacter(false);
    //}
    //public void ResyncCharacter()
    //{
    //    firebaseClothingInventory.ResyncCharacter();
    //}
    #endregion

    #region Progression Discarded

    //public void AddingProgression()
    //{
    //    firebaseProgressionInventory.AddingProgression();
    //}

    //public void ResyncProgression()
    //{
    //    firebaseProgressionInventory.ResyncProgression();
    //}

    #endregion

    #region Profile Discarded

    //public void AddingProfile()
    //{
    //    firebaseProfileInventory.AddingProfile();
    //}

    //public void ResyncProfile()
    //{
    //    firebaseProfileInventory.ResyncProfile();
    //}

    #endregion 

    #region Achievements
    public void AddAchievements(bool FirstTime)
    {
        if (FirstTime)
        {
            firebaseAchievementInventory.SetAllAchievementsLocked();
        }
        else
        {
            firebaseAchievementInventory.AddAchievements();
        }
    }

    public void ResyncAchievements()
    {
        firebaseAchievementInventory.ResyncAchievements();
    }

    #endregion

    public async Task SendingData(string json, string child1, string child2 = null, string child3 = null, string child4 = null, string child5 = null, string child6 = null)
    {
        try
        {
            if (child6 != null)
            {
                await firebaseManager.Database.RootReference.Child(child1).Child(child2).Child(child3).Child(child4).Child(child5).Child(child6).SetRawJsonValueAsync(json);
                return;
            }
            if (child5 != null)
            {
                await firebaseManager.Database.RootReference.Child(child1).Child(child2).Child(child3).Child(child4).Child(child5).SetRawJsonValueAsync(json);
                return;
            }
            else if (child4 != null)
            {
                await firebaseManager.Database.RootReference.Child(child1).Child(child2).Child(child3).Child(child4).SetRawJsonValueAsync(json);
                return;
            }
            else if (child3 != null)
            {
                await firebaseManager.Database.RootReference.Child(child1).Child(child2).Child(child3).SetRawJsonValueAsync(json);
                return;
            }
            else if (child2 != null)
            {
                await firebaseManager.Database.RootReference.Child(child1).Child(child2).SetRawJsonValueAsync(json);
                return;
            }
            else if (child1 != null)
            {
                await firebaseManager.Database.RootReference.Child(child1).SetRawJsonValueAsync(json);
                return;
            }
            else if (child1 == null)
            {
                await firebaseManager.Database.RootReference.SetRawJsonValueAsync(json);
                return;
            }
            else
            {
                Debug.LogWarning("SendingData Task has been filled in empty!");
            }
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
    }
}
