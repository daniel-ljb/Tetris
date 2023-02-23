using UnityEngine;

public class Trainer : MonoBehaviour
{
    public GameObject populationPrefab;
    public int populationSize = 128;
    public int[] layerSizes = new int[] { 286, 200, 200, 200, 200, 200, 7 };

    private void Start()
    {
        RunTrainer();
    }

    private void RunTrainer()
    {
        // Gen 1
        Population population = Instantiate(populationPrefab, transform).GetComponent<Population>();
        population.numberOfAis = populationSize;
        NeuralNetwork[] startingNeuralNetworks = new NeuralNetwork[populationSize];
        for (int i = 0; i < populationSize; i++)
        {
            startingNeuralNetworks[i] = new NeuralNetwork(layerSizes);
        }
        population._Start();
        population.SetNeuralNetworks(startingNeuralNetworks);
        population.RunPopulation();
    }
}
