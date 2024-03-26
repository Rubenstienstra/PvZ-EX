using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Theme", menuName = "Theme")]
public class ThemeInfo : ScriptableObject
{
    public string themeName;

    public Sprite themeSprite;
    public Sprite profileButtonSprite;
    public Sprite friendsListSprite;
    public Sprite profilePageSprite;
    public int id;

}
