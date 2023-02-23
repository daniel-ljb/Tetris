using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Population : MonoBehaviour
{
    public GameObject AIBoardPrefab;
    public int numberOfAis = 16;
    [HideInInspector] public bool running = true;
    private AIController[] AIBoards;
    private PieceList pieceList;
    private int width;
    private int height;
    private float[] scores;
    
    void Start()
    {
        NeuralNetwork n = new NeuralNetwork(new int[] { 12, 5, 7 });
        NeuralNetwork m = new NeuralNetwork(new int[] { 12, 5, 7 });
        NeuralNetwork o = NeuralNetwork.Crossover(n, m);
        Debug.Log(n.ToString());
        Debug.Log(m.ToString());
        Debug.Log(o.ToString());
        m.SaveToFile("hello");
        NeuralNetwork p = NeuralNetwork.LoadFromFile("hello");
        Debug.Log(p.ToString());

        pieceList = gameObject.GetComponentInChildren<PieceList>();
        running = true;

        AIBoards = new AIController[numberOfAis];

        width = (int)Mathf.Ceil(Mathf.Sqrt(numberOfAis / 2f)) * 2;
        height = (int)Mathf.Ceil(numberOfAis / (float)width);
        float offSetLastRow = (width - numberOfAis % width) % width;
        
        for (int i = 0; i < numberOfAis; i++)
        {
            int column = i % width;
            int row = i / width;
            int w = (int)((- width / 2f + column) * 20 + 10);
            int h = (int)((height / 2f - row) * 30 - 15);

            // If bottom row, centre
            if (row == height - 1)
            {
                w += (int)(offSetLastRow * 10);
            }
            
            GameObject board = Instantiate(AIBoardPrefab, new Vector3(w, h, 0), Quaternion.identity, transform);
            board.name = $"AI Board {i+1}";
            AIBoards[i] = board.GetComponent<AIController>();
            AIBoards[i].pieceList = pieceList;
        }

        // Camera
        Camera camera = Camera.main;
        camera.orthographicSize = height * 16;
    }

    private void _Update()
    {
        running = false;
        for (int i = 0; i < AIBoards.Length; i++)
        {
            AIBoards[i]._Update();
            if (!AIBoards[i].gameRunning)
            {
                AIBoards[i].gameObject.SetActive(false);
            }
            else
            {
                running = true;
            }
        }
    }

    private NeuralNetwork[] GenerateNextNeuralNetworks()
    {
        AIController[] sortedAIControllers = AIBoards;
        Array.Sort(sortedAIControllers,
            delegate (AIController x, AIController y) { return x.score.CompareTo(y.score); });

        List<NeuralNetwork> previousNeuralNetworks = new();
        
        for (int i = 0; i < AIBoards.Length / 2; i ++)
        {
            previousNeuralNetworks.Add(sortedAIControllers[i].neuralNetwork);
        }

        NeuralNetwork[] newNeuralNetworks = new NeuralNetwork[previousNeuralNetworks.Count * 2];

        for (int i = 0; i < newNeuralNetworks.Length; i += 2)
        {
            int randomIndex1 = Random.Range(0, previousNeuralNetworks.Count);
            NeuralNetwork neuralNetwork1 = previousNeuralNetworks[randomIndex1];
            previousNeuralNetworks.RemoveAt(randomIndex1);
            int randomIndex2 = Random.Range(0, previousNeuralNetworks.Count);
            NeuralNetwork neuralNetwork2 = previousNeuralNetworks[randomIndex2];
            previousNeuralNetworks.RemoveAt(randomIndex2);

            newNeuralNetworks[i] = NeuralNetwork.Crossover(neuralNetwork1, neuralNetwork2);
            newNeuralNetworks[i + 1] = NeuralNetwork.Crossover(neuralNetwork2, neuralNetwork1);
        }

        return newNeuralNetworks;
    }
}
