using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Unity.VisualScripting;
using Firebase.Database;
using TMPro;
using System.Linq;
using UnityEngine.Events;
using System;

public class FirebaseCharacterClothingInventory : MonoBehaviour//Add Clothing, Replace Character.
{
    public UnityEvent OnResyncCharacter;
    public UnityEvent OnResyncClothes;

    [Header("Local Inventory"), Space]
    public User.Clothes localUserClothes;
    public User.Character localUserCharacter;

    [Header("Debug"), Space]
    public bool allowDebug = true;
    public bool resyncedCharacter;
    public bool resyncedClothes;
    public List<long> onlineUserClothes = new();
    public User.Character onlineUserCharacter = new();

    [Header("Scripts")]
    public FirebaseManager firebaseManager;
    public FirebaseInventoryManager firebaseInventory;
    public Closet closet;

    private IEnumerator Start()//Waits until Firebase is ready
    {
        yield return new WaitUntil(() => firebaseManager.initializedFirebase);



        yield return null;
    }

    public void AddingClothingIdToList(long idToAdd) =>
        localUserClothes.clothesIds.Add(idToAdd);

    public async void AddingClothes(bool firstTime)
    {
        if (firstTime)
        {
            localUserClothes.clothesIds.Add(7);
            localUserClothes.clothesIds.Add(1);
            localUserClothes.clothesIds.Add(4);
        }
        await AddingClothingDataAsync();
    }

    public async void ResyncClothes() =>
        await ResyncClothingDataAsync();
    
    public async void AddingCharacter(bool firstTime)
    {
        if (firstTime)
        {
            localUserCharacter.topId = 7;
            localUserCharacter.bottomId = 1;
            localUserCharacter.hairId = 4;
        }
        await AddingCharacterDataAsync();
    }

    public async void ResyncCharacter() =>
        await ResyncCharacterDataAsync();

    #region Clothing

    private async Task AddingClothingDataAsync()
    {
        var jsonClothes = JsonConvert.SerializeObject(localUserClothes.clothesIds);

        await firebaseInventory.SendingData(jsonClothes, "Users", firebaseManager.User.DisplayName, "Inventory", "Clothes");

        if (allowDebug)
        {
            print("ClothingInventory to firebase updated!");
        }
    }
    private async Task ResyncClothingDataAsync()
    {
        try
        {
            var rootReference = firebaseManager.Database.RootReference.Child("Users");

            await rootReference.Child(firebaseManager.User.DisplayName).Child("Inventory").Child("Clothes").GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot dataSnapshot = task.Result;

                    onlineUserClothes.Clear();
                    localUserClothes.clothesIds.Clear();
                    for (int i = 0; i < dataSnapshot.ChildrenCount; i++)
                    {
                        onlineUserClothes.Add((long)dataSnapshot.Child(i.ToString()).Value);
                        localUserClothes.clothesIds.Add((long)dataSnapshot.Child(i.ToString()).Value);

                        if (i == dataSnapshot.ChildrenCount - 1)
                        {
                            closet.InitFirebaseClothes(onlineUserClothes.ToArray());

                            if (allowDebug)
                            {
                                print("ClothingInventory from firebase updated!");
                            }

                        }
                    }
                    closet.crCharacter = onlineUserCharacter;
                    resyncedClothes = true;
                }
            });
            OnResyncClothes.Invoke();
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
    }
    #endregion

    #region Character
    private async Task AddingCharacterDataAsync()
    {
        var jsonCharacter = JsonConvert.SerializeObject(localUserCharacter);
        await firebaseInventory.SendingData(jsonCharacter, "Users", firebaseManager.User.DisplayName, "Inventory", "Character", null, null);

        if (allowDebug)
        {
            print("CharacterInventory to firebase updated!");
        }
    }
    private async Task ResyncCharacterDataAsync()
    {
        try
        {
            var rootReference = firebaseManager.Database.RootReference.Child("Users");

            await rootReference.Child(firebaseManager.User.DisplayName).Child("Inventory").Child("Character").GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot dataSnaphot = task.Result;

                    onlineUserCharacter.hairId = (long)dataSnaphot.Child("hairId").Value;
                    onlineUserCharacter.headId = (long)dataSnaphot.Child("headId").Value;
                    onlineUserCharacter.topId = (long)dataSnaphot.Child("topId").Value;
                    onlineUserCharacter.bottomId = (long)dataSnaphot.Child("bottomId").Value;
                    onlineUserCharacter.shoesId = (long)dataSnaphot.Child("shoesId").Value;
                    onlineUserCharacter.accessoryId = (long)dataSnaphot.Child("accessoryId").Value;

                    localUserCharacter.hairId = (long)onlineUserCharacter.hairId;
                    localUserCharacter.headId = (long)onlineUserCharacter.headId;
                    localUserCharacter.topId = (long)onlineUserCharacter.topId;
                    localUserCharacter.bottomId = (long)onlineUserCharacter.bottomId;
                    localUserCharacter.shoesId = (long)onlineUserCharacter.shoesId;
                    localUserCharacter.accessoryId = (long)onlineUserCharacter.accessoryId;

                    resyncedCharacter = true;
                }
            });

            OnResyncCharacter.Invoke();
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
        if (allowDebug)
        {
            print("CharacterInventory from firebase updated!");
        }
    }
    #endregion
}
