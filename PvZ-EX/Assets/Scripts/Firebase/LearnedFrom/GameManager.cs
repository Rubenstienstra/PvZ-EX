using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text welcomeText;

    void Start()
    {
        ShowWelcomeMessage();
    }

    private void ShowWelcomeMessage()
    {
        welcomeText.text = $"Welcome {References.userName} to our Game Scene";
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("FirebaseDevelopment");
    }
}
