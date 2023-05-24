using UnityEngine;
using UnityEditor;
using System.IO;

public static class LogFile
{
    static string path = "Assets/Resources/LogFile.txt";

    public static void InitLogFile()
    {
        File.CreateText(path).Dispose();
    }

    public static void WriteString(string s)
    {
        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(s);
        writer.Close();
    }


    static void ReadString()
    {

        string path = "Assets/Resources/test.txt";

        //Read the text from directly from the test.txt file

        StreamReader reader = new StreamReader(path);

        Debug.Log(reader.ReadToEnd());

        reader.Close();

    }

}

