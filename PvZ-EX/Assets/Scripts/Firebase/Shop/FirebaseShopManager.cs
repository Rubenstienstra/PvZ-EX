using Code.Scripts.UI.Shop;
using Firebase.Database;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Code.Scripts.Databases.Items;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using System;

public class FirebaseShopManager : MonoBehaviour
{
    public UnityEvent OnResyncedShop;
    [Header("Variables")]
    public List<long> localShopIds = new();

    [Header("Debug")]
    public bool allowDebug = true;

    public List<Item> localShopItems = new();
    public List<long> onlineShopIds = new();
    public List<Item> onlineShopItems = new();

    [Header("Scripts")]
    public FirebaseManager firebaseManager;
    public FirebaseInventoryManager firebaseInventory;
    public ShopController shopController;
    public FirebaseCharacterClothingInventory firebaseClothesInventory;

    IEnumerator Start()
    {
        yield return new WaitUntil(() => firebaseManager.initializedFirebase);
        ResyncShop();
    }

    public async void SetShop()
    {
        await SetShopDataAsync();
    }
    public async void ResyncShop()//For Buttons
    {
        await ResyncShopDataAsync();
    }

    public async Task SetShopDataAsync()
    {
        string jsonShopData = JsonConvert.SerializeObject(localShopIds);

        await firebaseInventory.SendingData(jsonShopData, "Shop", "CrShop", null, null, null, null);

        if (allowDebug)
        {
            print("ShopData to firebase updated!");
        }
    }
    public async Task ResyncShopDataAsync()
    {
        try
        {
            var rootReference = firebaseManager.Database.RootReference.Child("Shop");

            await rootReference.Child("CrShop").GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot dataSnapshot = task.Result;

                    onlineShopIds.Clear();
                    for (int i = 0; i < dataSnapshot.ChildrenCount; i++)
                    {
                        onlineShopIds.Add((long)dataSnapshot.Child(i.ToString()).Value);
                    }

                    TurningIdIntoShopData();
                }
            });
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
    }
    private void TurningIdIntoShopData()
    {
        if(onlineShopIds.Count > shopController.allShopItems.Count)
        {
            Debug.LogWarning("onlineShopId: " + onlineShopIds.Count + ".Has more variables then the allShopItems: " + shopController.allShopItems.Count);
            return;
        }

        localShopIds.Clear();
        localShopItems.Clear();
        onlineShopItems.Clear();

        for (int i = 0; i < onlineShopIds.Count; i++)
        {
            localShopIds.Add(onlineShopIds[i]);

            var crShopItem = shopController.allShopItems[(int)onlineShopIds[i]];

            if(firebaseClothesInventory.localUserClothes.clothesIds.Contains(onlineShopIds[i]))
            {
                crShopItem.itemOwned = true;
                if (allowDebug)
                {
                    print("A Item has already been bought!");
                }
            }
            localShopItems.Add(crShopItem);
            onlineShopItems.Add(crShopItem);
            shopController.shopItemsToMake.Add(crShopItem);
        }

        OnResyncedShop.Invoke();
        if (allowDebug)
        {
            print("ShopData from firebase updated!");
        }
    }
}
