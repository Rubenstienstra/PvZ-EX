using Firebase.Database;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class FirebaseFriendsList : MonoBehaviour
{
    [Header("UnityEvents")]
    public UnityEvent OnResyncFriendsList;

    [Header("Variables")]
    private int totalFriends;

    [Header("Instantation")]
    public GameObject friendPrefab;
    public Transform transformToInstantiate;
    public List<GameObject> friendPrefabs = new();

    [Header("Local")]
    public List<User.UserIDs> localFriendListIds = new();
    public List<User.CompleteProfile> localFriendListInfo = new();

    [Header("Online")]
    public List<User.UserIDs> onlineFriendListIds = new();
    public List<User.CompleteProfile> onlineFriendListInfo = new();

    [Header("Debug")]
    public bool allowDebug = true;
    public bool initializedFriendsList;

    [Header("Scripts")]
    public FirebaseManager firebaseManager;
    public AllItemsDatabase allItemsDatabase;
    public FirebaseInventoryManager firebaseInventory;
    public FirebaseProfileInventory firebaseProfileInventory;
    public FirebaseSearchFriends firebaseSearchFriends;
    public FirebaseInOutFriendRequests firebaseInOutFriendRequests;
    public ChangeFriendsListLength changeFriendsListLength;

    private IEnumerator Start()//Waits until Firebase is ready, Don't have to add friends on new game sinds u don't have any friends on start.
    {
        yield return new WaitUntil(() => firebaseManager.initializedFirebase == true);

        ResyncFriendList();

        yield return null;
    }

    public async void AddFriendList()
    {
        await AddFriendListAsync();
    }
    public async void ResyncFriendList()
    {
        await ResyncFriendListAsync();
    }

    private async Task AddFriendListAsync()
    {
        var jsonClothes = JsonConvert.SerializeObject(localFriendListIds);

        await firebaseInventory.SendingData(jsonClothes, "Users", firebaseManager.User.DisplayName, "FriendList", "Friends");

        if (allowDebug)
        {
            print("FriendList to firebase updated!");
        }
    }
    private async Task ResyncFriendListAsync()
    {
        totalFriends = 0;
        try
        {
            DatabaseReference rootReference = firebaseManager.Database.RootReference.Child("Users").Child(firebaseManager.User.DisplayName).Child("FriendList").Child("Friends");

            await rootReference.GetValueAsync().ContinueWith(async task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot dataSnapshot = task.Result;

                    if (!dataSnapshot.HasChildren)
                    {
                        print("FriendsList from firebase updated! Player has no friends!");
                        return;
                    }
                    totalFriends = (int)dataSnapshot.ChildrenCount;

                    localFriendListIds.Clear();
                    onlineFriendListIds.Clear();
                    localFriendListInfo.Clear();
                    onlineFriendListInfo.Clear();
                    for (int i = 0; i < totalFriends; i++)
                    {
                        User.UserIDs userIDs = new();

                        //Getting Data
                        userIDs.userName = (string)dataSnapshot.Child(i.ToString()).Child("userName").Value;
                        userIDs.userId = (string)dataSnapshot.Child(i.ToString()).Child("userId").Value;

                        //Setting ID Data
                        localFriendListIds.Add(userIDs);
                        onlineFriendListIds.Add(userIDs);
                        localFriendListInfo.Add(new());

                        await GetCompleteProfileData(onlineFriendListIds[i].userId, onlineFriendListIds[i].userName, i);
                    }
                }
            });
            if (totalFriends > 0)
            {
                StartCoroutine(FriendUserTestCompletion(totalFriends));
            }
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
    }

    public async Task GetCompleteProfileData(string playerId, string playerName, int loopNumber)
    {
        try
        {
            DatabaseReference rootReference = firebaseManager.Database.RootReference.Child("Users");

            await rootReference.Child(playerName).Child("Inventory").GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot dataSnapshot = task.Result;

                    localFriendListInfo[loopNumber].profile.name = playerName;
                    localFriendListInfo[loopNumber].profile.bannerId = (long)dataSnapshot.Child("Profile").Child("bannerId").Value;
                    localFriendListInfo[loopNumber].profile.summary = (string)dataSnapshot.Child("Profile").Child("summary").Value;

                    //Achievements
                    localFriendListInfo[loopNumber].profile.achievements.Clear();

                    // User.CompleteProfile userProfile = JsonConvert.DeserializeObject<User.Profile>(dataSnapshot.GetRawJsonValue());

                    long amountOfChildren = dataSnapshot.Child("Profile").Child("achievements").ChildrenCount;
                    for (int i = 0; i < (int)amountOfChildren; i++)
                    {
                        localFriendListInfo[loopNumber].profile.achievements.Add(new()
                        {
                            achievementId = (long)dataSnapshot.Child("Profile").Child("achievements").Child(i.ToString()).Child("achievementId").Value,
                            rarity = (long)dataSnapshot.Child("Profile").Child("achievements").Child(i.ToString()).Child("rarity").Value,
                            progress = (long)dataSnapshot.Child("Profile").Child("achievements").Child(i.ToString()).Child("progress").Value
                        });
                    }

                    //Progression
                    localFriendListInfo[loopNumber].xp = (long)dataSnapshot.Child("Progression").Child("xp").Value;
                    localFriendListInfo[loopNumber].level = (long)dataSnapshot.Child("Progression").Child("level").Value;

                    //Character
                    localFriendListInfo[loopNumber].character.hairId = (long)dataSnapshot.Child("Character").Child("hairId").Value;
                    localFriendListInfo[loopNumber].character.headId = (long)dataSnapshot.Child("Character").Child("headId").Value;
                    localFriendListInfo[loopNumber].character.topId = (long)dataSnapshot.Child("Character").Child("topId").Value;
                    localFriendListInfo[loopNumber].character.bottomId = (long)dataSnapshot.Child("Character").Child("bottomId").Value;
                    localFriendListInfo[loopNumber].character.shoesId = (long)dataSnapshot.Child("Character").Child("shoesId").Value;
                    localFriendListInfo[loopNumber].character.accessoryId = (long)dataSnapshot.Child("Character").Child("accessoryId").Value;

                    onlineFriendListInfo.Add(localFriendListInfo[loopNumber]);
                }
            });
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
    }

    #region FriendList
    private IEnumerator FriendUserTestCompletion(int totalFriends)
    {
        yield return new WaitUntil(() => onlineFriendListInfo.Count == totalFriends);
        for (int i = 0; i < totalFriends; i++)
        {
            yield return new WaitUntil(() => onlineFriendListInfo[i].profile.name != "");//Waits for everydata that is transfered
            if (allowDebug)
            {
                print("Making: " + onlineFriendListInfo[i].profile.name);
            }
        }
        CreateFriendList(totalFriends);
    }

    private void CreateFriendList(int totalFriends)
    {
        //Deletes first
        int crFriendsInt = friendPrefabs.Count;
        for (int i = 0; i < crFriendsInt; i++)
        {
            Destroy(friendPrefabs[i]);
        }
        friendPrefabs.Clear();

        //Making FriendList
        for (int i = 0; i < totalFriends; i++)
        {
            GameObject crFriendPrefab = Instantiate(friendPrefab, transformToInstantiate);
            friendPrefabs.Add(crFriendPrefab);

            PlayerFriendProfile playerFriendProfile = crFriendPrefab.GetComponent<PlayerFriendProfile>();
            playerFriendProfile.completeProfile = onlineFriendListInfo[i];
            playerFriendProfile.userId = onlineFriendListIds[i].userId;
            playerFriendProfile.firebaseFriendsList = this;
            playerFriendProfile.firebaseSearchFriends = firebaseSearchFriends;
            playerFriendProfile.firebaseInOutFriendRequests = firebaseInOutFriendRequests;
            playerFriendProfile.backgroundImage.sprite = allItemsDatabase.GetBannerSet(null, (int)playerFriendProfile.completeProfile.profile.bannerId).friendListImage;

            playerFriendProfile.revertFriendRequestButton.gameObject.SetActive(false);
            playerFriendProfile.sendFriendRequestButton.gameObject.SetActive(false);
            playerFriendProfile.declineFriendRequestButton.gameObject.SetActive(false);
            playerFriendProfile.acceptFriendRequestButton.gameObject.SetActive(false);

            playerFriendProfile.SetInfoIntoUI();
        }
        if (changeFriendsListLength)
        {
            changeFriendsListLength.UpdateViewportLength();
        }
        else
        {
            Debug.LogWarning("Variable: changeFriendsListLength is empty!", this);
        }

        initializedFriendsList = true;
        OnResyncFriendsList.Invoke();
        print("FriendsList from firebase updated!");
    }

    public async Task DeleteFriend(User.UserIDs userID, User.CompleteProfile completeProfile, GameObject friendGameObject)
    {
        try
        {
            if (!friendPrefabs.Contains(friendGameObject))
            {
                if (allowDebug)
                {
                    print("User: " + completeProfile.profile.name + ", is not in FriendList!");
                }
                return;
            }

            int listNumberToRemove = new();

            //Other
            var rootReference = firebaseManager.Database.RootReference.Child("Users").Child(completeProfile.profile.name).Child("FriendList").Child("Friends");

            await rootReference.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot dataSnapshot = task.Result;
                    int childrenCount = (int)dataSnapshot.ChildrenCount;
                    for (int i = 0; i < childrenCount; i++)
                    {
                        string crUserName = (string)dataSnapshot.Child(i.ToString()).Child("userName").Value;
                        string crUserId = (string)dataSnapshot.Child(i.ToString()).Child("userId").Value;
                        if (crUserName == userID.userName && crUserId == userID.userId)
                        {
                            listNumberToRemove = i;
                        }
                    }
                }
            });
            await rootReference.Child(listNumberToRemove.ToString()).RemoveValueAsync();

            //Yourself
            for (int i = 0; i < localFriendListIds.Count; i++)
            {
                if (localFriendListIds[i].userName == userID.userName && localFriendListIds[i].userId == userID.userId)
                {
                    listNumberToRemove = i;
                }
            }
            await firebaseManager.Database.RootReference.Child("Users").Child(firebaseManager.User.DisplayName).Child("FriendList").Child("Friends")
            .Child(listNumberToRemove.ToString()).RemoveValueAsync();



            friendPrefabs.Remove(friendGameObject);
            friendGameObject.Destroy();

            localFriendListIds.Remove(userID);
            onlineFriendListIds.Remove(userID);
            localFriendListInfo.Remove(completeProfile);
            onlineFriendListInfo.Remove(completeProfile);
            totalFriends--;



            StartCoroutine(FriendUserTestCompletion(totalFriends));
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
    }
    #endregion

}
