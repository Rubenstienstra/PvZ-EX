using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeFriendsListLength : MonoBehaviour
{

    [Header("Variables")]
    public float spaceBetweenPrefabs = 411.7f;//311.7 + 100
    public RectTransform[] contentList = new RectTransform[1];

    private void Start()
    {
        UpdateViewportLength();
    }
    public void UpdateViewportLength()
    {
        for (int i = 0; i < contentList.Length; i++)
        {
            int amountOfFriends = contentList[i].childCount;
            contentList[i].sizeDelta = new Vector2(0, amountOfFriends * spaceBetweenPrefabs);
        }
    }
}
