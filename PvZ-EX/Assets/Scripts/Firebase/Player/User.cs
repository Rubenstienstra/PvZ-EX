using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

[Serializable]
public class User
{
    public Profile profile = new();
    public Character character = new();
    public Progression progression = new();
    public Clothes inventory = new();
    public UserIDs userIDs = new();

    [Serializable]
    public class Character
    {
        public long headId = new();
        public long topId = new();//shirts
        public long shoesId = new();
        public long hairId = new();
        public long bottomId = new();//pants
        public long accessoryId = new();
    }

    [Serializable]
    public class CompleteProfile
    {
        public Profile profile = new();
        public Character character = new();
        public long xp = new();
        public long level = new();
    }
    [Serializable]
    public class Profile
    {
        public string name;
        public string summary;
        public long bannerId = new();
        public List<AchievementInfo> achievements = new AchievementInfo[4].ToList();//Auto makes 4 slots

        //XP & Level can be get with Progression.
    }
    [Serializable]
    public class AchievementInfo
    {
        public long achievementId = new();
        public long progress = new();
        public long rarity = new();// 0 Locked, 1 Bronze, 2 Silver, 3 Gold, 4 Prismatic/Diamond.
    }
    [Serializable]
    public class PoiInfo
    {
        public long poiId = new();
        public string arrivedTime;
    }

    [Serializable]
    public class Progression
    {
        public long currency = new();
        public long xp = new();
        public long xpRequired = new();
        public long level = new();
    }
    [Serializable]
    public class Clothes
    {
        public List<long> clothesIds = new();
    }
    [Serializable]
    public struct UserIDs
    {
        public string userName;
        public string userId;//Text & Numbers
    }
}
