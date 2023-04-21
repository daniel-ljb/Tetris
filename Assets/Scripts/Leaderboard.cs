using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    public TMP_Text leaderboardText;
    public string leaderboardName;
    private List<int> scores = new();

    // Start is called before the first frame update
    void Start()
    {
        string destination = Application.persistentDataPath + "/" + leaderboardName + ".txt";
        StreamReader file = File.OpenText(destination);
        while (true)
        {
            string line = file.ReadLine();
            if (line == "end")
            {
                break;
            }
            scores.Add(int.Parse(line));
        }
        file.Close();

        scores.Sort();
        scores.Reverse();

        int length = Mathf.Min(10, scores.Count);
        string text = "Leaderboard\n";

        for (int i = 0; i < length; i++)
        {
            text += $"{i+1}. {scores[i]}\n";
        }

        leaderboardText.SetText(text);
    }

    public void AddScore(int score)
    {
        scores.Add(score);

        string destination = Application.persistentDataPath + "/" + leaderboardName + ".txt";
        StreamWriter file = File.CreateText(destination);
        for (int i = 0; i < scores.Count; i++)
        {
            file.WriteLine(scores[i]);
        }
        file.WriteLine("end");
        file.Close();
    }

}
