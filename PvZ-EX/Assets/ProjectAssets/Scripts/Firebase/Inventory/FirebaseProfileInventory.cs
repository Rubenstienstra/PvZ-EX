using Firebase.Auth;
using Firebase.Database;
using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class FirebaseProfileInventory : MonoBehaviour//Replace & Add Profile.
{
    public UnityEvent OnResyncComplete;
    [Header("Variables")]
    public TMP_Text usernameMainmenuText;
    public TMP_Text usernameProfileText;
    public User.Profile localUserProfile = new();

    [Header("Debug")]
    public bool allowDebug = true;
    public User.Profile onlineUserProfile = new();
    public List<User.CompleteProfile> onlineCompleteUserProfile;

    private List<User.CompleteProfile> cachedCompleteProfileData = new List<User.CompleteProfile>();//Used for learning

    [Header("Scripts")]
    public FirebaseManager firebaseManager;
    public FirebaseInventoryManager firebaseInventory;

    private IEnumerator Start()//Waits until Firebase is ready
    {
        yield return new WaitUntil(() => firebaseManager.initializedFirebase == true);

        SetProfileUI();

        yield return null;
    }

    public async void AddingProfile()//For Buttons
    {
        await AddingProfileDataAsync();
    }
    public async void ResyncProfile()//For Buttons
    {
        await ResyncProfileDataAsync();
    }

    private async Task AddingProfileDataAsync()
    {
        localUserProfile.name = firebaseManager.User.DisplayName;
        string jsonInventory = JsonConvert.SerializeObject(localUserProfile);

        await firebaseInventory.SendingData(jsonInventory, "Users", firebaseManager.User.DisplayName, "Inventory", "Profile");

        if (allowDebug)
        {
            print("ProfileInventory to firebase updated!");
        }
    }

    private async Task ResyncProfileDataAsync()
    {
        try
        {
            DatabaseReference rootReference = firebaseManager.Database.RootReference.Child("Users");

            await rootReference.Child(firebaseManager.User.DisplayName).Child("Inventory").Child("Profile").GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot dataSnapshot = task.Result;

                    //Getting Values
                    onlineUserProfile.name = (string)dataSnapshot.Child("name").Value;
                    onlineUserProfile.summary = (string)dataSnapshot.Child("summary").Value;
                    onlineUserProfile.bannerId = (long)dataSnapshot.Child("bannerId").Value;
                    for (int i = 0; i < dataSnapshot.Child("achievements").ChildrenCount; i++)//4 Achievements for profile.
                    {
                        onlineUserProfile.achievements[i].achievementId = (long)dataSnapshot.Child("achievements").Child(i.ToString()).Child("achievementId").Value;
                        onlineUserProfile.achievements[i].progress = (long)dataSnapshot.Child("achievements").Child(i.ToString()).Child("progress").Value;
                        onlineUserProfile.achievements[i].rarity = (long)dataSnapshot.Child("achievements").Child(i.ToString()).Child("rarity").Value;
                    }

                    //Setting Values
                    localUserProfile.name = onlineUserProfile.name;
                    localUserProfile.summary = onlineUserProfile.summary;
                    localUserProfile.bannerId = onlineUserProfile.bannerId;
                    for (int i = 0; i < onlineUserProfile.achievements.Count; i++)
                    {
                        localUserProfile.achievements[i].achievementId = onlineUserProfile.achievements[i].achievementId;
                        localUserProfile.achievements[i].progress = onlineUserProfile.achievements[i].progress;
                        localUserProfile.achievements[i].rarity = onlineUserProfile.achievements[i].rarity;
                    }

                    //Setting UI
                    if (allowDebug)
                    {
                        Debug.Log("ProfileInventory from firebase updated!");
                    }
                }
            });
            usernameMainmenuText.SetText(localUserProfile.name);
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
        OnResyncComplete.Invoke();
    }
    private void SetProfileUI()
    {
        usernameMainmenuText.SetText(firebaseManager.User.DisplayName);
        usernameProfileText.SetText(firebaseManager.User.DisplayName);
    }

    #region Complete Inventory
    #region OldCode
    //public async void UseTheMethodWeJustCreated()
    //{
    //    var tempExampleList = new List<User.UserIDs>();
    //    var listOfCompeleteProfile = await GetAllCompleteProfileInventoryDataConcurrently(tempExampleList);
    //}

    // Run tasks after each other so less efficient and less quick
    //public async Task<List<User.CompleteProfile>> GetAllCompleteProfileInventoryDataLinearly(User.UserIDs[] userIDs)
    //{
    //    List<User.CompleteProfile> tempList = new List<User.CompleteProfile>();

    //    for (int i = 0; i < userIDs.Length; i++)
    //    {
    //        var profile = await GetCompleteUserProfile(userIDs[i].userName);
    //        tempList.Add(profile);
    //    }

    //    return tempList;
    //}
    #endregion

    // Run all tasks at the same time for quicker results (multi-threading)
    //public List<User.CompleteProfile> GetAllCompleteProfileInventoryData(List<User.UserIDs> userIDs)
    //{
    //    for (int i = 0; i < userIDs.Count; i++)
    //    {
    //        onlineCompleteUserProfile.Add(new User.CompleteProfile { });
    //        CompleteProfileDataAsync(userIDs[i].userId, userIDs[i].userName, i);
    //    }
    //    return onlineCompleteUserProfile;
    //}

    public async Task<List<User.CompleteProfile>> GetAllCompleteProfileInventoryDataConcurrently(List<User.UserIDs> userIDs)
    {
        var completeProfileList = new List<User.CompleteProfile>();
        var completeProfileTaskList = new List<Task<User.CompleteProfile>>();
        try
        {
            for (int i = 0; i < userIDs.Count; i++)
            {
                // Caching Profile Data
                var profileData = cachedCompleteProfileData.FirstOrDefault(completeProfile => completeProfile.profile.name == userIDs[i].userName);
                if (profileData != null)
                {
                    completeProfileList.Add(profileData);
                }
                else
                {

                }
                var getProfileTask = GetCompleteUserProfile(userIDs[i].userName);
                completeProfileTaskList.Add(getProfileTask);
            }
            await Task.WhenAll(completeProfileTaskList);

            foreach (var completedTask in completeProfileTaskList)
            {
                completeProfileList.Add(completedTask.Result);
                cachedCompleteProfileData.Add(completedTask.Result);
            }
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
        return completeProfileList;
    }

    private async Task<User.CompleteProfile> GetCompleteUserProfile(string userName)
    {
        try
        {
            DatabaseReference rootReference = firebaseManager.Database.RootReference.Child("Users");

            User.CompleteProfile newUser = new User.CompleteProfile();

            DataSnapshot dataSnaphot = await rootReference.Child(userName).Child("Inventory").GetValueAsync();

            //Profile
            newUser.profile.name = userName;
            newUser.profile.bannerId = (long)dataSnaphot.Child("Profile").Child("bannerId").Value;
            newUser.profile.summary = (string)dataSnaphot.Child("Profile").Child("summary").Value;

            //Achievements
            newUser.profile.achievements.Clear();
            long amountOfChildren = dataSnaphot.Child("Profile").Child("achievements").ChildrenCount;
            for (int i = 0; i < amountOfChildren; i++)
            {
                newUser.profile.achievements.Add(new()
                {
                    achievementId = (long)dataSnaphot.Child("Profile").Child("achievements").Child(i.ToString()).Child("achievementId").Value,
                    progress = (long)dataSnaphot.Child("Profile").Child("achievements").Child(i.ToString()).Child("progress").Value,
                    rarity = (long)dataSnaphot.Child("Profile").Child("achievements").Child(i.ToString()).Child("rarity").Value
                });
            }

            //Progression
            newUser.xp = (long)dataSnaphot.Child("Progression").Child("xp").Value;
            newUser.level = (long)dataSnaphot.Child("Progression").Child("level").Value;

            //Character
            newUser.character.hairId = (long)dataSnaphot.Child("Character").Child("hairId").Value;
            newUser.character.headId = (long)dataSnaphot.Child("Character").Child("headId").Value;
            newUser.character.topId = (long)dataSnaphot.Child("Character").Child("topId").Value;
            newUser.character.bottomId = (long)dataSnaphot.Child("Character").Child("bottomId").Value;
            newUser.character.accessoryId = (long)dataSnaphot.Child("Character").Child("accessoryId").Value;
            newUser.character.shoesId = (long)dataSnaphot.Child("Character").Child("shoesId").Value;

            return newUser;
        }
        catch(Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        return null;
    }
    #region OldCode
    private async void CompleteProfileDataAsync(string userID, string userName, int crUserID)
    {
        await GetAllCompleteProfileDataAsync(userID, userName, crUserID);
    }

    private async Task GetAllCompleteProfileDataAsync(string userID, string userName, int crUserID)
    {
        DatabaseReference rootReference = firebaseManager.Database.RootReference.Child("Users");

        await rootReference.Child(userName).Child("Inventory").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot dataSnaphot = task.Result;


                //Profile
                onlineCompleteUserProfile[crUserID].profile.name = userName;
                onlineCompleteUserProfile[crUserID].profile.bannerId = (long)dataSnaphot.Child("Profile").Child("bannerId").Value;
                onlineCompleteUserProfile[crUserID].profile.summary = (string)dataSnaphot.Child("Profile").Child("summary").Value;

                //Achievements
                onlineCompleteUserProfile[crUserID].profile.achievements.Clear();
                long amountOfChildren = dataSnaphot.Child("Profile").Child("achievements").ChildrenCount;
                for (int i = 0; i < amountOfChildren; i++)
                {
                    onlineCompleteUserProfile[crUserID].profile.achievements.Add(new()
                    {
                        achievementId = (long)dataSnaphot.Child("Profile").Child("achievements").Child(i.ToString()).Child("achievementId").Value,
                        progress = (long)dataSnaphot.Child("Profile").Child("achievements").Child(i.ToString()).Child("progress").Value,
                        rarity = (long)dataSnaphot.Child("Profile").Child("achievements").Child(i.ToString()).Child("rarity").Value
                    });
                }

                //Progression
                onlineCompleteUserProfile[crUserID].xp = (long)dataSnaphot.Child("Progression").Child("xp").Value;
                onlineCompleteUserProfile[crUserID].level = (long)dataSnaphot.Child("Progression").Child("level").Value;

                //Character
                onlineCompleteUserProfile[crUserID].character.hairId = (long)dataSnaphot.Child("Character").Child("hairId").Value;
                onlineCompleteUserProfile[crUserID].character.headId = (long)dataSnaphot.Child("Character").Child("headId").Value;
                onlineCompleteUserProfile[crUserID].character.topId = (long)dataSnaphot.Child("Character").Child("topId").Value;
                onlineCompleteUserProfile[crUserID].character.bottomId = (long)dataSnaphot.Child("Character").Child("bottomId").Value;
                onlineCompleteUserProfile[crUserID].character.accessoryId = (long)dataSnaphot.Child("Character").Child("accessoryId").Value;
                onlineCompleteUserProfile[crUserID].character.shoesId = (long)dataSnaphot.Child("Character").Child("shoesId").Value;
            }
        });
    }
    #endregion

    #endregion
}
