using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

public class NeuralNetwork
{
    private int numberOfLayers;
    private int[] layerSizes;
    public Layer[] layers;

    public NeuralNetwork(int[] layerSizes)
    {
        numberOfLayers = layerSizes.Length;
        this.layerSizes = layerSizes;
        layers = new Layer[numberOfLayers - 1];

        for (int i = 1; i < numberOfLayers; i++)
        {
            layers[i - 1] = new Layer(layerSizes[i], layerSizes[i - 1]);
        }
    }

    public void SaveToFile(string fileName)
    {
        string destination = Application.persistentDataPath + "/" + fileName + ".txt";

        StreamWriter file = File.CreateText(destination);

        file.WriteLine(ToString());
        file.Close();
    }

    public static NeuralNetwork LoadFromFile(string fileName)
    {
        string destination = Application.persistentDataPath + "/" + fileName + ".txt";
        StreamReader file = File.OpenText(destination);
        string[] firstLine = file.ReadLine().Split(" ");
        string type = firstLine[0];

        // Check type
        if (type != "Neural_Network")
        {
            return null;
        }

        // Find sizes
        int[] layerSizes = new int[(firstLine[1].Length + 1) / 2];
        string[] stringLayerSizes = firstLine[1].Split("_");
        
        for (int i = 0; i < layerSizes.Length; i++)
        {
            layerSizes[i] = int.Parse(stringLayerSizes[i]);
        }
        NeuralNetwork loadedNeuralNetwork = new NeuralNetwork(layerSizes);

        for (int i = 0; i < layerSizes.Length - 1; i++)
        {
            file.ReadLine(); // [

            float[,] weightTable = new float[layerSizes[i + 1], layerSizes[i] + 1];
            
            for (int j = 0; j < layerSizes[i + 1]; j++)
            {
                string line = file.ReadLine();
                int start = line.IndexOf('[');
                int end = line.IndexOf(']');
                string[] weights = line.Substring(start + 1, end - start - 1).Split(",\t");
                for (int k = 0; k < weights.Length; k++)
                {
                    weightTable[j, k] = float.Parse(weights[k]);
                }
            }
            loadedNeuralNetwork.layers[i] = new Layer(weightTable);

            file.ReadLine(); // ]
        }

        file.Close();
        return loadedNeuralNetwork;
    }

    public override string ToString()
    {
        string repr = "Neural_Network ";
        for (int i = 0; i < layerSizes.Length - 1; i++)
        {
            repr += $"{layerSizes[i]}_";
        }
        repr += $"{layerSizes[^1]} [\n";

        foreach (var layer in layers)
        {
            repr += layer.ToString().Replace("\n", "\n\t");
            repr += "\n";
        }

        repr += "]";
        return repr;
    }

    public float[] CalculateValues(float[] currentLayerValues) // Starts with the values of the input layer
    {
        foreach (var layer in layers)
        {
            currentLayerValues = layer.CalculateNodeValues(currentLayerValues);
        }

        return currentLayerValues;
    }

    public static NeuralNetwork Crossover (NeuralNetwork network1, NeuralNetwork network2)
    {
        int[] layerSizes = network1.layerSizes;
        NeuralNetwork newNetwork = new NeuralNetwork(layerSizes);

        int crossOverPointLayer = Random.Range(0, layerSizes.Length - 1);
        
        for (int i = 0; i < layerSizes.Length - 1; i++)
        {
            if (i < crossOverPointLayer)
            {
                newNetwork.layers[i] = network1.layers[i];
            }
            else if (i > crossOverPointLayer)
            {
                newNetwork.layers[i] = network2.layers[i];
            }
            else
            {
                newNetwork.layers[i] = Layer.CrossOver(network1.layers[i], network2.layers[i]);
            }
        }

        return newNetwork;
    }

    public void Mutate(int numberOfMutations = -1)
    {
        if (numberOfMutations == -1)
        {
            // Mutate 0.1% by default
            int numberOfWeights = 0;
            for (int i = 0; i < numberOfLayers - 1; i++)
            {
                numberOfWeights += (layerSizes[i] + 1) * layerSizes[i + 1];
            }
            
            numberOfMutations = (int)Mathf.Ceil(numberOfWeights * 0.001f);
        }

        for (int i = 0; i < numberOfMutations; i++)
        {
            int layerToMutate = Random.Range(0, numberOfLayers - 1);

            layers[layerToMutate].Mutate();
        }
    }
}
