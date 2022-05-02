using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Firebase;
using Firebase.Storage;
using Firebase.Extensions;

public class VideoLoader : MonoBehaviour
{
    public VideoClip currentVideo;

    FirebaseStorage FBstorage;

    StorageReference storageRef;

    // Start is called before the first frame update
    void Start()
    {

        storageRef = FirebaseStorage.DefaultInstance.RootReference;


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void testvideoDownload() 
    {
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

        //currentVideo = temp;
        //storageRef.Child("videoplayback.mp4");
        Debug.Log(temp);

        
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
