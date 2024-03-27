using Code.Scripts.Databases.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllItemsDatabase : MonoBehaviour
{
    [Header("Variables")]
    public List<BannerSet> bannerSetList;
    public List<Item> shopItemsList;

    [Header("Debug")]
    public bool allowDebug;

    [Serializable]
    public class BannerSet
    {
        public string nameOfBannerSet;
        public Sprite mainmenuBackground;
        public Sprite profileBackground;
        public Sprite friendListImage;
    }
    public BannerSet GetBannerSet(string nameOfBannerSet = null, int? idOfBannerSet = null)
    {
        BannerSet bannerSetToSearch = new();

        if (idOfBannerSet != null)
        {
            bannerSetToSearch = bannerSetList[idOfBannerSet.Value];
            return bannerSetToSearch;
        }

        for (int i = 0; i < bannerSetList.Count; i++)
        {
            if(nameOfBannerSet != null)
            {
                if(bannerSetList[i].nameOfBannerSet == nameOfBannerSet)
                {
                    bannerSetToSearch = bannerSetList[i];
                    return bannerSetToSearch;
                }
            }
        }
        
        return null;
    }
}
