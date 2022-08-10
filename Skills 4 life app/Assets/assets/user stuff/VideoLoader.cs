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


    public void testvideoDownload() 
    {

        //db.Collection("Users").Document(nameToken.text).GetSnapshotAsync().ContinueWith


        
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

      
        
    }
    

}
