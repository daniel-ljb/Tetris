using System;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

public class Population2 : MonoBehaviour
{
    public GameObject AIBoardPrefab;
    private PieceList pieceList;
    private AIController2[] AIBoards;
    public int numberOfAis;
    private int width;
    private int height;
    float offSetLastRow;
    private int generationNumber = 1;
    private string generationBestWeights = string.Empty;
    private Transform currentGenerationParent;

    private void Start()
    {
        pieceList = gameObject.GetComponentInChildren<PieceList>();
        currentGenerationParent = new GameObject($"Generation {generationNumber}").transform;
        currentGenerationParent.SetParent(transform);

        AIBoards = new AIController2[numberOfAis];
        float[] startingWeights = new float[] {-5.96f, -10.89f, -6.45f, -162.41f, -32.3f, 18.22f, -4.44f, -1.24f};

        width = (int)Mathf.Ceil(Mathf.Sqrt(numberOfAis / 2f)) * 2;
        height = (int)Mathf.Ceil(numberOfAis / (float)width);
        offSetLastRow = (width - numberOfAis % width) % width;

        for (int i = 0; i < numberOfAis; i++)
        {
            int column = i % width;
            int row = i / width;
            int w = (int)((-width / 2f + column) * 20 + 10);
            int h = (int)((height / 2f - row) * 30 - 15);

            // If bottom row, centre
            if (row == height - 1)
            {
                w += (int)(offSetLastRow * 10);
            }

            GameObject board = Instantiate(AIBoardPrefab, new Vector3(w, h, 0), Quaternion.identity, currentGenerationParent);
            board.name = $"AI Board {i + 1}";
            AIBoards[i] = board.GetComponent<AIController2>();
            AIBoards[i].pieceList = pieceList;

            float[] boardWeights = new float[startingWeights.Length];
            for (int j = 0; j < startingWeights.Length; j++)
            {
                boardWeights[j] = startingWeights[j] * Random.Range(0.95f, 1.05f);
            }
            AIBoards[i].SetWeights(boardWeights);
        }

        Camera.main.orthographicSize = height * 16;
    }

    private void Update()
    {
        bool finishedGeneration = true;
        foreach (var aiBoard in AIBoards)
        {
            if (aiBoard.gameRunning)
            {
                finishedGeneration = false;
                break;
            }
        }

        if (finishedGeneration)
        {
            finishedGeneration = false;
            generationNumber++;
            currentGenerationParent = new GameObject($"Generation {generationNumber}").transform;
            currentGenerationParent.SetParent(transform);

            AIController2[] sortedAIControllers = AIBoards;
            Array.Sort(sortedAIControllers,
                delegate (AIController2 x, AIController2 y) { return y.score.CompareTo(x.score); });

            AIController2[] newAIBoards = new AIController2[numberOfAis];
            for (int i = 0; i < numberOfAis; i++)
            {
                int column = i % width;
                int row = i / width;
                int w = (int)((-width / 2f + column) * 20 + 10);
                int h = (int)((height / 2f - row) * 30 - 15);

                // If bottom row, centre
                if (row == height - 1)
                {
                    w += (int)(offSetLastRow * 10);
                }

                GameObject board = Instantiate(AIBoardPrefab, new Vector3(w, h, 0), Quaternion.identity, currentGenerationParent);
                board.name = $"AI Board G{generationNumber} N{i + 1}";
                newAIBoards[i] = board.GetComponent<AIController2>();
                newAIBoards[i].pieceList = pieceList;

                float[] boardWeights = AIBoards[i/2].GetWeights();
                for (int j = 0; j < 8; j++)
                {
                    boardWeights[j] *= Random.Range(0.985f, 1.015f);
                }
                newAIBoards[i].SetWeights(boardWeights);
            }

            generationBestWeights += $"{generationNumber} ";
            foreach (var weight in AIBoards[0].GetWeights())
            {
                generationBestWeights += $"{weight} ";
            }
            for (int i = 0; i < numberOfAis; i++)
            {
                generationBestWeights += $"{sortedAIControllers[i].score} ";
            }
            generationBestWeights += '\n';

            string destination = Application.persistentDataPath + "/bestAIs.txt";
            StreamWriter file = File.CreateText(destination);
            file.Write(generationBestWeights);
            file.Close();

            for (int i = 0; i < numberOfAis; i++)
            {
                AIBoards[i].gameObject.SetActive(false);
                AIBoards[i] = newAIBoards[i];
            }
        }
    }


}
