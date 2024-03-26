using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ThemesManager : MonoBehaviour
{
    public Image profileButton;
    public Transform friendListContent;
    public Image profilePage;
    public Transform themesContent;
    public List<ThemeInfo> themes;
    public GameObject themePrefab;
    public static ThemeInfo selectedTheme;

    public void Start() {
        SetupThemeMenu(themesContent);
        Debug.Log(profileButton.name);
    }
    public void SetupThemeMenu(Transform content) {
        for (int i = 0; i < themes.Count; i++) {
            GameObject themeObject = Instantiate(themePrefab, content);
            themes[i].id = i;
            themeObject.GetComponent<Theme>().themeInfo = themes[i];
            themeObject.GetComponent<Image>().sprite = themes[i].themeSprite;
            themeObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = themes[i].themeName;
            themeObject.GetComponent<Button>().onClick.AddListener(() => {
                ChangeTheme(themeObject.GetComponent<Theme>());
            });
        }
    }
    public void ChangeTheme(Theme theme) {
        selectedTheme = theme.themeInfo;
        profileButton.sprite = selectedTheme.profileButtonSprite;
        profilePage.gameObject.SetActive(true);
        profilePage.sprite = selectedTheme.profilePageSprite;
        ApplyThemeToFriendList();
    }

    public void ApplyThemeToFriendList() {
        foreach(Transform child in friendListContent) {
            child.GetComponent<Image>().sprite = selectedTheme.friendsListSprite;
        }
    }
}
