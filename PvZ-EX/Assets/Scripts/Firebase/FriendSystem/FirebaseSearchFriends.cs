using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class FirebaseSearchFriends : MonoBehaviour
{
    public UnityEvent OnResyncFriends;
    [Header("Variables")]
    public GameObject prefabSearchedUser;
    public Transform transformToInstantiate;
    private GameObject searchedUser;

    [Header("Local")]
    public User.CompleteProfile localCompleteUserProfile = new();

    [Header("Online")]
    public User.CompleteProfile onlineCompleteUserProfile = new();
    public User.UserIDs onlineUserID = new();

    [Header("Debug")]
    public bool allowDebug;

    [Header("Scripts")]
    public FirebaseManager firebaseManager;
    public AllItemsDatabase allItemsDatabase;
    public FirebaseProfileInventory profileInventory;
    public FirebaseFriendsList firebaseFriendsList;
    public FirebaseInOutFriendRequests firebaseInOutFriendRequests;
    public ChangeFriendsListLength changeFriendsListLength;


    public async void SearchThroughFriends(string nameToSearch)
    {
        nameToSearch = nameToSearch.Trim();

        if (searchedUser != null)//Destroys searchedUser Everytime you search.
        {
            Destroy(searchedUser);
        }

        await SearchTroughFriendsAsync(nameToSearch);
    }

    private async Task SearchTroughFriendsAsync(string nameToSearch)
    {
        bool allowToCreate = new();
        try
        {
            var rootReference = firebaseManager.Database.RootReference.Child("Users");

            await rootReference.GetValueAsync().ContinueWith(async task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot dataSnapshot = task.Result;

                    if (allowDebug)
                    {
                        print(dataSnapshot.GetRawJsonValue());
                    }
                    if (nameToSearch == firebaseManager.User.DisplayName)//1st Check
                    {
                        if (allowDebug)
                        {
                            print("Name is your own Name!");
                        }
                    }
                    else if (dataSnapshot.Child(nameToSearch).Exists)//2nd Check, OLD:dataSnapshot.GetRawJsonValue().Contains("\"name\":\"" + nameToSearch + "\"")
                    {
                        onlineUserID.userId = "";
                        onlineUserID.userName = nameToSearch;
                        onlineUserID.userId = (string)dataSnapshot.Child(nameToSearch).Child("FirebaseSettings").Child("ID").Value;
                        bool newplayer = (bool)dataSnapshot.Child(nameToSearch).Child("FirebaseSettings").Child("Newplayer").Value;

                        if (onlineUserID.userId != "" && !newplayer)//3rd & 4th Check
                        {
                            allowToCreate = true;
                            await GetCompleteProfileData(onlineUserID.userId, onlineUserID.userName);
                        }
                        else
                        {
                            if (allowDebug)
                            {
                                print("Id does not exist!");
                            }
                        }
                    }
                    else
                    {
                        if (allowDebug)
                        {
                            print("Name does not exist!");
                        }
                    }
                }
            });
            if (allowToCreate)
            {
                StartCoroutine(FriendUserTestCompletion());
            }
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
    }

    private IEnumerator FriendUserTestCompletion()
    {
        yield return new WaitUntil(() => onlineCompleteUserProfile.profile.name != "");//Waits for everydata that is transfered
        print("Making: " + onlineCompleteUserProfile.profile.name);

        CreateUserList();
    }

    private void CreateUserList()
    {
        //Deletes first
        if (searchedUser != null)
        {
            Destroy(searchedUser);
        }

        //Making FriendList
        searchedUser = Instantiate(prefabSearchedUser, transformToInstantiate);
        print("Creating User");
        PlayerFriendProfile playerFriendProfile = searchedUser.GetComponent<PlayerFriendProfile>();

        playerFriendProfile.completeProfile = onlineCompleteUserProfile;
        playerFriendProfile.userId = onlineUserID.userId;
        playerFriendProfile.firebaseFriendsList = firebaseFriendsList;
        playerFriendProfile.firebaseSearchFriends = this;
        playerFriendProfile.firebaseInOutFriendRequests = firebaseInOutFriendRequests;
        playerFriendProfile.backgroundImage.sprite = allItemsDatabase.GetBannerSet(null, (int)playerFriendProfile.completeProfile.profile.bannerId).friendListImage;

        playerFriendProfile.revertFriendRequestButton.gameObject.SetActive(false);
        playerFriendProfile.deleteFriendButton.gameObject.SetActive(false);
        playerFriendProfile.declineFriendRequestButton.gameObject.SetActive(false);
        playerFriendProfile.acceptFriendRequestButton.gameObject.SetActive(false);
        //playerFriendProfile.inviteFriendButton.gameObject.SetActive(false);

        playerFriendProfile.sendFriendRequestButton.gameObject.SetActive(true);

        playerFriendProfile.SetInfoIntoUI();

        if (changeFriendsListLength)
        {
            changeFriendsListLength.UpdateViewportLength();
        }
        else
        {
            Debug.LogWarning("Variable: changeFriendsListLength is empty!", this);
        }
        OnResyncFriends.Invoke();
        print("SearchedUser from firebase updated!");
    }

    public async Task GetCompleteProfileData(string playerId, string playerName)
    {
        try
        {
            DatabaseReference rootReference = firebaseManager.Database.RootReference.Child("Users");

            await rootReference.Child(playerName).Child("Inventory").GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    print("Task went wrong");
                }
                if (task.IsCanceled)
                {
                    print("Task is canceled");
                }
                if (task.IsCompleted)
                {
                    DataSnapshot dataSnapshot = task.Result;

                    onlineCompleteUserProfile.profile.name = playerName;
                    onlineCompleteUserProfile.profile.bannerId = (long)dataSnapshot.Child("Profile").Child("bannerId").Value;
                    onlineCompleteUserProfile.profile.summary = (string)dataSnapshot.Child("Profile").Child("summary").Value;

                    //Achievements
                    onlineCompleteUserProfile.profile.achievements.Clear();
                    long amountOfChildren = dataSnapshot.Child("Profile").Child("achievements").ChildrenCount;
                    for (int i = 0; i < amountOfChildren; i++)
                    {
                        onlineCompleteUserProfile.profile.achievements.Add(new()
                        {
                            achievementId = (long)dataSnapshot.Child("Profile").Child("achievements").Child(i.ToString()).Child("achievementId").Value,
                            rarity = (long)dataSnapshot.Child("Profile").Child("achievements").Child(i.ToString()).Child("rarity").Value,
                            progress = (long)dataSnapshot.Child("Profile").Child("achievements").Child(i.ToString()).Child("progress").Value
                        });
                    }

                    //Progression
                    onlineCompleteUserProfile.xp = (long)dataSnapshot.Child("Progression").Child("xp").Value;
                    onlineCompleteUserProfile.level = (long)dataSnapshot.Child("Progression").Child("level").Value;

                    //Character
                    onlineCompleteUserProfile.character.hairId = (long)dataSnapshot.Child("Character").Child("hairId").Value;
                    onlineCompleteUserProfile.character.headId = (long)dataSnapshot.Child("Character").Child("headId").Value;
                    onlineCompleteUserProfile.character.topId = (long)dataSnapshot.Child("Character").Child("topId").Value;
                    onlineCompleteUserProfile.character.bottomId = (long)dataSnapshot.Child("Character").Child("bottomId").Value;
                    onlineCompleteUserProfile.character.shoesId = (long)dataSnapshot.Child("Character").Child("shoesId").Value;
                    onlineCompleteUserProfile.character.accessoryId = (long)dataSnapshot.Child("Character").Child("accessoryId").Value;

                    localCompleteUserProfile = new()
                    {
                        character = onlineCompleteUserProfile.character,
                        level = onlineCompleteUserProfile.level,
                        profile = onlineCompleteUserProfile.profile,
                        xp = onlineCompleteUserProfile.xp,
                    };
                }
            });
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
    }
}