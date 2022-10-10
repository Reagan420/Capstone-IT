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

public class authControler : MonoBehaviour
{

    public Text emailInput, passInput, userName;
    public GameObject UserComunication;
    private string message = "";

    private GameObject currentScreen;

    public GameObject WelcomeScreen;
    public GameObject LoginScreen;
    public GameObject firstUserScreen;
    public GameObject profileScreen;
    public GameObject shopScreen;
    public GameObject settingsScreen;
    public GameObject videoPlayerMenu;
    public GameObject mediaOptions;

    FirebaseStorage storage;

    StorageReference storageReference;

    StorageReference storageRef;


    public string currUserID;
    private string UserName;

    FirebaseAuth auth;
    FirebaseUser user;

    public string[] videonames;//can potentially store 500 things

    bool finishedUpdatingtextfile = true;
    bool mediaOptionsToggle = false;

    public GameObject videoPlayerScreen;
    string videoName;

    int currUserStory = 0;

    public GameObject thisobj;

    //Firebase.Auth.FirebaseAuth auth;

    //Firebase.Auth.FirebaseUser user;

    private void Start()
    {
        
        DatabaseReference refrence = FirebaseDatabase.DefaultInstance.RootReference;
        storageRef = FirebaseStorage.DefaultInstance.RootReference;
        currentScreen = WelcomeScreen;
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

    // Update is called once per frame
    void Update()
    {
        try
        {
            if (UserComunication == null || UserComunication.active == false)
            {
                UserComunication = GameObject.FindWithTag("feedback");
                UserComunication.GetComponent<Text>().text = message;
            }
        }
        catch
        {

        }
       
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
        auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;
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
        currentScreen.SetActive(false);
        currentScreen = firstUserScreen;
        currentScreen.SetActive(true);
        updateUserstories(currUserID);
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

                        makeUserStorageDirectory(newUser.UserId +"/"+ prefs);//upload default prefrences as a default file to make the directory
                        message = "You succesfully registered!";
                    }
                });
            }

        });


        // create user prefrences file
        



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
        readmeRef.PutFileAsync(path).ContinueWith((task) => {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log(task.Exception.ToString());
            // Uh-oh, an error occurred!
                }
                else if(task.IsCompleted)
                {
            // Metadata contains file metadata such as size, content-type, and download URL.
                   // StorageMetadata metadata = task.Result;
                    //string md5Hash = metadata.Md5Hash;
                    Debug.Log("Finished uploading...");
                }
        });
    }



    void getErrorMessage(AuthError errorCode)
    {
        string msg = "";

        msg = errorCode.ToString();
        Debug.Log(msg);

        message = msg;

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

    public void goToProfileScreen()
    {
        currentScreen.SetActive(false);
        currentScreen = profileScreen;
        currentScreen.SetActive(true);
        getPrefrences();
    }

    public void nextPageFromLogin()
    {
        currentScreen.SetActive(false);
        currentScreen = firstUserScreen;
        currentScreen.SetActive(true);
        
        updateUserstories(currUserID);

    }

    public void goToShopScreen()
    {
        currentScreen.SetActive(false);
        currentScreen = shopScreen;
        currentScreen.SetActive(true);
    }

    public void goToSettingsScreen()
    {
        currentScreen.SetActive(false);
        currentScreen = settingsScreen;
        currentScreen.SetActive(true);
    }

    public void mediaOptionsOn()
    {
        if (mediaOptionsToggle = false)
        {
            mediaOptions.SetActive(true);
            mediaOptionsToggle = true;
        }
        else if (mediaOptionsToggle = true)
        {
            mediaOptions.SetActive(false);
            mediaOptionsToggle = false;
        }

    }

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

    public void getNamesOfUserVideos()
    {
        storageReference.GetFileAsync("UserVideos.txt").ContinueWith(task =>//for some fucking magical reason this gets all of the files in all directories.
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
        
        Debug.Log("end of loop");

        videonames = tempVidNames;

        Debug.Log("loop finished");
        Debug.Log("video 1 is: " + videonames[0]);
        Debug.Log("video 2 is: " + videonames[1]);
        Debug.Log("video 2 is: " + videonames[2]);

        GameObject[] userstories = GameObject.FindGameObjectsWithTag("story");
   
        userstories[0].transform.GetChild(2).gameObject.GetComponent<Text>().text = videonames[0];
        userstories[1].transform.GetChild(2).gameObject.GetComponent<Text>().text = videonames[1];
        userstories[2].transform.GetChild(2).gameObject.GetComponent<Text>().text = videonames[2];
        userstories[3].transform.GetChild(2).gameObject.GetComponent<Text>().text = videonames[3];
        Debug.Log("finished finding video names");

    }

    public void throwError()
    {
        Debug.LogError("A error has been encountered, please throw something at Reagan (i know this is going to come back to bite me)");
    }

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

        currentScreen.SetActive(false);
        currentScreen = videoPlayerMenu;
        currentScreen.SetActive(true);

       



        testvideoDownload(uid, fileName);
        
    }

    public void US1press()
    {
        currUserStory = 1;
        playVideoButton();
    }

    public void US2press()
    {
        currUserStory = 2;
        playVideoButton();
    }

    public void US3press()
    {
        currUserStory = 3;
        playVideoButton();
    }

    public void US4press()
    {
        currUserStory = 4;
        playVideoButton();
    }



    public void testvideoDownload(string userID, string filename)
    {
        storage = FirebaseStorage.DefaultInstance;
        storageReference = FirebaseStorage.DefaultInstance.RootReference;
        VideoClip video;
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
            else
            {
                throwError();
            }
        });

        playVideo(filename);
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

    public GameObject nickname;
    public GameObject Username;
    public GameObject email;
    public GameObject DOB;
    public GameObject pronoun;
    public GameObject favColour;
    public GameObject Intrests;

    public string tempnickname;
    public string tempUsername;
    public string tempemail;
    public string tempDOB;
    public string temppronoun;
    public string tempfavColour;
    public string tempIntrests;

    string prefs = "userPrefrences.txt";





    private void getUserID()
    {
        //currUserID = currUserID;//this entire section was originally in another script
    }
    public void makeDefaultPrefrences()
    {
        //Debug.Log("1");
        setdefaultprefVariables();
        //Debug.Log("2");
        makedefaultprefrencesFile();
        //Debug.Log("3");
        StorageReference uploadref = storageRef.Child(currUserID + "/" + prefs);
        //Debug.Log("4");
        //uploadFile(prefs, uploadref);
        Debug.Log("uploaded prefrences");
    }

    private void setdefaultprefVariables()
    {
        tempnickname = "default";
        tempUsername = "default";
        tempemail = "default";
        tempDOB = "default";
        temppronoun = "default";
        tempfavColour = "default";
        tempIntrests = "default";
    }

    public void getPrefrences()
    {
        getUserID();
        StorageReference uploadref = storageRef.Child(currUserID + "/" + prefs);
        getFiles(prefs, uploadref);


    }

    public void setPrefrences()
    {
        getUserID();
        StorageReference uploadref = storageRef.Child(currUserID + "/" + prefs);
        //setdefaultprefVariables();
        makeprefrencesFile();
        uploadFile(prefs, uploadref);
    }

    private void makeprefrencesFile()
    {

        StreamWriter writer;
        writer = new StreamWriter(prefs);
        writer.WriteLine(
            "nickname:" + nickname.gameObject.GetComponent<Text>().text + "\n" +
            "Username:" + Username.gameObject.GetComponent<Text>().text + "\n" +
            "email:" + email.gameObject.GetComponent<Text>().text + "\n" +
            "DOB:" + DOB.gameObject.GetComponent<Text>().text + "\n" +
            "pronoun:" + pronoun.gameObject.GetComponent<Text>().text + "\n" +
            "favColour:" + favColour.gameObject.GetComponent<Text>().text + "\n" +
            "Intrests:" + Intrests.gameObject.GetComponent<Text>().text + "\n"
            );
        writer.Close();


    }

    private void makedefaultprefrencesFile()
    {

        StreamWriter writer;
        writer = new StreamWriter(prefs);
        writer.WriteLine(
            "Default \n" +
            "Default \n" +
            "Default \n" +
            "Default \n" +
            "Default \n" +
            "Default \n" +
            "Default \n" 
            );
        writer.Close();


    }

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


    public void getFiles(string filename, StorageReference Location)
    {

        Location.GetFileAsync(filename).ContinueWithOnMainThread((task) =>
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
                Debug.Log("File uploaded successfully");


                var readText = File.ReadLines(prefs);
                string[] lines = new string[10];
                int i = 0;
                foreach (string s in readText)
                {
                    lines[i] = s;
                    i++;
                }


                nickname.gameObject.GetComponent<Text>().text = lines[0];
                Username.gameObject.GetComponent<Text>().text = lines[1];
                email.gameObject.GetComponent<Text>().text = lines[2];
                DOB.gameObject.GetComponent<Text>().text = lines[3];
                pronoun.gameObject.GetComponent<Text>().text = lines[4];
                favColour.gameObject.GetComponent<Text>().text = lines[5];
                Intrests.gameObject.GetComponent<Text>().text = lines[6];


                Debug.Log(nickname.gameObject.GetComponent<Text>().text + " \n");
                Debug.Log(Username.gameObject.GetComponent<Text>().text + " \n");
                Debug.Log(email.gameObject.GetComponent<Text>().text + " \n");
                Debug.Log(DOB.gameObject.GetComponent<Text>().text + " \n");
                Debug.Log(pronoun.gameObject.GetComponent<Text>().text + " \n");
                Debug.Log(favColour.gameObject.GetComponent<Text>().text + " \n");
                Debug.Log(Intrests.gameObject.GetComponent<Text>().text + " \n");
            }
        });

    }
}