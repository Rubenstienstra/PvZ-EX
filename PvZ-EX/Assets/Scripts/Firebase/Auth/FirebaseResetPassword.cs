using Firebase;
using Firebase.Auth;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FirebaseResetPassword : MonoBehaviour
{
    public UnityEvent OnEmailSend;

    [Header("Variables")]
    public bool useUserEmail;
    public TMP_InputField emailInputField;
    public Button resetButton;
    public Task resetPasswordTask;
    public float timeBeforePressAgain = 10;

    [Header("Debug")]
    public string errorMessage = "";
    public TMP_Text errorMessageText;
    public TMP_Text goodMessageText;

    [Header("Scripts")]//You don't need both scripts
    public FirebaseAuthManager firebaseAuthManager;
    public MainMenuUIManager uiManager;
    public FirebaseManager firebaseManager;
    public void ResetPassword()
    {
        errorMessageText.text = "";
        string crEmail;
        
        if (firebaseAuthManager)
        {
            if (useUserEmail)
            {
                if(firebaseAuthManager.user == null)
                {
                    errorMessageText.text = "No one has logged in yet!";
                    Debug.LogWarning("FirebaseResetPassword has no current user!", this);
                    return;
                }
                crEmail = firebaseAuthManager.user.Email;
            }
            else
            {
                crEmail = emailInputField.text.Trim();
            }
            if (CheckEmail(crEmail))
            {
                resetPasswordTask = firebaseAuthManager.auth.SendPasswordResetEmailAsync(crEmail);
                StartCoroutine(CheckAsyncTask());
            }
        }
        else if (firebaseManager)
        {
            if (useUserEmail)
            {
                if (firebaseManager.User == null)
                {
                    errorMessageText.text = "No one has logged in yet!";
                    Debug.LogWarning("FirebaseResetPassword has no current user!", this);
                    return;
                }
                crEmail = firebaseManager.User.Email;
            }
            else
            {
                crEmail = emailInputField.text.Trim();
            }
            if (CheckEmail(crEmail))
            {
                resetPasswordTask = firebaseManager.Auth.SendPasswordResetEmailAsync(crEmail);
                StartCoroutine(CheckAsyncTask());
            }
        }
        if (resetButton)
        {
            StartCoroutine(ResetCooldownTimer(resetButton));
        }
        OnEmailSend.Invoke();
    }

    private bool CheckEmail(string crEmail)
    {
        if (crEmail == "")
        {
            errorMessageText.text = "Nothing is filled in the email text field!";
            Debug.LogError("Email field does not contain text");
            return false;
        }
        else if (!crEmail.Contains("@"))
        {
            errorMessageText.text = "Your email does not contain a: @";
            Debug.LogError("Email field does not contain: @");
            return false;
        }
        return true;
    }

    private IEnumerator CheckAsyncTask()
    {
        yield return new WaitUntil(() => resetPasswordTask.IsCompleted);

        if (resetPasswordTask.IsFaulted)
        {
            FirebaseException firebaseException = resetPasswordTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseException.ErrorCode;
        
            if (resetPasswordTask.Exception != null)
            {
                switch (authError)
                {
                    case AuthError.InvalidEmail:
                        errorMessage = "Email is invalid";
                        break;
                    case AuthError.MissingEmail:
                        errorMessage = "Email is missing";
                        break;
                    case AuthError.Cancelled:
                        errorMessage = "Action got cancelled";
                        break;
                    case AuthError.UnverifiedEmail:
                        errorMessage = "Unverified email";
                        break;
                    case AuthError.EmailChangeNeedsVerification:
                        errorMessage = "Email needs verification";
                        break;
                    default:
                        errorMessage = "Something went wrong";
                        break;
                }
                Debug.LogError(errorMessage);
                errorMessageText.text = errorMessage;
            }
        }   
        else if (firebaseAuthManager)//firebaseManager doesn't have a login panel.
        {
            uiManager.OpenLoginPanel();
            goodMessageText.text = "Email has been sended!";
        }
        yield return null;
    }

    public IEnumerator ResetCooldownTimer(Button button)
    {
        button.interactable = false;
        yield return new WaitForSeconds(timeBeforePressAgain);
        button.interactable = true;

        yield return null;
    }
}
