using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

public class Trainer : MonoBehaviour
{
    public GameObject populationPrefab;
    public int populationSize = 128;
    public int[] layerSizes = new int[] { 286, 200, 200, 200, 200, 200, 7 };
    public bool running = false;
    private Population currentPopulation;
    private int currentGenerationNumber;
    private List<float> bestScores = new();

    private void Start()
    {
        RunTrainer();
    }

    private void RunTrainer()
    {
        // Gen 1
        Population population = Instantiate(populationPrefab, transform).GetComponent<Population>();
        currentPopulation = population;
        currentGenerationNumber = 1;
        population.numberOfAis = populationSize;
        NeuralNetwork[] startingNeuralNetworks = new NeuralNetwork[populationSize];
        for (int i = 0; i < populationSize; i++)
        {
            startingNeuralNetworks[i] = new NeuralNetwork(layerSizes);
        }
        population._Start();
        population.SetNeuralNetworks(startingNeuralNetworks);
        population.RunPopulation();
        running = true;
    }

    private void Update()
    {
        if (running)
        {
            if (currentPopulation.finished)
            {
                running = false;
            }
        }
        else
        {
            NeuralNetwork[] nextGenerationNeuralNetworks = currentPopulation.GenerateNextNeuralNetworks();
            float bestScore = Mathf.Max(currentPopulation.GetScores());
            bestScores.Add(bestScore);
            NeuralNetwork bestNeuralNetwork = currentPopulation.GetBestNeuralNetwork();
            bestNeuralNetwork.SaveToFile($"Best from gen {currentGenerationNumber}");
            Destroy(currentPopulation.gameObject);
            currentPopulation = Instantiate(populationPrefab, transform).GetComponent<Population>();
            currentPopulation.numberOfAis = populationSize;
            currentPopulation._Start();
            currentPopulation.SetNeuralNetworks(nextGenerationNeuralNetworks);
            currentPopulation.RunPopulation();
            currentGenerationNumber++;
            running = true;
        }
    }
}
