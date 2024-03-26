using UnityEngine;
using System.Collections.Generic;
using Assets.Code.Scripts.Achievements;

public static class PlayerProgressManager {
    public static void UpdateProgress(string achievementName, int valueToAdd) {
        bool updated = false;
        foreach(AchievementInfo achievement in PlayerAchievements.Instance.achievements) {
            if (achievement.title == achievementName) {
                achievement.progress = achievement.progress + valueToAdd;
                achievement.rarity = achievement.CheckRequirements();
                Debug.Log(achievement.title + achievement.rarity);
                updated = true;
            }
        }
        if (updated == false) {
            Debug.LogError("could not update progress of: " + achievementName + ". Because it is not an achievement.");
        }
        updated = false;
    }

}
