using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using System.IO;
public class authControler : MonoBehaviour
{

    public Text emailInput, passInput, userName;
    public GameObject UserComunication;
    private string message = "";

    private GameObject currentScreen;

    public GameObject WelcomeScreen;
    public GameObject LoginScreen;
    public GameObject firstUserScreen;

    StorageReference storageRef;


    private string currUserID;
    private string UserName;

    //Firebase.Auth.FirebaseAuth auth;
    
    Firebase.Auth.FirebaseUser user;

    private void Start()
    {
        
        DatabaseReference refrence = FirebaseDatabase.DefaultInstance.RootReference;
        storageRef = FirebaseStorage.DefaultInstance.RootReference;
        currentScreen = WelcomeScreen;
        try
        {
            WelcomeScreen.SetActive(true);

            LoginScreen.SetActive(false);
            firstUserScreen.SetActive(false);
        }
        catch
        {
        }

        //WelcomeScreen.SetActive(true);
    }


    public void Login()
    {
        FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(emailInput.text, passInput.text)
            .ContinueWith(task => {

                if (task.IsCanceled)
                {
                    Firebase.FirebaseException e = task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                    getErrorMessage((AuthError)e.ErrorCode);

                    return;
                }
                if (task.IsFaulted)
                {
                    Firebase.FirebaseException e = task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                    getErrorMessage((AuthError)e.ErrorCode);

                    return;
                }
                if (task.IsCompleted)
                {
                    Firebase.Auth.FirebaseUser newUser = task.Result;
                    Debug.LogFormat("User signed in successfully: {0} ({1})",
                        newUser.DisplayName, newUser.UserId);
                    message = "You succesfully logged in!";
                    currUserID = newUser.UserId;
                    UserName = newUser.DisplayName;
                }
            });
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            string name = user.DisplayName;
            string email = user.Email;
            System.Uri photo_url = user.PhotoUrl;
            // The user's Id, unique to the Firebase project.
            // Do NOT use this value to authenticate with your backend server, if you
            // have one; use User.TokenAsync() instead.
            string uid = user.UserId;
            Debug.Log("name: " + name + " email: " + email + " UserID: " + uid + " unique to firebase project");
        }
        
    }
    public void logout()
    {
        if(FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            FirebaseAuth.DefaultInstance.SignOut();
            message = "You have successfully logged out!";
        }
        else
        {
            message = "Nobody has logged in";
        }
        
    }

    public void registerUser()
    {

        if (emailInput.text.Equals("") && passInput.text.Equals(""))
        {
            Debug.Log("Error: No text in password or Email");
            //return;
        }
        FirebaseUser newUser = null;
        var RegisterTask = FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(emailInput.text, passInput.text).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Firebase.FirebaseException e = task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                getErrorMessage((AuthError)e.ErrorCode);
                Debug.Log("Canceled");

                return;
            }
            if (task.IsFaulted)
            {
                Firebase.FirebaseException e = task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                getErrorMessage((AuthError)e.ErrorCode);
                Debug.Log("faulted");
                return;
            }
            if (task.IsCompleted)
            {
                // Firebase user has been created.
                newUser = task.Result;
                Debug.Log("Success" + newUser.Email + " " + newUser.DisplayName);
                message = "You succesfully registered!";

                UserProfile profile = new UserProfile { DisplayName = userName.text };

                var profileTask = newUser.UpdateUserProfileAsync(profile).ContinueWith(task =>
                {
                    if (task.IsCanceled)
                    {
                        Firebase.FirebaseException e = task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                        getErrorMessage((AuthError)e.ErrorCode);
                        Debug.Log("Canceled username generation");

                        return;
                    }
                    if (task.IsFaulted)
                    {
                        Firebase.FirebaseException e = task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                        getErrorMessage((AuthError)e.ErrorCode);
                        Debug.Log("faulted username generation");
                        return;
                    }
                    if (task.IsCompleted)
                    {
                        Debug.Log("updated username: " + profile.DisplayName);

                        makeUserStorageDirectory(newUser.UserId + "/notImportant.txt");
                        message = "You succesfully registered!";
                    }
                });
            }

        });


        // Create a root reference
        

    }

    private void makeUserStorageDirectory(string directory)
    {
        StorageReference storageRef = FirebaseStorage.DefaultInstance.RootReference;

        StorageReference readmeRef = storageRef.Child(directory);

        string path = "notImportant.txt";

        File.WriteAllText(path, "This is a test file designed to make a directory in firebase using the Userid of the person.");


        // Upload the file to the path "images/rivers.jpg"
        readmeRef.PutFileAsync(path)
            .ContinueWith((task) => {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log(task.Exception.ToString());
            // Uh-oh, an error occurred!
        }
                else
                {
            // Metadata contains file metadata such as size, content-type, and download URL.
                    StorageMetadata metadata = task.Result;
                    string md5Hash = metadata.Md5Hash;
                    Debug.Log("Finished uploading...");
                    Debug.Log("File made in cloud!");
                }
            });
        File.Delete(path);


    }

    void getErrorMessage(AuthError errorCode)
    {
        string msg = "";

        msg = errorCode.ToString();
        Debug.Log(msg);

        message = msg;

    }

    private void Update()
    {
            if (UserComunication == null || UserComunication.active == false)
            {
                UserComunication = GameObject.FindWithTag("feedback");
                UserComunication.GetComponent<Text>().text = message;
            }


    }

    public void QuitButton()
    {
        Application.Quit();

    }

    public void goToLogin()
    {
        currentScreen.SetActive(false);
        currentScreen = LoginScreen;
        currentScreen.SetActive(true);

    }

    public void nextPageFromLogin()
    {
        currentScreen.SetActive(false);
        currentScreen = firstUserScreen;
        firstUserScreen.SetActive(true);
    }

    public void throwError()
    {
        Debug.LogError("A error has been encountered, please throw something at Reagan (i know this is going to come back to bite me)");
    }



    public void testvideoDownload()
    {

        var temp = storageRef.GetFileAsync("videoplayback.mp4").ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.Log("Error video not recieved");
            }
            else if(task.IsCompleted)
            {
                Debug.Log("Video recieved and able to be used");
            }
            else
            {
                throwError();
            }
        });

    }
}