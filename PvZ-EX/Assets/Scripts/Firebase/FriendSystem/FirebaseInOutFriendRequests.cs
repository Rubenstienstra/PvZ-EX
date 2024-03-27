using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class FirebaseInOutFriendRequests : MonoBehaviour
{
    public UnityEvent OnResyncInOutRequestList;

    [Header("InRequests")]
    public List<User.UserIDs> inLocalUserIDs;
    public List<User.UserIDs> inOnlineUserIDs;

    [Header("OutRequests")]
    public List<User.UserIDs> outLocalUserIDs;
    public List<User.UserIDs> outOnlineUserIDs;

    [Header("OtherUser Requests")]
    public List<User.UserIDs> inOtherUserIDsFriends = new();
    public List<User.UserIDs> outOtherUserIDsFriends = new();

    [Header("InOutList")]
    public GameObject requestPrefab;

    public Transform inRequestsParent;
    public Transform outRequestsParent;
    
    public List<GameObject> inGameObjectsRequests;
    public List<GameObject> outGameObjectsRequests;

    [Header("Debug")]
    public bool allowDebug = true;

    [Header("Scripts")]
    public FirebaseManager firebaseManager;
    public AllItemsDatabase allItemsDatabase;
    public FirebaseFriendsList firebaseFriendsList;
    public FirebaseSearchFriends firebaseSearchFriends;
    public ChangeFriendsListLength changeFriendsListLength;

    private IEnumerator Start()//Waits until Firebase is ready
    {
        yield return new WaitUntil(() => firebaseManager.initializedFirebase && firebaseFriendsList.initializedFriendsList);

        MakeRequestList();

        yield return null;
    }

    public async void GetInRequests()
    {
        await GetInRequestsAsync();
    }
    public async void GetOutRequests()
    {
        await GetOutRequestsAsync();
    }
    public async void AcceptFriendRequest(User.UserIDs playerUserIDs)
    {
        await AcceptFriendRequestAsync(playerUserIDs);
    }
    public async void DeclineFriendRequest(User.UserIDs userIDsOut, User.UserIDs userIDsIn)
    {
        await DeclineFriendRequestAsync(userIDsOut, userIDsIn, false);
    }
    public async void RevertFriendRequest(User.UserIDs userIDsOut, User.UserIDs userIDsIn)
    {
        await DeclineFriendRequestAsync(userIDsIn, userIDsOut, true);
    }
    public async void SendFriendRequest(User.UserIDs playerUserIDs)
    {
        if (outLocalUserIDs.Contains(playerUserIDs))
        {
            Debug.LogWarning("Already sended request towards: " + playerUserIDs.userName);
            return;
        }
        if(inLocalUserIDs.Contains(playerUserIDs) || firebaseFriendsList.localFriendListIds.Contains(playerUserIDs))
        {
            Debug.LogWarning("Already received request from: " + playerUserIDs.userName);
            return;
        }

        await GetInRequestsAsync();
        await GetOutRequestsAsync();

        if (outLocalUserIDs.Contains(playerUserIDs) || inLocalUserIDs.Contains(playerUserIDs) || firebaseFriendsList.localFriendListIds.Contains(playerUserIDs))
        {
            Debug.LogWarning("Already sended request towards: " + playerUserIDs.userName);
            return;
        }
        outLocalUserIDs.Add(playerUserIDs);

        await SendFriendRequestAsync(playerUserIDs);
    }
    public void MakeRequestList()
    {
        GetInRequests();
        GetOutRequests();
        ShowInOutGoingRequestList();
    }

    private async Task SendFriendRequestAsync(User.UserIDs playerUserIDs)
    {
        try
        {
            inOtherUserIDsFriends.Clear();

            var rootReference = firebaseManager.Database.RootReference.Child("Users").Child(playerUserIDs.userName).Child("FriendList").Child("Ingoing");

            await rootReference.GetValueAsync().ContinueWith(task =>
            {
                DataSnapshot dataSnapshot = task.Result;

                int childrenCount = (int)dataSnapshot.ChildrenCount;
                for (int i = 0; i < childrenCount; i++)
                {
                    inOtherUserIDsFriends.Add(new()
                    {
                        userName = (string)dataSnapshot.Child(i.ToString()).Child("userName").Value,
                        userId = (string)dataSnapshot.Child(i.ToString()).Child("userId").Value
                    });
                }
                inOtherUserIDsFriends.Add(new()
                {
                    userName = firebaseManager.User.DisplayName,
                    userId = firebaseManager.User.UserId
                });

            });

            string thisUserjson = JsonConvert.SerializeObject(inOtherUserIDsFriends);
            string toFriendUserjson = JsonConvert.SerializeObject(outLocalUserIDs);
            await firebaseManager.Database.RootReference.Child("Users").Child(playerUserIDs.userName).Child("FriendList").Child("InGoing").SetRawJsonValueAsync(thisUserjson);
            await firebaseManager.Database.RootReference.Child("Users").Child(firebaseManager.User.DisplayName).Child("FriendList").Child("OutGoing").SetRawJsonValueAsync(toFriendUserjson);

            if (allowDebug)
            {
                print("SendedFriendRequest to firebase updated!");
            }
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
    }

    private async Task AcceptFriendRequestAsync(User.UserIDs playerUserIDs)
    {
        try
        {
            outOtherUserIDsFriends.Clear();

            var rootReference = firebaseManager.Database.RootReference.Child("Users").Child(playerUserIDs.userName).Child("FriendList").Child("Friends");

            await rootReference.GetValueAsync().ContinueWith(task =>
            {
                DataSnapshot dataSnapshot = task.Result;

                int childrenCount = (int)dataSnapshot.ChildrenCount;
                for (int i = 0; i < childrenCount; i++)
                {
                    outOtherUserIDsFriends.Add(new()
                    {
                        userName = (string)dataSnapshot.Child(i.ToString()).Child("userName").Value,
                        userId = (string)dataSnapshot.Child(i.ToString()).Child("userId").Value
                    });
                }
                outOtherUserIDsFriends.Add(new()
                {
                    userName = firebaseManager.User.DisplayName,
                    userId = firebaseManager.User.UserId
                });

            });

            string thisUserjson = JsonConvert.SerializeObject(outOtherUserIDsFriends);
            string toRecieveFriendUserjson = JsonConvert.SerializeObject(inLocalUserIDs);
            await firebaseManager.Database.RootReference.Child("Users").Child(playerUserIDs.userName).Child("FriendList").Child("Friends").SetRawJsonValueAsync(thisUserjson);
            await firebaseManager.Database.RootReference.Child("Users").Child(firebaseManager.User.DisplayName).Child("FriendList").Child("Friends").SetRawJsonValueAsync(toRecieveFriendUserjson);

            await DeclineFriendRequestAsync(playerUserIDs, new User.UserIDs { userName = firebaseManager.User.DisplayName, userId = firebaseManager.User.UserId }, false);
            firebaseFriendsList.ResyncFriendList();
            if (allowDebug)
            {
                print("AcceptedFriendRequest to firebase updated!");
            }
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
    }

    private async Task DeclineFriendRequestAsync(User.UserIDs userIDsOut, User.UserIDs userIDsIn, bool revertFriendRequest)
    {
        try
        {
            //Other person
            int crChildrenCount = new();
            int inRequestToRemoveListInt = new();
            int outRequestToRemoveListInt = new();

            var rootReference = firebaseManager.Database.RootReference.Child("Users").Child(userIDsOut.userName).Child("FriendList").Child("OutGoing");
            await rootReference.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot dataSnapshot = task.Result;
                    crChildrenCount = (int)dataSnapshot.ChildrenCount;
                    for (int i = 0; i < crChildrenCount; i++)
                    {
                        string crUserName = (string)dataSnapshot.Child(i.ToString()).Child("userName").Value;
                        string crUserId = (string)dataSnapshot.Child(i.ToString()).Child("userId").Value;
                        if (crUserName == userIDsOut.userName)
                        {
                            outRequestToRemoveListInt = i;
                        }
                    }
                }
            });
            await rootReference.Child(outRequestToRemoveListInt.ToString()).RemoveValueAsync().ContinueWith(task => { });

            //Yourself

            rootReference = firebaseManager.Database.RootReference.Child("Users").Child(userIDsIn.userName).Child("FriendList").Child("InGoing");

            await rootReference.GetValueAsync().ContinueWith(task =>
            {
                DataSnapshot dataSnapshot = task.Result;
                crChildrenCount = (int)dataSnapshot.ChildrenCount;
                for (int i = 0; i < crChildrenCount; i++)
                {
                    string crUserName = (string)dataSnapshot.Child(i.ToString()).Child("userName").Value;
                    string crUserId = (string)dataSnapshot.Child(i.ToString()).Child("userId").Value;
                    if (crUserName == userIDsIn.userName)
                    {
                        inRequestToRemoveListInt = i;
                    }
                }
            });
            await rootReference.Child(inRequestToRemoveListInt.ToString()).RemoveValueAsync().ContinueWith(task => { });

            if (!revertFriendRequest)
            {
                //In & Out Swapped
                inLocalUserIDs.Remove(userIDsOut);
                inOnlineUserIDs.Remove(userIDsOut);

                Destroy(inGameObjectsRequests[inRequestToRemoveListInt]);
                inGameObjectsRequests.RemoveAt(inRequestToRemoveListInt);

                inOtherUserIDsFriends.Remove(userIDsIn);

                if (allowDebug)
                {
                    print("DeclinedFriendRequest to firebase updated!");
                }
            }
            else
            {
                //In & Out Swapped
                outLocalUserIDs.Remove(userIDsIn);
                outOnlineUserIDs.Remove(userIDsIn);

                Destroy(outGameObjectsRequests[outRequestToRemoveListInt]);
                outGameObjectsRequests.RemoveAt(outRequestToRemoveListInt);

                inOtherUserIDsFriends.Remove(userIDsOut);

                if (allowDebug)
                {
                    print("RevertFriendRequest to firebase updated!");
                }
            }
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
    }

    private async Task GetInRequestsAsync()
    {
        try
        {
            var rootReference = firebaseManager.Database.RootReference.Child("Users").Child(firebaseManager.User.DisplayName).Child("FriendList").Child("InGoing");

            await rootReference.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot dataSnapshot = task.Result;

                    inOnlineUserIDs.Clear();
                    inLocalUserIDs.Clear();
                    for (int i = 0; i < dataSnapshot.ChildrenCount; i++)
                    {
                        string crUserName = (string)dataSnapshot.Child(i.ToString()).Child("userName").Value;
                        string crUserId = (string)dataSnapshot.Child(i.ToString()).Child("userId").Value;

                        inOnlineUserIDs.Add(new()
                        {
                            userName = crUserName,
                            userId = crUserId
                        });
                        inLocalUserIDs.Add(new()
                        {
                            userName = crUserName,
                            userId = crUserId,
                        });
                    }
                }
            });
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
    }
    private async Task GetOutRequestsAsync()
    {
        try
        {
            var rootReference = firebaseManager.Database.RootReference.Child("Users").Child(firebaseManager.User.DisplayName).Child("FriendList").Child("OutGoing");

            await rootReference.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot dataSnapshot = task.Result;

                    outOnlineUserIDs.Clear();
                    outLocalUserIDs.Clear();
                    for (int i = 0; i < dataSnapshot.ChildrenCount; i++)
                    {
                        string crUserName = (string)dataSnapshot.Child(i.ToString()).Child("userName").Value;
                        string crUserId = (string)dataSnapshot.Child(i.ToString()).Child("userId").Value;

                        outOnlineUserIDs.Add(new()
                        {
                            userName = crUserName,
                            userId = crUserId
                        });
                        outLocalUserIDs.Add(new()
                        {
                            userName = crUserName,
                            userId = crUserId,
                        });
                    }
                }
            });
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
    }
    private async void ShowInOutGoingRequestList()
    {
        try
        {
            for (int i = 0; i < inGameObjectsRequests.Count; i++)
            {
                Destroy(inGameObjectsRequests[i]);
            }
            for (int i = 0; i < outGameObjectsRequests.Count; i++)
            {
                Destroy(outGameObjectsRequests[i]);
            }
            inGameObjectsRequests.Clear();
            outGameObjectsRequests.Clear();

            for (int i = 0; i < inLocalUserIDs.Count; i++)
            {
                inGameObjectsRequests.Add(Instantiate(requestPrefab, inRequestsParent));
                PlayerFriendProfile playerFriendProfile = inGameObjectsRequests[i].GetComponent<PlayerFriendProfile>();
                playerFriendProfile.firebaseFriendsList = firebaseFriendsList;
                playerFriendProfile.firebaseInOutFriendRequests = this;
                playerFriendProfile.firebaseSearchFriends = firebaseSearchFriends;
                playerFriendProfile.backgroundImage.sprite = allItemsDatabase.GetBannerSet(null, (int)playerFriendProfile.completeProfile.profile.bannerId).friendListImage;

                playerFriendProfile.revertFriendRequestButton.gameObject.SetActive(false);
                playerFriendProfile.sendFriendRequestButton.gameObject.SetActive(false);
                playerFriendProfile.deleteFriendButton.gameObject.SetActive(false);
                //playerFriendProfile.inviteFriendButton.gameObject.SetActive(false);

                playerFriendProfile.acceptFriendRequestButton.gameObject.SetActive(true);
                playerFriendProfile.declineFriendRequestButton.gameObject.SetActive(true);

                //Not sure if works correctly
                playerFriendProfile.completeProfile.profile.name = inLocalUserIDs[i].userName;
                playerFriendProfile.userId = inLocalUserIDs[i].userId;
                await playerFriendProfile.GetCompleteProfileData();
                playerFriendProfile.SetInfoIntoUI();
            }

            for (int i = 0; i < outLocalUserIDs.Count; i++)
            {
                outGameObjectsRequests.Add(Instantiate(requestPrefab, outRequestsParent));
                PlayerFriendProfile playerFriendProfile = outGameObjectsRequests[i].GetComponent<PlayerFriendProfile>();
                playerFriendProfile.firebaseFriendsList = firebaseFriendsList;
                playerFriendProfile.firebaseInOutFriendRequests = this;
                playerFriendProfile.firebaseSearchFriends = firebaseSearchFriends;
                playerFriendProfile.backgroundImage.sprite = allItemsDatabase.GetBannerSet(null, (int)playerFriendProfile.completeProfile.profile.bannerId).friendListImage;

                playerFriendProfile.acceptFriendRequestButton.gameObject.SetActive(false);
                playerFriendProfile.declineFriendRequestButton.gameObject.SetActive(false);
                playerFriendProfile.deleteFriendButton.gameObject.SetActive(false);
                playerFriendProfile.sendFriendRequestButton.gameObject.SetActive(false);
                //playerFriendProfile.inviteFriendButton.gameObject.SetActive(false);

                playerFriendProfile.revertFriendRequestButton.gameObject.SetActive(true);

                //Not sure if works correctly
                playerFriendProfile.completeProfile.profile.name = outLocalUserIDs[i].userName;
                playerFriendProfile.userId = outLocalUserIDs[i].userId;
                await playerFriendProfile.GetCompleteProfileData();
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

            OnResyncInOutRequestList.Invoke();
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }

        if (allowDebug)
        {
            print("In Game Requests made: " + inGameObjectsRequests.Count + ". Out Game Requests made: " + outGameObjectsRequests.Count);
        }
    }
}
