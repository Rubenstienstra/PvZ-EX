using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Firebase;
using Firebase.Auth;
using System;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.SceneManagement;
using Google.MiniJSON;
using Firebase.Database;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using UnityEngine.Events;
using System.Linq;

public class FirebaseAuthManager : MonoBehaviour
{
    public UnityEvent OnLoginSucces;
    public UnityEvent OnStartAutoLogin;
    public UnityEvent OnAccountCreationSucces;

    [Header("Variables")]
    public bool allowAutoLogin;
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;
    public FirebaseDatabase database;

    [Space, Header("Login")]
    public GameObject parentLogin;
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;

    [Space, Header("Registration")]
    public TMP_InputField nameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField confirmPasswordRegisterField;
    public UnityEngine.UI.Button confirmButtonRegisterField;

    public bool checkForSameName;
    public bool checkForSameEmail;
    [Header("Verification")]
    public GameObject parentEmailVerification;

    [Header("SceneName")]
    public string gameSceneToGo = "Game";

    [Header("Debug"), Space]
    public bool allowDebug;
    public TMP_Text errorTextMessage;

    #region OldCode
    //private void Awake()
    //{
    //    FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
    //    {
    //        dependencyStatus = task.Result;

    //        if(dependencyStatus == DependencyStatus.Available)
    //        {
    //            InitializerFirebase();
    //        }
    //        else
    //        {
    //            Debug.LogWarning("Could not resolve all firebase dependencies: " + dependencyStatus);
    //        }
    //    });
    //}
    #endregion

    private void OnApplicationFocus(bool focus)
    {
        if (user != null)
        {
            if (user.IsEmailVerified && parentEmailVerification.activeSelf)
            {
                if (allowDebug)
                {
                    print("Email is verified!");
                }
                parentEmailVerification.SetActive(false);
                parentLogin.SetActive(true);
            }
        }
    }

    private void Start()
    {
        StartCoroutine(CheckAndFixDependenciesAsync());
    }

    private IEnumerator CheckAndFixDependenciesAsync()
    {
        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();

        yield return new WaitUntil(() => dependencyTask.IsCompleted);

        dependencyStatus = dependencyTask.Result;

        if (dependencyStatus == DependencyStatus.Available)
        {
            InitializeFirebase();
            yield return new WaitForEndOfFrame();
            if (allowAutoLogin)
            {
                StartCoroutine(CheckForAutoLogin());
            }
        }
        else
        {
            Debug.LogWarning("Could not resolve all firebase dependencies: " + dependencyStatus);
        }
    }

    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        database = FirebaseDatabase.DefaultInstance;

        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    #region AutoLogin
    private IEnumerator CheckForAutoLogin()
    {
        if(user != null && allowAutoLogin)//If user has logged in before
        {
            OnStartAutoLogin.Invoke();
            var reloadUserTask = user.ReloadAsync();

            yield return new WaitUntil(() => reloadUserTask.IsCompleted);

            AutoLogin();
        }
        else
        {
            MainMenuUIManager.Instance.OpenLoginPanel();
        }
    }

    private void AutoLogin()
    {
        if(user != null && allowAutoLogin)//If user has logged in before
        {
            if (user.IsEmailVerified)
            {
                OnLoginSucces.Invoke();
                References.userName = user.DisplayName;
                SceneManager.LoadScene(gameSceneToGo);
                //UIManager.Instance.OpenGamePanel();
            }
            else
            {
                SendEmailForVerification();
            }
        }
        else
        {
            MainMenuUIManager.Instance.OpenLoginPanel();
        }
    }
    #endregion

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if(!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
                MainMenuUIManager.Instance.OpenGamePanel();
                ClearLoginInputFieldText();
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
            }
        }
    }

    private void ClearLoginInputFieldText()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
    }

    #region Logout

    public void Logout()
    {
        StartCoroutine(LogoutAsync());
    }

    public IEnumerator LogoutAsync()
    {
        if (auth != null && user != null)
        {
            auth.SignOut();
            yield return new WaitUntil(() => auth != null);
            print("Succesfully logged out ");

            MainMenuUIManager.Instance.OpenLoginPanel();
        }
        yield return null;
    }

    #endregion

    #region Login
    public void Login() //For Button
    {
        StartCoroutine(LoginAsync(emailLoginField.text.Trim(), passwordLoginField.text.Trim()));
    }

    private IEnumerator LoginAsync(string email, string password)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => loginTask.IsCompleted);

        if(loginTask.Exception != null)// If something is wrong
        {
            #region Debugging
            Debug.LogError(loginTask.Exception);

            FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseException.ErrorCode;

            string failedMessage = "Login Failed! Because: ";

            switch (authError)//Debug
            {
                case AuthError.InvalidEmail:
                    failedMessage += "Email is invalid!";
                    break;
                case AuthError.WrongPassword:
                    failedMessage += "Wrong Password";
                    break;
                case AuthError.MissingEmail:
                    failedMessage += "Email is missing!";
                    break;
                case AuthError.MissingPassword:
                    failedMessage += "Password is missing!";
                    break;
                default:
                    failedMessage += "Error";
                    break;
            }

            errorTextMessage.text = failedMessage;
            Debug.Log(failedMessage);
            #endregion
        }
        else
        {
            user = loginTask.Result.User; //Original: user = loginTask.Result;

            Debug.LogFormat("{0} You are Successfully logged in!", user.DisplayName);

            if(user.IsEmailVerified)// Check if email is vertified
            {
                SuccesfullyLoggedIn();
            }
            else
            {
                SendEmailForVerification();
            }
        }
    }
    #endregion

    #region Register
    
    public void Register()//For button
    {
        StartCoroutine(RegisterAsync(nameRegisterField.text.Trim(), emailRegisterField.text.Trim(), passwordRegisterField.text.Trim(), confirmPasswordRegisterField.text.Trim()));
    }

    private IEnumerator RegisterAsync(string name, string email, string password, string confirmPassword)
    {
        if(name == "")
        {
            Debug.LogError("User Name is empty!");
        }
        else if (!CheckIfNameHasEnoughLetters())
        {
            Debug.LogError("Username doesn't have enough or too many letters!" + nameRegisterField.text.Trim().Length);
        }
        else if (checkForSameName == true)
        {
            Debug.LogError("UserName is already being used!");
        }
        else if(email == "")
        {
            Debug.LogError("Email field is empty!");
        }
        //else if (CheckIfEmailExistsAsync(email).Result)
        //{
        //    Debug.LogError("Email already exists!");
        //}
        else if(passwordRegisterField.text != confirmPasswordRegisterField.text)
        {
            Debug.LogError("Password does not match!");
        }
        
        else//When nothing is wrong
        {
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
            yield return new WaitUntil(() => registerTask.IsCompleted); //waiting until registration done
            
            if(registerTask.Exception != null)// When something is wrong
            {
                #region Debugging
                Debug.LogError(registerTask.Exception);

                FirebaseException firebaseException = registerTask.Exception.GetBaseException() as FirebaseException;
                AuthError authError = (AuthError)firebaseException.ErrorCode;

                string failedMessage = "Register Failed! Because: ";

                switch (authError)//Debug
                {
                    case AuthError.InvalidEmail:
                        failedMessage += "Email is invalid!";
                        break;
                    case AuthError.WrongPassword:
                        failedMessage += "Wrong Password";
                        break;
                    case AuthError.MissingEmail:
                        failedMessage += "Email is missing!";
                        break;
                    case AuthError.MissingPassword:
                        failedMessage += "Password is missing!";
                        break;
                    case AuthError.WeakPassword:
                        failedMessage += "Password is too weak!";
                        break;
                    default:
                        failedMessage += "Registration Failed!";
                        break;
                }

                errorTextMessage.text = failedMessage;
                Debug.Log(failedMessage);
                #endregion
            }
            else//Registration Succes
            {
                user = registerTask.Result.User;

                UserProfile userProfile = new UserProfile { DisplayName = name };

                var updateProfileTask = user.UpdateUserProfileAsync(userProfile);

                FirstTimeCreation();

                yield return new WaitUntil(() => updateProfileTask.IsCompleted); //Waits until updating profile done

                if(updateProfileTask.Exception != null)// When something is wrong
                {
                    user.DeleteAsync();

                    #region Debugging
                    Debug.LogError(updateProfileTask.Exception);

                    FirebaseException firebaseException = updateProfileTask.Exception.GetBaseException() as FirebaseException;
                    AuthError authError = (AuthError)firebaseException.ErrorCode;

                    string failedMessage = "Profile update Failed! Because: ";

                    switch (authError)
                    {
                        case AuthError.InvalidEmail:
                            failedMessage += "Email is invalid!";
                            break;
                        case AuthError.WrongPassword:
                            failedMessage += "Wrong Password";
                            break;
                        case AuthError.MissingEmail:
                            failedMessage += "Email is missing!";
                            break;
                        case AuthError.MissingPassword:
                            failedMessage += "Password is missing!";
                            break;
                        default:
                            failedMessage += "File update Failed!";
                            break;
                    }

                    errorTextMessage.text = failedMessage;
                    Debug.Log(failedMessage);

                    #endregion
                }
                else //Checks if user is email verificated
                {
                    OnAccountCreationSucces.Invoke();
                    Debug.Log("Registration Sucessful Welcome " + user.DisplayName);
                    if(user.IsEmailVerified)
                    {
                        MainMenuUIManager.Instance.OpenLoginPanel();
                    }
                    else
                    {
                        SendEmailForVerification();
                    }
                }
            }
        }
    }
    #endregion

    #region CheckForSameName

    public async void CheckForSameName()//Name must be between 3-12 characters
    {
        if (CheckIfNameHasEnoughLetters())
        {
            await CheckForSameNameAsync();
        }
    }

    private async Task CheckForSameNameAsync()//Doesn't need await
    {
        try
        {
            checkForSameName = false;
            string nameToSearch = nameRegisterField.text.Trim();
            var rootReference = database.RootReference.Child("Users");
            await rootReference.GetValueAsync().ContinueWith(task =>
            {
                checkForSameName = task.Result.Child(nameToSearch).Exists;
            });

            //checkForSameName = rootReference.GetValueAsync().Result.Child(nameToSearch).Exists;
            if(checkForSameName)
            {
                Debug.LogError("UserName is already being used!");
            }
            #region OldCode
            //await rootReference.GetValueAsync().ContinueWith(task =>
            //{
            //    if (task.IsCompleted)
            //    {
            //        DataSnapshot dataSnapshot = task.Result;

            //        checkForSameName = dataSnapshot.Child(nameToSearch).Exists;
            //    }
            //});
            #endregion
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
    }

    #endregion

    #region EmailVerification

    public void SendEmailForVerification()
    {
        StartCoroutine(SendEmailForVerificationAsync());
    }

    private IEnumerator SendEmailForVerificationAsync()
    {
        if(user != null)
        {
            var sendEmailTask = user.SendEmailVerificationAsync();

            yield return new WaitUntil(() => sendEmailTask.IsCompleted);

            if(sendEmailTask.Exception != null)// When something is wrong
            {
                #region Debugging
                FirebaseException firebaseException = sendEmailTask.Exception.GetBaseException() as FirebaseException;
                AuthError error = (AuthError)firebaseException.ErrorCode;

                string errorMessage;
                switch (error)
                {
                    case AuthError.Cancelled:
                        errorMessage = "Email verification was canceled";
                        break;
                    case AuthError.TooManyRequests:
                        errorMessage = "Too many Requests";
                        break;
                    case AuthError.InvalidRecipientEmail:
                        errorMessage = "The email you entered";
                        break;
                    case AuthError.ExpiredActionCode:
                        errorMessage = "The email code has Expired";
                        break;
                    default:
                        errorMessage = "Unknown Error : Please try again later";
                        break;
                }
                #endregion

                errorTextMessage.text = errorMessage;
                MainMenuUIManager.Instance.ShowVerficationResponse(false, user.Email, errorMessage);
            }
            else
            {
                Debug.Log("Email has successfully sent");
                MainMenuUIManager.Instance.ShowVerficationResponse(true, user.Email, null);
            }
        }
    }

    #endregion

    public void SuccesfullyLoggedIn()
    {
        errorTextMessage.text = "";
        References.userName = user.DisplayName;
        SceneManager.LoadScene(gameSceneToGo);
    }

    public void OpenGameScene()
    {
        SceneManager.LoadScene(gameSceneToGo);
    }

    public async void FirstTimeCreation()
    {
        bool firstTime = true;
        var jsonFirstTime = JsonConvert.SerializeObject(firstTime);
        var jsonID = JsonConvert.SerializeObject(user.UserId);

        await database.RootReference.Child("Users").Child(nameRegisterField.text).Child("FirebaseSettings").Child("Newplayer").SetRawJsonValueAsync(jsonFirstTime);
        await database.RootReference.Child("Users").Child(nameRegisterField.text).Child("FirebaseSettings").Child("ID").SetRawJsonValueAsync(jsonID);
        // ADD 1X Only bool = true; To check if player logged in for first time then turn false.
    }

    #region CheckingEmailExist
    public async void CheckIfEmailExists()
    {
        await CheckIfEmailExistsAsync();
    }
    public async Task<bool> CheckIfEmailExistsAsync(string email = null)
    {
        if (email == null)
        {
            email = emailRegisterField.text;
        }
        print("Trying CheckIfEmailInUse: " + email);
        checkForSameEmail = false;

        try
        {
            await auth.FetchProvidersForEmailAsync(email).ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    print("Task Completed");
                    List<string> listOfEmails = task.Result.ToList();
                    print("List amount: " + listOfEmails.Count);
                    for (int i = 0; i < listOfEmails.Count; i++)
                    {
                        if (listOfEmails[i] == email)
                        {
                            checkForSameEmail = true;
                            if (allowDebug)
                            {
                                print(email + "Is used!");
                            }
                        }
                        
                    }
                }
            });
        }
        catch(Exception ex)
        {
            Debug.LogWarning(ex.ToString());
        }
        return checkForSameEmail;
    }
    #endregion

    private bool CheckIfNameHasEnoughLetters()
    {
        if (nameRegisterField.text.Trim().Length >= 3 && nameRegisterField.text.Trim().Length <= 12)
        {
            return true;
        }
        return false;
    }
}
