using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Extensions;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.Video;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;

public class authControler : MonoBehaviour
{

    public Text emailInput, passInput, userName;
    
    private string message = "";


    /// <summary>
    /// all screens for the menus in the app
    /// </summary>
    public GameObject UserComunication;
    private GameObject currentScreen;
    public GameObject WelcomeScreen;
    public GameObject LoginScreen;
    public GameObject firstUserScreen;
    public GameObject profileScreen;
    public GameObject shopScreen;
    public GameObject settingsScreen;
    public GameObject videoPlayerMenu;
    public GameObject mediaOptions;
    public GameObject playButton;
    public GameObject pauseButton;
    public GameObject avatarSelect;
    public VideoPlayer videoPlayer;

    /// <summary>
    /// firebase assets
    /// firebase assets
    /// </summary>
    FirebaseStorage storage;
    StorageReference storageReference;
    StorageReference storageRef;
    FirebaseAuth auth;
    FirebaseUser user;

    public string currUserID;

    public string UserName;

    public string[] videonames;//can potentially store 500 things

    bool finishedUpdatingtextfile = true;

   /// <summary>
   /// variables for the prefrences part of the script mostly
   /// </summary>
    public GameObject nickname;
    public GameObject Username;
    public GameObject email;
    public GameObject DOB;
    public GameObject pronoun;
    public GameObject favColour;
    public GameObject Intrests;
    public GameObject rhiEnabled;
    public GameObject timEnabled;
    public GameObject auslanEnabled;


    public string tempnickname;
    public string tempUsername;
    public string tempemail;
    public string tempDOB;
    public string temppronoun;
    public string tempfavColour;
    public string tempIntrests;
    public string preferedAvatar;
    public bool auslan;

    string prefs = "userPrefrences.txt";
    public int currUserStory;
    public string videoName;
    public GameObject videoPlayerScreen;


    public GameObject thisobj;

    public GameObject subtitles_text;
    public GameObject subCheckBox;

    //Firebase.Auth.FirebaseAuth auth;


    /// <summary>
    /// create initial storage refrences for future use and store in public variables
    /// these variables will mostly be public so that other scripts can access them
    /// </summary>
    private void Start()
    {

        DatabaseReference refrence = FirebaseDatabase.DefaultInstance.RootReference;
        storageRef = FirebaseStorage.DefaultInstance.RootReference;
        //currentScreen = WelcomeScreen;
        try
        {
            WelcomeScreen.SetActive(false);
            LoginScreen.SetActive(true);
            firstUserScreen.SetActive(false);
            profileScreen.SetActive(false);
            shopScreen.SetActive(false);
            settingsScreen.SetActive(false);
            videoPlayerMenu.SetActive(false);
            mediaOptions.SetActive(false);
        }
        catch
        {
        }
        storageReference = FirebaseStorage.DefaultInstance.RootReference;
        //WelcomeScreen.SetActive(true);
    }

    /// <summary>
    /// is used to provide user feedback when a error occurs in a generic text box
    /// onlky finds active text boxes and doesnt need to work for the sript to run
    /// </summary>
    void Update()
    {
        try//try to display user feedback
        {
            if (UserComunication == null || UserComunication.active == false)
            {
                UserComunication = GameObject.FindWithTag("feedback");
                UserComunication.GetComponent<Text>().text = message;
            }
        }
        catch//if fails then dont
        {

        }

    }

    /// <summary>
    /// allow users to log in
    /// requisite: account must exist in the firebase database
    /// </summary>
    public void Login()
    {
        FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(emailInput.text, passInput.GetComponentInParent<InputField>().text)//sign in the user
            .ContinueWith(task => {

                if (task.IsCanceled)//if a problem is encountered then end the script here
                {
                    Firebase.FirebaseException e = task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;//return the exeption generated for user feedback

                    getErrorMessage((AuthError)e.ErrorCode);//retrieve the error message

                    return;
                }
                if (task.IsFaulted)
                {
                    Firebase.FirebaseException e = task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;//return the exeption generated for user feedback

                    getErrorMessage((AuthError)e.ErrorCode);//retrieve the error message

                    return;
                }
                if (task.IsCompleted)
                {
                    FirebaseUser newUser = task.Result;
                    Debug.LogFormat("User signed in successfully: {0} ({1})",
                        newUser.DisplayName, newUser.UserId);
                    
                    currUserID = newUser.UserId;//set global variables to apropreate values for the rest of the session while the user is loged in
                    UserName = newUser.DisplayName;
                }
            });
        auth = FirebaseAuth.DefaultInstance;//these 2 lines cannot be shortened... idfk...
        user = auth.CurrentUser;
        if (user != null)//while there is a logged in user
        {
            string name = user.DisplayName;
            string email = user.Email;
            System.Uri photo_url = user.PhotoUrl;
            // The user's Id, unique to the Firebase project.
            // Do NOT use this value to authenticate with your backend server, if you
            // have one; use User.TokenAsync() instead.
            string uid = user.UserId;
            Debug.Log("name: " + name + " email: " + email + " UserID: " + uid + " unique to firebase project");//used for dev debugging in development for displaying user id mostly
        }
        LoginScreen.SetActive(false);//auto change from loggin screen and go to the users library 
        //currentScreen = firstUserScreen;//change current screen
        firstUserScreen.SetActive(true);//display current screen
        updateUserstories(currUserID);//retrieve user stories document from firebase and scan it for the users stories
    }

    /// <summary>
    /// logout the user
    /// requisite: must be logged in
    /// </summary>
    public void logout()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            auth.SignOut();
            message = "You have successfully logged out!";
        }
        else
        {
            message = "Nobody has logged in";
        }
        
        if(profileScreen.activeSelf == true)
        {
            profileScreen.SetActive(false);
            LoginScreen.SetActive(true);
        }
    }

    /// <summary>
    /// create user in firebase
    /// requisite: user must input a username, password and email to be associated with their email
    /// </summary>
    public void registerUser()
    {

        if (emailInput.text.Equals("") && passInput.text.Equals(""))
        {
            Debug.Log("Error: No text in password or Email");
            //return;
        }
        FirebaseUser newUser = null;
        var RegisterTask = FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(emailInput.text, passInput.GetComponentInParent<InputField>().text).ContinueWith(task =>
        {
            if (task.IsCanceled)//problem with making new user
            {
                Firebase.FirebaseException e = task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                getErrorMessage((AuthError)e.ErrorCode);
                //Debug.Log("Canceled");

                return;
            }
            if (task.IsFaulted)//problem with making new user
            {
                Firebase.FirebaseException e = task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                getErrorMessage((AuthError)e.ErrorCode);
                //Debug.Log("faulted");
                return;
            }
            if (task.IsCompleted)
            {
                // Firebase user has been created.
                newUser = task.Result;
                Debug.Log("Success" + newUser.Email + " " + newUser.DisplayName);
                message = "You succesfully registered!";

                UserProfile profile = new UserProfile { DisplayName = userName.text };
                //SendIntroEmails(newUser.Email);//try to send a email
               
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

                        makeUserStorageDirectory(newUser.UserId + "/" + prefs);//upload default prefrences as a default file to make the directory
                        message = "You succesfully registered!";
                        
                    }
                });
            }

        });
    }



    /// <summary>
    /// this is all working right up to: smtpServer.Send(mail);
    /// i have no idea whats going wrong and i dont have time to fix it. 
    /// its not even giving me a error :'( 
    /// </summary>
    /// <param name="useremail"></param>
    void SendIntroEmails(string useremail)
    {
        Debug.Log("starting sending email");
        MailMessage mail = new MailMessage();
        mail.From = new MailAddress("skills4lifefirebase@gmail.com");
        mail.To.Add(useremail);
        mail.Subject = "Account creation";
        mail.Body = "Hello!, \n Congratulations on making a new account in the skills for life app. If you encounter any problems please message this email adress";
        SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
        smtpServer.Timeout = 10000;
        smtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
        smtpServer.UseDefaultCredentials = false;
        smtpServer.Port = 587;//GIVE CORRECT PORT HERE
        Debug.Log("mid working");
        smtpServer.Credentials = new System.Net.NetworkCredential("skills4lifefirebase@gmail.com", "AdminFirebase") as ICredentialsByHost;
        Debug.Log("mid2 working");
        smtpServer.EnableSsl = true;
        Debug.Log("mid3 working");
        ServicePointManager.ServerCertificateValidationCallback =
        delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        { return true; };
        Debug.Log("mid3 working");
        smtpServer.Send(mail);
        Debug.Log("end working");
        mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
        if(mail.DeliveryNotificationOptions == DeliveryNotificationOptions.OnFailure)
        {
            Debug.Log("email failure");
        }
        //smtpServer.SendAsync(mail)
        Debug.Log("successfully sent email!!!");
    }
    private void makeUserStorageDirectory(string directory)
    {
        StorageReference storageRef = FirebaseStorage.DefaultInstance.RootReference;

        StorageReference readmeRef = storageRef.Child(directory);

        string path = prefs;//all videos users own

        File.WriteAllText(path, "");
        makeDefaultPrefrences();
        //StorageReference uploadref = storageRef.Child(currUserID + "/" + prefs);
        // Upload the file to the path "images/rivers.jpg"
        readmeRef.PutFileAsync(path)
            .ContinueWith((task) => {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log(task.Exception.ToString());
            // Uh-oh, an error occurred!
        }
                else if(task.IsCompleted)
                {
            // Metadata contains file metadata such as size, content-type, and download URL.//remove metadata for now
                   // StorageMetadata metadata = task.Result;
                    //string md5Hash = metadata.Md5Hash;
                    Debug.Log("Finished uploading...");
                    this.gameObject.GetComponent<uploadPrefrences>().setPrefrences();//make user prefrences once the directory is made
                }
            });

    }


    /// <summary>
    /// retrieve errormessage for display to user
    /// </summary>
    /// <param name="errorCode"></param>
    void getErrorMessage(AuthError errorCode)
    {
        string msg = "";

        msg = errorCode.ToString();
        Debug.Log(msg);

        message = msg;

    }

    /// <summary>
    /// quit
    /// </summary>
    public void QuitButton()
    {
        Application.Quit();

    }

    /// <summary>
    /// change current screen to login screen
    /// </summary>
    public void goToLogin()
    {   
        if(firstUserScreen.activeSelf == true)
        {
            firstUserScreen.SetActive(false);
        }
        if (settingsScreen.activeSelf == true)
        {
            settingsScreen.SetActive(false);
        }
        if (shopScreen.activeSelf == true)
        {
            shopScreen.SetActive(false);
        }
        if (WelcomeScreen.activeSelf == true)
        {
            WelcomeScreen.SetActive(false);
        }
        if (profileScreen.activeSelf == true)
        {
            profileScreen.SetActive(false);
        }
        if (videoPlayerMenu.activeSelf == true)
        {
            videoPlayerMenu.SetActive(false);
        }
        if (mediaOptions.activeSelf == true)
        {
            mediaOptions.SetActive(false);
        }
        if (avatarSelect.activeSelf == true)
        {
            avatarSelect.SetActive(false);
        }

    LoginScreen.SetActive(true);
    }

    /// <summary>
    /// change current screen to profile screen
    /// </summary>
    public void goToProfileScreen()
    {

        if (firstUserScreen.activeSelf == true)
        {
            firstUserScreen.SetActive(false);
        }
        if (settingsScreen.activeSelf == true)
        {
            settingsScreen.SetActive(false);
        }
        if (shopScreen.activeSelf == true)
        {
            shopScreen.SetActive(false);
        }
        if (WelcomeScreen.activeSelf == true)
        {
            WelcomeScreen.SetActive(false);
        }
        if (profileScreen.activeSelf == true)
        {
            profileScreen.SetActive(false);
        }
        if (videoPlayerMenu.activeSelf == true)
        {
            videoPlayerMenu.SetActive(false);
        }
        if (mediaOptions.activeSelf == true)
        {
            mediaOptions.SetActive(false);
        }
        if (avatarSelect.activeSelf == true)
        {
            avatarSelect.SetActive(false);
        }

        profileScreen.SetActive(true);
        getPrefrences();
    }

    //change current screen to library screen (sorry for the terrible method name)
    public void nextPageFromLogin()
    {

        if (firstUserScreen.activeSelf == true)
        {
            firstUserScreen.SetActive(false);
        }
        if (settingsScreen.activeSelf == true)
        {
            settingsScreen.SetActive(false);
        }
        if (shopScreen.activeSelf == true)
        {
            shopScreen.SetActive(false);
        }
        if (WelcomeScreen.activeSelf == true)
        {
            WelcomeScreen.SetActive(false);
        }
        if (profileScreen.activeSelf == true)
        {
            profileScreen.SetActive(false);
        }
        if (videoPlayerMenu.activeSelf == true)
        {
            videoPlayerMenu.SetActive(false);
        }
        if (mediaOptions.activeSelf == true)
        {
            mediaOptions.SetActive(false);
        }
        if (avatarSelect.activeSelf == true)
        {
            avatarSelect.SetActive(false);
        }

        firstUserScreen.SetActive(true);

        
        updateUserstories(currUserID);

    }

    /// <summary>
    /// go the screen which enables purchese for user
    /// </summary>
    public void goToShopScreen()
    {
        if (firstUserScreen.activeSelf == true)
        {
            firstUserScreen.SetActive(false);
        }
        if (settingsScreen.activeSelf == true)
        {
            settingsScreen.SetActive(false);
        }
        if (shopScreen.activeSelf == true)
        {
            shopScreen.SetActive(false);
        }
        if (WelcomeScreen.activeSelf == true)
        {
            WelcomeScreen.SetActive(false);
        }
        if (profileScreen.activeSelf == true)
        {
            profileScreen.SetActive(false);
        }
        if (videoPlayerMenu.activeSelf == true)
        {
            videoPlayerMenu.SetActive(false);
        }
        if (mediaOptions.activeSelf == true)
        {
            mediaOptions.SetActive(false);
        }
        if (avatarSelect.activeSelf == true)
        {
            avatarSelect.SetActive(false);
        }

        shopScreen.SetActive(true);

    }


    /// <summary>
    /// go the screen which enables purchese for user
    /// </summary>
    public void goToavatarSelectScreen()
    {
        if (firstUserScreen.activeSelf == true)
        {
            firstUserScreen.SetActive(false);
        }
        if (settingsScreen.activeSelf == true)
        {
            settingsScreen.SetActive(false);
        }
        if (shopScreen.activeSelf == true)
        {
            shopScreen.SetActive(false);
        }
        if (WelcomeScreen.activeSelf == true)
        {
            WelcomeScreen.SetActive(false);
        }
        if (profileScreen.activeSelf == true)
        {
            profileScreen.SetActive(false);
        }
        if (videoPlayerMenu.activeSelf == true)
        {
            videoPlayerMenu.SetActive(false);
        }
        if (mediaOptions.activeSelf == true)
        {
            mediaOptions.SetActive(false);
        }
        if (avatarSelect.activeSelf == true)
        {
            avatarSelect.SetActive(false);
        }

        avatarSelect.SetActive(true);

    }


    public void goToSettingsScreen()
    {
        if (firstUserScreen.activeSelf == true)
        {
            firstUserScreen.SetActive(false);
        }
        if (settingsScreen.activeSelf == true)
        {
            settingsScreen.SetActive(false);
        }
        if (shopScreen.activeSelf == true)
        {
            shopScreen.SetActive(false);
        }
        if (WelcomeScreen.activeSelf == true)
        {
            WelcomeScreen.SetActive(false);
        }
        if (profileScreen.activeSelf == true)
        {
            profileScreen.SetActive(false);
        }
        if (videoPlayerMenu.activeSelf == true)
        {
            videoPlayerMenu.SetActive(false);
        }
        if (mediaOptions.activeSelf == true)
        {
            mediaOptions.SetActive(false);
        }
        if (avatarSelect.activeSelf == true)
        {
            avatarSelect.SetActive(false);
        }

        settingsScreen.SetActive(true);
    }

    public void goToMediaScreen()
    {
        if (firstUserScreen.activeSelf == true)
        {
            firstUserScreen.SetActive(false);
        }
        if (settingsScreen.activeSelf == true)
        {
            settingsScreen.SetActive(false);
        }
        if (shopScreen.activeSelf == true)
        {
            shopScreen.SetActive(false);
        }
        if (WelcomeScreen.activeSelf == true)
        {
            WelcomeScreen.SetActive(false);
        }
        if (profileScreen.activeSelf == true)
        {
            profileScreen.SetActive(false);
        }
        if (videoPlayerMenu.activeSelf == true)
        {
            videoPlayerMenu.SetActive(false);
        }
        if (mediaOptions.activeSelf == true)
        {
            mediaOptions.SetActive(false);
        }
        if (avatarSelect.activeSelf == true)
        {
            avatarSelect.SetActive(false);
        }

        videoPlayerMenu.SetActive(true);
    }


    public void mediaOptionsToggle()
    {
        if (mediaOptions.activeSelf == false)
        {
            mediaOptions.SetActive(true);
        }
        else if (mediaOptions.activeSelf == true)
        {
            mediaOptions.SetActive(false);
        }

    }

    /// <summary>
    /// retrieve list of user stories from fire base and give user the user stories that they own
    /// </summary>
    /// <param name="userID"></param>
    public void updateUserstories(string userID)
    {
        GameObject[] userStories = GameObject.FindGameObjectsWithTag("story");
        //StorageReference uploadref = storageReference.Child(userID + "/" + filename);
        Debug.Log("File upload started!");

        var metadata = new MetadataChange();
        metadata.ContentType = "video/mp4";
        StreamWriter writer;

        storageReference.GetFileAsync("UserVideos.txt").ContinueWith(task =>//for some fucking magical reason this gets all of the files in all directories.
        //this is some fucking magic boys
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.Log("problems with updating user textfile");
            }
            else if (task.IsCompleted)
            {

                Debug.Log("making textfile");
                //txtfileRecieved = true;
                string path = "UserVideos.txt";
                writer = new StreamWriter(path, true);
                writer.WriteLine("Test");
                writer.Close();

                //StorageReference uploadrefTXTFirebase = storageReference.Child(userID + "/" + path);
                //StorageReference uploadrefTXTLocal = storageReference.Root;
                StorageReference uploadrefTXTLocal = storageReference.Child("UserVideos.txt");
                metadata.ContentType = "text/txt";
                // Delete the file
                uploadrefTXTLocal.DeleteAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log("File deleted successfully.");
                        uploadrefTXTLocal.PutFileAsync(path, metadata).ContinueWithOnMainThread((task) =>
                        {
                            if (task.IsCanceled || task.IsFaulted)
                            {
                                Debug.Log("error with uploading files");
                            }
                            else if (task.IsCompleted)
                            {
                                getNamesOfUserVideos();
                                Debug.Log("finished updating user files");
                            }
                        });
                    }
                    else if (task.IsCanceled || task.IsFaulted)
                    {
                        Debug.Log("File was not deleted");
                        // Uh-oh, an error occurred!
                    }
                });
            }
        });
    }

    /// <summary>
    /// once user stories has been retrieved and can be displayed itterate though the list of files and make 
    /// </summary>
    public void getNamesOfUserVideos()
    {
        storageReference.GetFileAsync("UserVideos.txt").ContinueWith(task =>//for some  magical reason this gets all of the files in all directories.
        //this is some fucking magic boys
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.Log("problems with updating user textfile");
            }
            else if (task.IsCompleted)
            {

            }
        });
        string path = "UserVideos.txt";
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        //Debug.Log(reader.ReadToEnd());
        string directorystring = reader.ReadToEnd();
        reader.Close();
        int numVideo = 0;//start on the first video name
        string[] tempVidNames = new string[8];

        int numCorrectVals = 0;

        for (int i = 0; i < directorystring.Length; i++)//for every letter
        {
            numCorrectVals = 0;
            //Debug.Log("0");
            for (int j = 0; j < currUserID.Length; j++)//check every letter after that if the currUserID matches
            {
                //Debug.Log("1");
               
                //Debug.Log(videonames);
                if ( i + j < directorystring.Length  && directorystring[i + j] == currUserID[j])//check if all the letters in the id match
                {
                    numCorrectVals += 1;
                    if (numCorrectVals == currUserID.Length-1)//have reached the end of the user id and we have found the correct ID
                    {
                        Debug.Log("i: " + i + "  j: " + j + "  idLen: " + currUserID.Length + "  totalLength: " + directorystring.Length);
                        int k = i + j + 3;//+2 because we have to seperate the / from the video name and we start 1 back because j = length-1
                        //exameple "EAySNv3oCfQ9jKy3MMYnnNKscwj1/UserVideos.txt",
                        // -> userid/videoname + ",
                        //Debug.Log("3");
                        string tempstring = "";
                        while (directorystring[k].ToString() != "\"" && directorystring[k + 1].ToString() != ",")
                        {
                            tempstring += directorystring[k];
                            k++;
                        }
                        Debug.Log(tempstring);
                        //Debug.Log("attempting to assign first video");
                        tempVidNames[numVideo] = tempstring;

                        //Debug.Log("first video has been assigned");
                        numVideo += 1;//place name in next video

                    }
                }

            }
        }
        

        videonames = tempVidNames;

        //can store as many user stories as nessasary
        GameObject[] userstories = GameObject.FindGameObjectsWithTag("story");
        
        ///this could be done in a for loop if a scroll bar and infinant amount of user stories are located
        userstories[0].transform.GetChild(2).gameObject.GetComponent<Text>().text = videonames[0];
        userstories[1].transform.GetChild(2).gameObject.GetComponent<Text>().text = videonames[1];
        userstories[2].transform.GetChild(2).gameObject.GetComponent<Text>().text = videonames[2];
        userstories[3].transform.GetChild(2).gameObject.GetComponent<Text>().text = videonames[3];
    }



    /// <summary>
    /// find which user story is which and assign user stories to them
    /// </summary>
    public void playVideoButton()
    {
        string uid = user.UserId;
        string fileName = videoName;
        if (currUserStory == 1)
        {
            fileName = GameObject.FindGameObjectWithTag("US1TXT").GetComponent<Text>().text;

        }
        if (currUserStory == 2)
        {
            fileName = GameObject.FindGameObjectWithTag("US2TXT").GetComponent<Text>().text;
        }
        if (currUserStory == 3)
        {
            fileName = GameObject.FindGameObjectWithTag("US3TXT").GetComponent<Text>().text;
        }
        if (currUserStory == 4)
        {
            fileName = GameObject.FindGameObjectWithTag("US4TXT").GetComponent<Text>().text;
        }
        //once video is selected change screen and display the video
        goToMediaScreen();

        testvideoDownload(uid, fileName);
        
    }

    /// <summary>
    /// use the first video that corisponds to the first user story
    /// </summary>
    public void US1press()
    {
        currUserStory = 1;
        playVideoButton();
    }

    /// <summary>
    /// play second video
    /// </summary>
    public void US2press()
    {
        currUserStory = 2;
        playVideoButton();
    }

    /// <summary>
    /// play 3rd user story
    /// </summary>
    public void US3press()
    {
        currUserStory = 3;
        playVideoButton();
    }

    /// <summary>
    /// play 4th user story
    /// </summary>
    public void US4press()
    {
        currUserStory = 4;
        playVideoButton();
    }


    /// <summary>
    /// download the video the user wants to play.
    /// user id is needed to find what videos the user has and to make sure the videos are looked for in the right location
    /// file name is needed to determine exactly what video needs to be retrieved
    /// </summary>
    /// <param name="userID"></param>
    /// <param name="filename"></param>
    public void testvideoDownload(string userID, string filename)
    {
        storage = FirebaseStorage.DefaultInstance;
        storageReference = FirebaseStorage.DefaultInstance.RootReference;
        StorageReference uploadref = storageReference.Child(userID + "/" + filename);

        storageRef.GetFileAsync(filename).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.Log("Error video not recieved");
            }
            else if(task.IsCompleted)
            {
                Debug.Log("Video recieved and able to be used");
            }
        });

        playVideo(filename);//once video has been downloaded, play video for the user by displaying the video
    }


    public void playVideo(string filepath)
    {
        // Will attach a VideoPlayer to the main camera.
        videoPlayerScreen = GameObject.FindGameObjectWithTag("Video Player");

        // VideoPlayer automatically targets the camera backplane when it is added
        // to a camera object, no need to change videoPlayer.targetCamera.
        var videoPlayer = videoPlayerScreen.GetComponent<VideoPlayer>();

        //var rendertex = videoPlayerScreen.AddComponent<MeshRenderer>();

        // Play on awake defaults to true. Set it to false to avoid the url set
        // below to auto-start playback since we're in Start().
        videoPlayer.playOnAwake = false;

        // By default, VideoPlayers added to a camera will use the far plane.
        // Let's target the near plane instead.
        videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.RenderTexture;
        //videoPlayer.targetMaterialRenderer = rendertex;

        // This will cause our Scene to be visible through the video being played.
        videoPlayer.targetCameraAlpha = 0.5F;

        // Set the video to play. URL supports local absolute or relative paths.
        // Here, using absolute.
        videoPlayer.url = filepath;

        // Skip the first 100 frames.
        //videoPlayer.frame = 100;

        // Restart from beginning when done.
        videoPlayer.isLooping = false;

        // Each time we reach the end, we slow down the playback by a factor of 10.
        //videoPlayer.loopPointReached += EndReached;

        // Start playback. This means the VideoPlayer may have to prepare (reserve
        // resources, pre-load a few frames, etc.). To better control the delays
        // associated with this preparation one can use videoPlayer.Prepare() along with
        // its prepareCompleted event.
        videoPlayer.Play();
        videoPlayer.playOnAwake = true;
    }

   



    public void makeDefaultPrefrences()
    {
        setdefaultprefVariables();//set all prefrences to "default" to indicate that they are dummy variables
        makedefaultprefrencesFile();//once the prefrences have been generated, make them into a text file
        StorageReference uploadref = storageRef.Child(currUserID + "/" + prefs);//get the location this file needs to be sent to
        Debug.Log("uploaded prefrences");
    }

    /// <summary>
    /// set all prefrences to default
    /// </summary>
    private void setdefaultprefVariables()
    {
        tempnickname = "Nickname: Default";
        tempUsername = "Username: Default";
        tempemail = "Email: Default";
        tempDOB = "DOB: Default";
        temppronoun = "Pronoun: Default";
        tempfavColour = "Favourite colour: Default";
        tempIntrests = "Intrests: Default";
        preferedAvatar = "Prefered avatar: Rhiana";
        auslan = false;
    }

    /// <summary>
    /// get the prefrences file
    /// </summary>
    public void getPrefrences()
    {
        StorageReference uploadref = storageRef.Child(currUserID + "/" + prefs);
        getFiles(prefs, uploadref);
    }

    /// <summary>
    /// once the prefrences have been implemented by the user, save them to a file and upload them
    /// </summary>
    public void setPrefrences()
    {
        StorageReference uploadref = storageRef.Child(currUserID + "/" + prefs);
        //setdefaultprefVariables();
        makeprefrencesFile();
        uploadFile(prefs, uploadref);
    }

    /// <summary>
    /// this method makes the prefrences file
    /// </summary>
    private void makeprefrencesFile()
    {

        StreamWriter writer;
        writer = new StreamWriter(prefs);
        writer.WriteLine(
             nickname.gameObject.GetComponent<Text>().text + "\n" +
             Username.gameObject.GetComponent<Text>().text + "\n" +
             email.gameObject.GetComponent<Text>().text + "\n" +
             DOB.gameObject.GetComponent<Text>().text + "\n" +
             pronoun.gameObject.GetComponent<Text>().text + "\n" +
             favColour.gameObject.GetComponent<Text>().text + "\n" +
             Intrests.gameObject.GetComponent<Text>().text + "\n"
            );
        writer.Close();


    }
    /// <summary>
    /// makes the default prefrences file for submission to firebase
    /// </summary>
    private void makedefaultprefrencesFile()
    {

        StreamWriter writer;
        writer = new StreamWriter(prefs);
        writer.WriteLine(
            "Nickname: Default \n" +
            "Username: Default \n" +
            "Email: Default \n" +
            "DOB: Default \n" +
            "Pronoun: Default \n" +
            "Favourite colour: Default \n" +
            "Intrests: Default \n" +
            "Prefered avatar: Rhiana \n" +
            "Auslan mode: False"
            );
        writer.Close();
    }

    /// <summary>
    /// take a generic file called filename from root directory  and 
    /// and submit it to firebase in the location specified
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="Location"></param>
    public void uploadFile(string filename, StorageReference Location)
    {

        Location.PutFileAsync(filename).ContinueWithOnMainThread((task) =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.Log("error with uploading files");
            }
            else if (task.IsCompleted)
            {
                Debug.Log("File uploaded successfully");
            }
            else
            {
                Debug.Log("idk what happened, not completed, canceled or faulted");
            }
        });

    }

    /// <summary>
    /// generatue auslan prefs and avatar prefs
    /// </summary>
    public void setAvatarPrefs()
    {
        StorageReference uploadref = storageRef.Child(currUserID + "/" + prefs);
        //setdefaultprefVariables();
        getFiles(prefs, uploadref, false);
        Thread.Sleep(1000);
        UpdateAuslanPrefs(prefs);
        Thread.Sleep(1000);
        Debug.Log("setting avatar prefs");
        uploadFile(prefs, uploadref);
        Thread.Sleep(1000);
    }

    


    /// <summary>
    /// allows the function to retrieve
    /// </summary>
    private void UpdateAuslanPrefs(string prefs1)
    {
        string[] arrLine = File.ReadAllLines(prefs1);

        if (rhiEnabled.GetComponent<Toggle>().isOn == true)
        {
            arrLine[arrLine.Length - 2] = "Rhiana";
            Debug.Log("rhi toggled");
        }
        else if (timEnabled.GetComponent<Toggle>().isOn == true)
        {
            arrLine[arrLine.Length - 2] = "Tim";
            Debug.Log("tim toggled");
        }

        if (auslanEnabled.GetComponent<Toggle>().isOn == true)
        {
            arrLine[arrLine.Length - 1] = "True";
            Debug.Log("aus on");
        }
        else
        {
            arrLine[arrLine.Length - 1] = "False";
            Debug.Log("aus off");
        }
        




        File.WriteAllLines(prefs, arrLine);
    }

    /// <summary>
    /// take a generic file called filename from location in firebase and 
    /// download it
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="Location"></param>
    public void getFiles(string filename, StorageReference Location, bool sendingBasePrefs = true)
    {

        Location.GetFileAsync( filename).ContinueWithOnMainThread((task) =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                if (task.IsFaulted)
                {
                    Debug.Log("error with downloading files");
                }
                else
                {
                    Debug.Log("file download canceled");
                }

            }
            else if (task.IsCompleted)
            {
                Debug.Log("File gotten successfully");

                if(sendingBasePrefs == true)
                {
                    var readText = File.ReadLines(prefs);
                    string[] lines = new string[10];
                    int i = 0;
                    foreach (string s in readText)
                    {
                        lines[i] = s;
                        i++;
                    }


                    //once the prefrences have been retrieved, put them where the user can see them and change them
                    nickname.gameObject.GetComponent<Text>().text = lines[0];
                    Username.gameObject.GetComponent<Text>().text = lines[1];
                    email.gameObject.GetComponent<Text>().text = lines[2];
                    DOB.gameObject.GetComponent<Text>().text = lines[3];
                    pronoun.gameObject.GetComponent<Text>().text = lines[4];
                    favColour.gameObject.GetComponent<Text>().text = lines[5];
                    Intrests.gameObject.GetComponent<Text>().text = lines[6];
                }
                else if (sendingBasePrefs == false)
                {

                }
             
            }
        });

    }

    public void playPause()
    {
        if (playButton.activeSelf == true)
        {
            videoPlayer.Pause();
            playButton.SetActive(false);
            pauseButton.SetActive(true);
        }
        else if (pauseButton.activeSelf == true)
        {
            videoPlayer.Play();
            playButton.SetActive(true);
            pauseButton.SetActive(false);
        }
    }
}