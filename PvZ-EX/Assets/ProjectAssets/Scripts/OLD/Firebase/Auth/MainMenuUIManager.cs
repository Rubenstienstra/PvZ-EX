using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Code.Scripts.Firebase.Player;

public class MainMenuUIManager : MonoBehaviour
{
    public static MainMenuUIManager Instance;
    public FirebaseAuthManager firebaseAuthManager;



    [Header("Variables")]
    public GameObject loginPanel;
    public GameObject registrationPanel;
    public CanvasGroup gamePanel;
    public GameObject emailVerificationPanel;
    public GameObject passwordResetPanel;

    public TMP_Text emailVerificationInfoText;

    public float fadeDuration = 2f; // Duration of the fade in seconds
    public CanvasGroup loadingCanvasGroupImage;
    public GameObject gameUI;
    public bool uiOpen;

    [Header("Debug")]
    public TMP_Text errorMessageText;
    public TMP_Text goodMessageText;


    private void Awake()
    {
        CreateInstance();
    }
    private void CreateInstance()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    IEnumerator FadeOut(CanvasGroup canvasGroup) {
        float elapsedTime = 0f;

        // Gradually decrease the alpha value over time
        while (elapsedTime < fadeDuration) {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            canvasGroup.alpha = alpha;

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Ensure the final alpha value is set to 0
        canvasGroup.alpha = 0f;

    }

    public void ClearUI()
    {
        loginPanel.SetActive(false);
        registrationPanel.SetActive(false);
        emailVerificationPanel.SetActive(false);
        passwordResetPanel.SetActive(false);
        gamePanel.gameObject.SetActive(false);
        goodMessageText.SetText("");
        errorMessageText.SetText("");
    }

    public void OpenLoginPanel()
    {
        ClearUI();
        loginPanel.SetActive(true);
    }

    public void OpenRegistrationPanel()
    {
        ClearUI();
        registrationPanel.SetActive(true);
    }

    public void OpenGamePanel()
    {
        ClearUI();
        gamePanel.gameObject.SetActive(true);
    }

    public void OpenResetPasswordPanel()
    {
        ClearUI();
        passwordResetPanel.SetActive(true);
    }

    public void ShowVerficationResponse(bool emailIsSent, string emailId, string errorMessage)
    {
        ClearUI();
        emailVerificationPanel.SetActive(true);

        if (emailIsSent)
        {
            emailVerificationInfoText.text = $"Please verify your email adress \n Verification email has been sent to {emailId}";
        }
        else
        {
            emailVerificationInfoText.text = $"Couldn't sent email : {errorMessage}";
        }
    }

    public void Play() 
    {
        StartCoroutine(PlayDelay());
    }
    IEnumerator PlayDelay() 
    {
        yield return StartCoroutine(FadeOut(gamePanel));
        firebaseAuthManager.OpenGameScene();
    }
}
