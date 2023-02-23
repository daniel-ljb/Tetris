using UnityEngine;
using Random = UnityEngine.Random;

public class Layer
{
    // weightTable contains all the weights and bias between the current layer and the previous layer
    // Each row corresponds to the weights from all previous node to a single node in the current row + the bias
    // e.g. If there are 2 nodes in the current layer and 2 in the previous layer:
    // 0.5 0.25 0.5
    // 0.1 0.8 0.7
    // This would mean that N1 = 0.5 * n1 + 0.25 * n2 + 0.5
    public float[,] weightTable;
    private int nodes;
    private int nodesOnPreviousLayer;
    public int numberOfWeights;

    public Layer(int nodes, int nodesOnPreviousLayer)
    {
        this.nodes = nodes;
        this.nodesOnPreviousLayer = nodesOnPreviousLayer;
        // node rows, previous layer + 1 columns
        weightTable = new float[nodes, nodesOnPreviousLayer + 1];
        numberOfWeights = nodes * (nodesOnPreviousLayer + 1);
        RandomiseWeights();
    }

    public Layer(int nodes, int nodesOnPreviousLayer, float[,] weightTable)
    {
        this.nodes = nodes;
        this.nodesOnPreviousLayer = nodesOnPreviousLayer;
        this.weightTable = weightTable;
    }

    public Layer(float[,] weightTable)
    {
        nodes = weightTable.GetLength(0);
        nodesOnPreviousLayer = weightTable.GetLength(1) - 1;
        this.weightTable = weightTable;
    }

    public override string ToString()
    {
        string repr = "[\n";
        for (int row = 0; row < nodes; row++)
        {
            repr += "\t[";
            for (int column = 0; column < nodesOnPreviousLayer; column++)
            {
                repr += weightTable[row, column].ToString();
                repr += ",\t";
            }
            repr += weightTable[row, nodesOnPreviousLayer].ToString();
            repr += "]\n";
        }
        repr += "]";

        return repr;
    }

    public void Mutate()
    {
        int row = Random.Range(0, nodes);
        int column = Random.Range(0, nodesOnPreviousLayer + 1);
        weightTable[row, column] = Random.Range(0, 1f);
    }

    private void RandomiseWeights()
    {
        for (int i = 0; i < weightTable.GetLength(0); i++)
        {
            for (int j = 0; j < weightTable.GetLength(1); j++)
            {
                weightTable[i, j] = Random.Range(0f, 1f);
            }
        }
    }

    public float[] CalculateNodeValues(float[] previousNodeValues)
    {
        float[] values = new float[nodes];
        for (int i = 0; i < nodes; i++)
        {
            // Start with bias
            float value = weightTable[i, nodesOnPreviousLayer];

            // For each previous nodes multiply by weight and add
            for (int j = 0; j < nodesOnPreviousLayer; j++)
            {
                value += weightTable[i, j] * previousNodeValues[j];
            }

            // Add to array
            values[i] = value;
        }

        return values;
    }

    public void SetWeight(int index, float value)
    {
        int row = index / (nodesOnPreviousLayer + 1);
        int column = index % (nodesOnPreviousLayer + 1);

        weightTable[row, column] = value;
    }

    private float GetWeight(int index)
    {
        int row = index / (nodesOnPreviousLayer + 1);
        int column = index % (nodesOnPreviousLayer + 1);

        return weightTable[row, column];
    }

    public static Layer CrossOver(Layer layer1, Layer layer2)
    {
        Layer newLayer = new Layer(layer1.nodes, layer1.nodesOnPreviousLayer);
        int crossOverPoint = Random.Range(0, layer1.numberOfWeights);

        for (int i = 0; i < layer1.numberOfWeights; i++)
        {
            if (i < crossOverPoint)
            {
                newLayer.SetWeight(i, layer1.GetWeight(i));
            }
            else
            {
                newLayer.SetWeight(i, layer2.GetWeight(i));
            }
        }

        return newLayer;
    }
}
