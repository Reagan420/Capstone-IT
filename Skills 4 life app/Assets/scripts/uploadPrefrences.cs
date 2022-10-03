using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SimpleFileBrowser;
using UnityEngine.UI;

using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Extensions;
public class uploadPrefrences : MonoBehaviour
{

	FirebaseStorage storage;

	StorageReference storageReference;

	StorageReference storageRef;

	public string userID;

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

	string prefs = "userPrefrences";


	// Start is called before the first frame update
	void Start()
    {
		DatabaseReference refrence = FirebaseDatabase.DefaultInstance.RootReference;
		storageRef = FirebaseStorage.DefaultInstance.RootReference;
	}

    // Update is called once per frame
    void Update()
    {
        
    }


	private void getUserID()
    {
		userID = this.gameObject.GetComponent<authControler>().currUserID;
    }
    public void makeDefaultPrefrences()
    {
		Debug.Log("0");
		getUserID();
		Debug.Log("1");
		setdefaultprefVariables();
		Debug.Log("2");
		makeprefrencesFile();
		Debug.Log("3");
		StorageReference uploadref = storageRef.Child(userID + "/" + prefs);
		Debug.Log("4");
		uploadFile(prefs, uploadref);
		Debug.Log("uploaded prefrences");
	}

	private void setdefaultprefVariables()
    {
		nickname.gameObject.GetComponent<Text>().text = "default";
	    Username.gameObject.GetComponent<Text>().text = "default";
		email.gameObject.GetComponent<Text>().text = "default";
		DOB.gameObject.GetComponent<Text>().text = "default";
		pronoun.gameObject.GetComponent<Text>().text = "default";
		favColour.gameObject.GetComponent<Text>().text = "default";
		Intrests.gameObject.GetComponent<Text>().text = "default";
	}

	public void getPrefrences()
    {
		getUserID();
		StorageReference uploadref = storageRef.Child(userID + "/" + prefs);
		getFiles(prefs, uploadref);

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

    public void setPrefrences()
    {
		getUserID();
		StorageReference uploadref = storageRef.Child(userID + "/" + prefs);
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


    public void uploadFile(string filename,  StorageReference Location)
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


	public string getFiles(string filename, StorageReference Location)
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
			}
		});
		return filename;

	}
}
