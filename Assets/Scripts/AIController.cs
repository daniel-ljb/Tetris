using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class AIController : Board
{
    public NeuralNetwork neuralNetwork;
    private int movesWhileCantMoveDown;
    private int movesWithCurrentPiece;

    public AIController(NeuralNetwork neuralNetwork)
    {
        this.neuralNetwork = neuralNetwork;
    }

    public void SetNeuralNetwork(NeuralNetwork neuralNetwork)
    {
        this.neuralNetwork = neuralNetwork;
    }

    public void _Start()
    {
        nextPieces = new();
        gameRunning = true;
        if (pieceList == null)
        {
            pieceList = gameObject.GetComponentInChildren<PieceList>();
        }
        pieceList.Start();
        if (tilemap == null)
        {
            tilemap = gameObject.GetComponentInChildren<Tilemap>();
        }
        deadCellMap = new List<Tile[]>();
        for (int i = 0; i < 23; i++) // 3 Cells above top to allow pieces there
        {
            deadCellMap.Add(new Tile[10]);
        }

        SpawnPiece();
    }

    public string _Update()
    {
        string toReturn = "";
        if (gameRunning)
        {
            if (!CanMoveDown())
            {
                movesWhileCantMoveDown++;
                if (movesWhileCantMoveDown >= 5)
                {
                    HardDrop();
                    toReturn += "S";
                }
            }

            movesWithCurrentPiece++;
            score -= 1.5f;
            if (movesWithCurrentPiece >= 30)
            {
                HardDrop();
                toReturn += "O";
            }

            int input = GetAIInputs();
            if (input == 7)
            {
                if (Hold())
                {
                    toReturn += "H";
                }
            }
            if (input == 1)
            {
                currentPiece.MovePiece(Vector2Int.left);
                if (!CurrentPieceValid())
                {
                    currentPiece.MovePiece(Vector2Int.right);
                }
            }
            if (input == 2)
            {
                currentPiece.MovePiece(Vector2Int.right);
                if (!CurrentPieceValid())
                {
                    currentPiece.MovePiece(Vector2Int.left);
                }
            }
            if (input == 3)
            {
                RotatePiece(3);
            }
            if (input == 4)
            {
                RotatePiece(1);
            }
            if (input == 5)
            {
                SoftDrop();
            }
            if (input == 6)
            {
                HardDrop();
            }
            if (!gameRunning)
            {
                return $"{toReturn}{input}";
            }
            GUI();

            return $"{toReturn}{input}";
        }

        return "-1";
    }

    new private void HardDrop()
    {
        base.HardDrop();
        movesWithCurrentPiece = 0;
    }

    // left right z x down space c
    private int GetAIInputs()
    {
        //int input = (new int[] { 1, 2, 3, 4, 5, 6, 7 })[Random.Range(0, 7)];
        //return input;

        //bool[] inputs = new bool[7];
        //inputs[Random.Range(1, 7)] = true;

        float[] neuralNetworkInput = GetNeuralNetworkInput();
        float[] neuralNetworkOutput = neuralNetwork.CalculateValues(neuralNetworkInput);
        return 1 + Array.IndexOf(neuralNetworkOutput, neuralNetworkOutput.Max());
    }

    private float[] GetNeuralNetworkInput()
    {
        float[] neuralInput = new float[286];
        //   0 - 229, 23*10 dead cell map
        // 230 - 236, Held Piece
        // 237 - 243, Current Piece
        // 244 - 278, 7 per 5 next pieces
        // 279 Horizontal position
        // 280 Vertical position
        // 281 - 284 Rotation
        // 285 CanHold

        // deadCellMap
        int deadCellMapWidth = deadCellMap[0].Length;
        for (int i = 0; i < 230; i ++)
        {
            int row = i / deadCellMapWidth;
            int column = i % deadCellMapWidth;

            if (deadCellMap[row][column] != null)
            {
                neuralInput[i] = 1f;
            }
        }

        // Held
        if (heldPiece != null)
        {
            neuralInput[230 + PieceToNumber(heldPiece.tetrominoData.tetrominoName)] = 1f;
        }

        // Current
        neuralInput[237 + PieceToNumber(currentPiece.tetrominoData.tetrominoName)] = 1f;

        // Next
        for (int i = 0; i < 5; i ++)
        {
            neuralInput[244 + 7 * i + PieceToNumber(nextPieces[i].tetrominoData.tetrominoName)] = 1f;
        }

        // Position
        neuralInput[279] = (currentPiece.position.x + 4) / 9f;
        neuralInput[280] = (currentPiece.position.y + 10) / 22f;

        // Rotation
        neuralInput[281 + currentPiece.rotation] = 1f;

        // canHold
        if (canHold)
        {
            neuralInput[282] = 1f;
        }

        return neuralInput;
    }

    static int PieceToNumber(TetrominoName tetrominoName)
    {
        TetrominoName[] names = {
                TetrominoName.I,
                TetrominoName.T,
                TetrominoName.O,
                TetrominoName.J,
                TetrominoName.L,
                TetrominoName.S,
                TetrominoName.Z
                };

        return Array.IndexOf(names, tetrominoName);
    }
}
