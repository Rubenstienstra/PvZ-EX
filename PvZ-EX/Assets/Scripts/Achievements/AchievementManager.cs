using Assets.Code.Scripts.Achievements;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementManager : MonoBehaviour
{
    public PlayerAchievements playerAchievements;
    public ProfileAchievementsManager profileAchievementsManager;
    public List<Sprite> raritySprites;
    public Transform achievementMenuContent;
    public GameObject AchievementPrefab;

    [Header("Selected Achievement UI")]
    public TMP_Text selectedAchievementDescription;
    public TMP_Text selectedAchievementProgress;
    public TMP_Text selectedAchievementTitle;
    public Image selectedAchievementImage;

    public AchievementInfo currentAchievement;



    public void Start() {
        SetupAchievements(achievementMenuContent, false);
        ChangeAchievementInfo(playerAchievements.achievements[0]);
    }

    public void UpdateRaritySprites() {
        foreach( Transform child in achievementMenuContent) {
            child.GetComponent<Achievement>().badge.sprite = raritySprites[child.GetComponent<Achievement>().achievementInfo.rarity];
        }
        ChangeAchievementInfo(currentAchievement);
    }

    public void SetupAchievements(Transform content, bool profilemode) {
        for (int i = 0; i < playerAchievements.achievements.Count; i++) {
            GameObject achievementObject = Instantiate(AchievementPrefab, content);
            Achievement achievement = achievementObject.GetComponent<Achievement>();
            playerAchievements.achievements[i].id = i;
            achievement.achievementInfo = playerAchievements.achievements[i];
            achievementObject.transform.GetChild(0).GetComponent<Image>().sprite = raritySprites[achievement.achievementInfo.rarity];
            achievementObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = achievement.achievementInfo.name;
            achievementObject.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = achievement.achievementInfo.description;
            achievementObject.GetComponent<Button>().onClick.AddListener(() => {
                if (!profilemode) {
                    ChangeAchievementInfo(achievementObject.GetComponent<Achievement>().achievementInfo);
                } else {
                    profileAchievementsManager.ChangeProfileAchievements(achievementObject.GetComponent<Achievement>().achievementInfo);
                }
            });
        }
    }


    private void ChangeAchievementInfo(AchievementInfo info) {
        currentAchievement = info;
        selectedAchievementTitle.text = info.title;
        selectedAchievementDescription.text = info.description;
        selectedAchievementImage.sprite = raritySprites[info.rarity];
        selectedAchievementProgress.text = "Progress: " + info.progress + "/" + info.maxValue;
    }
}
