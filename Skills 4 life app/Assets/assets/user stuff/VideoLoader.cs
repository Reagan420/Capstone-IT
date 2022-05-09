using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Firebase;
using Firebase.Storage;
using Firebase.Extensions;
using Firebase.Firestore;
public class VideoLoader : MonoBehaviour
{
    public VideoClip currentVideo;

    FirebaseStorage FBstorage;

    StorageReference storageRef;

    FirebaseFirestore db;
    Dictionary<string, object> user;
    [SerializeField]InputField username;
    [SerializeField] InputField email;
    [SerializeField] InputField nameToken;

    // Start is called before the first frame update
    void Start()
    {

        db = FirebaseFirestore.DefaultInstance;

        storageRef = FirebaseStorage.DefaultInstance.RootReference;


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void testvideoDownload() 
    {

        //db.Collection("Users").Document(nameToken.text).GetSnapshotAsync().ContinueWith


        /*
        var temp = storageRef.GetFileAsync("videoplayback.mp4").ContinueWith(task =>
        {
            if(task.IsCanceled || task.IsFaulted)
            {
                Debug.Log("Something broke");
            }
            else
            {
                Debug.Log("Its alive?");
            }
        });

        */
        /*
        // Fetch the download URL
        storageRef.GetDownloadUrlAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                Debug.Log("Download URL: " + task.Result);
                // ... now download the file via WWW or UnityWebRequest.
            }
            else if (task.IsFaulted)
            {
                Debug.Log("faulted");
            }
            else if (task.IsCanceled)
            {
                Debug.Log("cancel culture");
            }
        });
        */

        //currentVideo = temp;
        //storageRef.Child("videoplayback.mp4");
        //Debug.Log(temp);

        
    }
    

    /*
     * storageRef.Child("videoplayback.mp4").GetDownloadUrlAsync().
            ContinueWithOnMainThread(task =>
            {
                if(!task.IsFaulted && !task.IsCanceled)
                {
                    StartCoroutine(download)
                }
            });*/
}
