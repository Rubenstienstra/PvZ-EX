using Assets.Code.Scripts.Achievements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Achievement : MonoBehaviour {
    public AchievementInfo achievementInfo;
    public Image badge;
    public void Start() {
        achievementInfo.rarity = achievementInfo.CheckRequirements();
        Debug.Log(achievementInfo.title + " rarity: " + achievementInfo.rarity);
    }

}
