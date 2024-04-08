using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;
using Firebase;
using System.Threading.Tasks;
using UnityEditor;
using Newtonsoft.Json;
using System;

public class FirebaseManager : MonoBehaviour
{
    public FirebaseApp App;
    public FirebaseAuth Auth;
    public FirebaseDatabase Database;
    public FirebaseUser User;

    private bool donePlayerCheck = new();
    public bool initializedFirebase;// if this is true then you can use firebase
    public bool allowDebug;

    [Header("Scripts")]
    public FirebaseInventoryManager firebaseInventoryManager;
    public FirebasePoiManager firebasePoiManager;

    private async void Awake()
    {
        await InitializeFirebase();
    }

    public async Task InitializeFirebase()
    {
        try
        {
            var returnedStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (returnedStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                StartCoroutine(InitializeFirebaseInstances());
            }
            else// Firebase Unity SDK is not safe to use here.
            {
                UnityEngine.Debug.LogError(System.String.Format("Firebase is not opperational and could not resolve all Firebase dependencies: {0}", returnedStatus));
            }
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
    }

    private IEnumerator InitializeFirebaseInstances()
    {
        App = FirebaseApp.DefaultInstance;
        yield return new WaitUntil(() => App != null);
        if (allowDebug) { print("Innitialized App"); }
        
        Auth = FirebaseAuth.DefaultInstance;
        yield return new WaitUntil(() => Auth != null);
        if (allowDebug) { print("Innitialized Auth"); }

        Database = FirebaseDatabase.DefaultInstance;
        yield return new WaitUntil(() => Database != null);
        if (allowDebug) { print("Initialized Database"); }

        User = Auth.CurrentUser;
        yield return new WaitUntil(() => User != null);
        if (allowDebug) { print("Initialized User"); }

        CheckForNewPlayer();
        yield return new WaitUntil(() => donePlayerCheck);
        if (allowDebug) { print("Checked if Player new"); }

        initializedFirebase = true;
        print("Firebase: App, Auth, Data & User is ready!");
    }
    #region NewPlayer
    public async void CheckForNewPlayer()
    {
        await CheckForNewPlayerAsync();
    }


    private async Task CheckForNewPlayerAsync()
    {
        try
        {
            var rootReference = Database.RootReference.Child("Users").Child(User.DisplayName).Child("FirebaseSettings").Child("Newplayer");
            var dataSnapshot = await rootReference.GetValueAsync();

            if ((bool)dataSnapshot.Value)//If player is new player
            {
                if (allowDebug)
                {
                    print("Player is New: true");
                }
                //True, FirstTime
                AddAllData(true);

                bool firstTime = false;
                var jsonFirstTime = JsonConvert.SerializeObject(firstTime);
                await rootReference.SetRawJsonValueAsync(jsonFirstTime);
            }
            else
            {
                if (allowDebug)
                {
                    print("Player is New: false");
                }
                ResyncAllData();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
        donePlayerCheck = true;
        #region OldCode
        //private async Task CheckForNewPlayerAsync()
        //{
        //    var rootReference = Database.RootReference.Child("Users").Child(User.DisplayName).Child("FirebaseSettings").Child("Newplayer");
        //    var task = await rootReference.GetValueAsync().ContinueWith(async task =>
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
        //                //True, FirstTime
        //                AddAllData(true);

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
        //                ResyncAllData();
        //            }
        //            newPlayerCheck = true;
        //        }
        //    });
        //}
        #endregion
    }


    private void AddAllData(bool newPlayer)
    {
        firebaseInventoryManager.AddingAllInventoryData(newPlayer);
        firebasePoiManager.AddPlayerPOIData();
    }
    private void ResyncAllData()
    {
        firebaseInventoryManager.ResyncAllInventoryData();
        firebasePoiManager.ResyncPlayerPOIData();
    }
    #endregion
}
