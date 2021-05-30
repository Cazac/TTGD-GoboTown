using System;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

///////////////
/// <summary>
///     
/// The Serializer class is used to serizalie data and sotre it after the game is powered off or between scences
/// 
///     //Usage Examples
///     Serializer.Save<ExampleClass>(filenameWithExtension, exampleClass);
///     ExampleClass exampleClass = Serializer.Load<ExampleClass>(filenameWithExtension));
/// 
/// </summary>
///////////////

public class Serializer
{
    ///////////////////////////////////////////////////////////////// - Loading Files

    public static SaveFile Load(string filename)
    {
        //Create Directories, only trigger if they do not exist.
        Directory.CreateDirectory("Saves");
        Directory.CreateDirectory("Saves/Backups");

        //Check if save file is valid
        if (File.Exists("Saves/" + filename))
        {
            //Send all Data back converted
            //StreamReader streamReader = new StreamReader("Saves/" + filename);
            //return JsonUtility.FromJson<TC_Player>(streamReader.ReadToEnd());

            //Send all Data back converted
            return JsonUtility.FromJson<SaveFile>(File.ReadAllText("Saves/" + filename));
        }
        else
        {
            ////Debug.Log("Test Code: No Data Found");
            return null;
        }
    }

    ///////////////////////////////////////////////////////////////// - Saving Files

    public static void Save(string filename, SaveFile data)
    {
        //Create Dirceotries if they do not exist
        Directory.CreateDirectory("Saves");
        Directory.CreateDirectory("Saves/Backups");

        //Save File to a Json
        string jsonString = JsonUtility.ToJson(data, true);
        File.WriteAllText("Saves/" + filename, jsonString);

        //File.Encrypt; ??
    }

    /////////////////////////////////////////////////////////////////
}