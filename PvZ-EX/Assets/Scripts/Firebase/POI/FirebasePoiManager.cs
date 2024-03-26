using Firebase.Database;
using JetBrains.Annotations;
using Mapbox.Examples;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

//jop@easysee.nl

public class FirebasePoiManager : MonoBehaviour
{
    public UnityEvent OnResyncPlayerPoi;

    [Header("Variables")]
    public FirstTimePOI firstTimePOI = new() { firstTimeBonus = true };

    public List<User.PoiInfo> allLocalPlayerPoiInfo = new();
    public List<User.PoiInfo> allOnlinePlayerPoiInfo = new();

    [Header("Debug")]
    public bool allowDebug = true;


    [Header("Scripts")]
    public FirebaseManager firebaseManager;
    public FirebaseInventoryManager firebaseInventoryManager;
    public SpawnOnMap spawnOnMap;
    public enum PoiType { General = 0, Leisure = 1, History = 2 };

    [Serializable]
    public class FirstTimePOI
    {
        public string firstArrivedTime;
        public bool firstTimeBonus;
    }

    [Serializable]
    public class POIData
    {
        //Doesn't get saved
        public string poiName;
        public PoiType poiType;
    }
    private IEnumerator Start()//Waits until Firebase is ready
    {
        yield return new WaitUntil(() => firebaseManager.initializedFirebase == true);

        if (!spawnOnMap)
        {
            Debug.LogWarning("FirebasePoiManager variable: spawnOnMap is empty before game start!", this);
            spawnOnMap = GameObject.Find("EventSpawner").GetComponent<SpawnOnMap>();
        }
        yield return null;
    }

    public async void AddPlayerPOIData()
    {
        await AddPlayerPOIDataAsync();
    }
    public async void ResyncPlayerPOIData()
    {
        await ResyncPlayerPOIDataAsync();
    }

    public async Task AddPlayerPOIDataAsync()
    {
        string playerPoiJson = JsonConvert.SerializeObject(allLocalPlayerPoiInfo);
        string firstTimeJson = JsonConvert.SerializeObject(firstTimePOI);
        try
        {
            await firebaseInventoryManager.SendingData(playerPoiJson, "Users", firebaseManager.User.DisplayName, "POIData");
            await firebaseInventoryManager.SendingData(firstTimeJson, "Users", firebaseManager.User.DisplayName, "POIData", "FirstTimePOI");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"FirebasePoiManager: {ex}");
        }

        if (allowDebug)
        {
            print("FirebasePoiManager to firebase updated!");
        }
    }
    public async Task ResyncPlayerPOIDataAsync()
    {
        allOnlinePlayerPoiInfo.Clear();
        allLocalPlayerPoiInfo.Clear();

        try
        {
            var rootreference = firebaseManager.Database.RootReference.Child("Users");
            await rootreference.Child(firebaseManager.User.DisplayName).Child("POIData").GetValueAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot dataSnapshot = task.Result;

                    int totalChildren = (int)dataSnapshot.ChildrenCount - 1;// -1 Because FirstTimePOI is also a child. 

                    firstTimePOI.firstArrivedTime = (string)dataSnapshot.Child("FirstTimePOI").Child("firstArrivedTime").Value;
                    firstTimePOI.firstTimeBonus = (bool)dataSnapshot.Child("FirstTimePOI").Child("firstTimeBonus").Value;

                    for (int i = 0; i < totalChildren; i++)
                    {
                        allOnlinePlayerPoiInfo.Add(new()
                        {
                            arrivedTime = (string)dataSnapshot.Child(i.ToString()).Child("arrivedTime").Value,
                            poiId = (long)dataSnapshot.Child(i.ToString()).Child("poiId").Value
                        });
                        allLocalPlayerPoiInfo.Add(new()
                        {
                            arrivedTime = allOnlinePlayerPoiInfo[i].arrivedTime,
                            poiId = allOnlinePlayerPoiInfo[i].poiId
                        });
                        print("FirebasePoiManager from firebase updated!");
                        OnResyncPlayerPoi.Invoke();
                    }
                }
            });
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
    }

    #region OldCode
    //private async void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        await SetData();
    //    }

    //    #region oldcode
    //    // Van json class naar User
    //    //string json = "";
    //    //User user = JsonConvert.DeserializeObject<User>(json);

    //    // Van User class naar json
    //    //User user1 = new();
    //    //string json1 = JsonConvert.SerializeObject(user1);

    //    //await GetData();
    //    //await firebaseManager.Database.RootReference.Child("Dier").SetValueAsync(newDier);
    //    #endregion
    //}

    //public async Task SetData()
    //{
    //    try
    //    {
    //        Dier newDier = new()
    //        {
    //            Age = 40,
    //            zitposities = new(),
    //        };

    //        string json = JsonConvert.SerializeObject(newDier);

    //        Debug.Log(json);

    //        await firebaseManager.Database.RootReference.Child("Dier").SetRawJsonValueAsync(json);


    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogWarning(ex);
    //    }
    //}

    [Serializable]
    public class Dier
    {
        public long Age;
        public string Gender = "een idee";
        public Zitposities zitposities;

        [Serializable]
        public class Zitposities
        {
            public string favoriete = "hok";
            public int aantalKeerGezeten = 56;
        }
    }

    //public async Task GetData()
    //{
    //    try
    //    {
    //        //var result = await firebaseManager.Database.RootReference.Child("Dier");
    //        //{

    //        //});

    //        //Debug.Log(result.GetRawJsonValue());

    //        //string json = result.GetRawJsonValue();
    //        //Dier dier = JsonConvert.DeserializeObject<Dier>(json);

    //        //Debug.Log(dier.Age);

    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogWarning(ex);
    //    }

    //    //RunTransaction
    //    //Get
    //    //Set
    //    //Push
    //    //Update
    //}
    #endregion
}

