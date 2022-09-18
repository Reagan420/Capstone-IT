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
		getUserID();
		setdefaultprefVariables();
		makeprefrencesFile();
		StorageReference uploadref = storageReference.Child(userID + "/");
		uploadFile(prefs, uploadref);
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
		StorageReference uploadref = storageReference.Child(userID + "/" + prefs);
		getFiles(prefs, uploadref);
		string readText = File.ReadAllText(prefs);

		nickname.gameObject.GetComponent<Text>().text = "";
		Username.gameObject.GetComponent<Text>().text = "";
		email.gameObject.GetComponent<Text>().text = "";
		DOB.gameObject.GetComponent<Text>().text = "";
		pronoun.gameObject.GetComponent<Text>().text = "";
		favColour.gameObject.GetComponent<Text>().text = "";
		Intrests.gameObject.GetComponent<Text>().text = "";
		for (int i = 0; i < readText.Length; i++)
        {
			///////////////////nickname
			if(readText[i].ToString() == "!" && readText[i+1].ToString() == "!")
            {
				i += 10;//skip the !!nickname: part of the prefrence

				while (readText[i].ToString() != "!" && readText[i+1].ToString() != "!")
                {
					tempnickname += readText[i];

					i++;
                }
				i += 2;
            }
			/////////////////username
			if (readText[i].ToString() == "@" && readText[i + 1].ToString() == "@")
			{
				i += 11;//skip the !!nickname: part of the prefrence

				while (readText[i].ToString() != "@" && readText[i + 1].ToString() != "@")
				{
					tempUsername += readText[i];

					i++;
				}
				i += 2;
			}
			/////////////////DOB
			if (readText[i].ToString() == "$" && readText[i + 1].ToString() == "$")
			{
				i += 6;//skip the !!nickname: part of the prefrence

				while (readText[i].ToString() != "$" && readText[i + 1].ToString() != "$")
				{
					tempDOB += readText[i];

					i++;
				}
				i += 2;
			}
			/////////////////pronoun
			if (readText[i].ToString() == "%" && readText[i + 1].ToString() == "%")
			{
				i += 10;//skip the !!nickname: part of the prefrence

				while (readText[i].ToString() != "%" && readText[i + 1].ToString() != "%")
				{
					temppronoun += readText[i];

					i++;
				}
				i += 2;
			}
			/////////////////favColour
			if (readText[i].ToString() == "^" && readText[i + 1].ToString() == "^")
			{
				i += 10;//skip the !!nickname: part of the prefrence

				while (readText[i].ToString() != "^" && readText[i + 1].ToString() != "^")
				{
					tempfavColour += readText[i];
					i++;
				}
				i += 2;
			}
			/////////////////Intrests
			if (readText[i].ToString() == "*" && readText[i + 1].ToString() == "*")
			{
				i += 10;//skip the !!nickname: part of the prefrence

				while (readText[i].ToString() != "*" && readText[i + 1].ToString() != "*")
				{
					tempIntrests += readText[i];
					i++;
				}
				i += 2;
			}
		}
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
		StorageReference uploadref = storageReference.Child(userID + "/" + prefs);
		setdefaultprefVariables();
		makeprefrencesFile();
		uploadFile(prefs, uploadref);
	}

	private string makeprefrencesFile()
    {

		StreamWriter writer;
		writer = new StreamWriter(prefs);
        writer.WriteLine(
			"!!nickname:" + nickname.gameObject.GetComponent<Text>().text + "!!" +
			"@@Username:" + Username.gameObject.GetComponent<Text>().text + "@@" +
			"##email:" + email.gameObject.GetComponent<Text>().text + "##" +
			"$$DOB:" + DOB.gameObject.GetComponent<Text>().text + "$$" +
			"%%pronoun:" + pronoun.gameObject.GetComponent<Text>().text + "%%" +
			"^^favColour:" + favColour.gameObject.GetComponent<Text>().text + "^^" +
			"**Intrests:" + Intrests.gameObject.GetComponent<Text>().text + "**" 
			);
        writer.Close();

		return prefs;

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
		});

    }


	public string getFiles(string filename, StorageReference Location)
	{

		Location.GetFileAsync(filename).ContinueWithOnMainThread((task) =>
		{
			if (task.IsCanceled || task.IsFaulted)
			{
				Debug.Log("error with uploading files");
			}
			else if (task.IsCompleted)
			{
				Debug.Log("File uploaded successfully");
			}
		});
		return filename;

	}
}
