using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShowHidePassword : MonoBehaviour
{
    [Header("Variables")]
    public TMP_InputField passwordInputField;
    public TMP_InputField.ContentType defaultContentType;//Cannot be password

    public Image lookAtPasswordButtonImage;
    public Sprite passwordInvisibleSprite;
    public Sprite passwordVisibleSprite;

    public void PasswordShow()
    {
        if (!CheckIfReady())
        {
            return;
        }

        passwordInputField.contentType = defaultContentType;
        lookAtPasswordButtonImage.sprite = passwordVisibleSprite;
        passwordInputField.ForceLabelUpdate();
    }
    public void PasswordHide()
    {
        if (!CheckIfReady())
        {
            return;
        }

        passwordInputField.contentType = TMP_InputField.ContentType.Password;
        lookAtPasswordButtonImage.sprite = passwordInvisibleSprite;
        passwordInputField.ForceLabelUpdate();
    }
    public void PasswordToggle()
    {
        if (!CheckIfReady())
        {
            return;
        }

        if(passwordInputField.contentType == TMP_InputField.ContentType.Password)
        {
            PasswordShow();
        }
        else if(passwordInputField.contentType == defaultContentType)
        {
            PasswordHide();
        }
    }
    private bool CheckIfReady()
    {
        if(passwordInputField && lookAtPasswordButtonImage && passwordInvisibleSprite && passwordVisibleSprite)
        {
            return true;
        }
        return false;
    }
}
