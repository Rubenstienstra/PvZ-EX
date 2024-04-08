using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFriendProfile : MonoBehaviour
{
    [Header("Variables")]
    public string userId;
    public bool hasCompleteProfile;

    public TMP_Text textName;
    public TMP_Text textSummary;
    public TMP_Text textLevel; private string crTextLevel;
    public Image backgroundImage;

    public Button inviteFriendButton;
    public Button deleteFriendButton;
    public Button sendFriendRequestButton;
    public Button acceptFriendRequestButton;
    public Button declineFriendRequestButton;
    public Button revertFriendRequestButton;

    [Header("Scripts")]
    public FirebaseFriendsList firebaseFriendsList;//Empty Till Created
    public FirebaseSearchFriends firebaseSearchFriends;//Empty Till Created
    public FirebaseInOutFriendRequests firebaseInOutFriendRequests;//Empty Till Created;
    public User.CompleteProfile completeProfile;//Empty Till Created

    //UI
    public void SetInfoIntoUI()
    {
        textName.text = completeProfile.profile.name;
        textSummary.text = completeProfile.profile.summary;
        crTextLevel = textLevel.text;
        textLevel.text = crTextLevel + completeProfile.level;
        //backgroundImage = 
    }
    public void UpdateInfoIntoUI()
    {
        textName.text = completeProfile.profile.name;
        textSummary.text = completeProfile.profile.summary;
        textLevel.text = crTextLevel + completeProfile.level;
        //backgroundImage = 
    }

    //Friends
    public void InviteThisFriend()
    {

    }
    public void AddThisFriend()
    {
        firebaseInOutFriendRequests.SendFriendRequest(new User.UserIDs { userId = userId, userName = completeProfile.profile.name });
    }
    public void AcceptThisFriend()
    {
        firebaseInOutFriendRequests.AcceptFriendRequest(new User.UserIDs { userId = userId, userName = completeProfile.profile.name });
    }
    public async void DeleteThisFriend()
    {
        await firebaseFriendsList.DeleteFriend(new User.UserIDs { userId = userId, userName = completeProfile.profile.name }, completeProfile, gameObject);
    }
    public void DeclineFriendRequest()
    {
        firebaseInOutFriendRequests.DeclineFriendRequest(
        new User.UserIDs
        {
            userName = completeProfile.profile.name,
            userId = userId
        }, new User.UserIDs
        {
            userName = firebaseFriendsList.firebaseManager.User.DisplayName,
            userId = firebaseFriendsList.firebaseManager.User.UserId

        });
    }
    public void RevertFriendRequest()
    {
        firebaseInOutFriendRequests.RevertFriendRequest(
        new User.UserIDs
        {
            userName = completeProfile.profile.name,
            userId = userId

        }, new User.UserIDs
        {
            userName = firebaseFriendsList.firebaseManager.User.DisplayName,
            userId = firebaseFriendsList.firebaseManager.User.UserId
        });
    }

    public async Task GetCompleteProfileData()
    {
        try
        {
            DatabaseReference rootReference = firebaseFriendsList.firebaseManager.Database.RootReference.Child("Users");

            await rootReference.Child(completeProfile.profile.name).Child("Inventory").GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot dataSnapshot = task.Result;

                    completeProfile.profile.bannerId = (long)dataSnapshot.Child("Profile").Child("bannerId").Value;
                    completeProfile.profile.summary = (string)dataSnapshot.Child("Profile").Child("summary").Value;

                    //Achievements
                    completeProfile.profile.achievements.Clear();
                    long amountOfChildren = dataSnapshot.Child("Profile").Child("achievements").ChildrenCount;
                    for (int i = 0; i < amountOfChildren; i++)
                    {
                        completeProfile.profile.achievements.Add(new()
                        {
                            achievementId = (long)dataSnapshot.Child("Profile").Child("achievements").Child(i.ToString()).Child("achievementId").Value,
                            progress = (long)dataSnapshot.Child("Profile").Child("achievements").Child(i.ToString()).Child("progress").Value,
                            rarity = (long)dataSnapshot.Child("Profile").Child("achievements").Child(i.ToString()).Child("rarity").Value
                        });
                    }

                    //Progression
                    completeProfile.xp = (long)dataSnapshot.Child("Progression").Child("xp").Value;
                    completeProfile.level = (long)dataSnapshot.Child("Progression").Child("level").Value;

                    //Character
                    completeProfile.character.hairId = (long)dataSnapshot.Child("Character").Child("hairId").Value;
                    completeProfile.character.headId = (long)dataSnapshot.Child("Character").Child("headId").Value;
                    completeProfile.character.topId = (long)dataSnapshot.Child("Character").Child("topId").Value;
                    completeProfile.character.bottomId = (long)dataSnapshot.Child("Character").Child("bottomId").Value;
                    completeProfile.character.shoesId = (long)dataSnapshot.Child("Character").Child("shoesId").Value;
                    completeProfile.character.accessoryId = (long)dataSnapshot.Child("Character").Child("accessoryId").Value;

                    hasCompleteProfile = true;
                }
            });
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
    }
}
