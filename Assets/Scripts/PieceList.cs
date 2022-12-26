using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PieceList : MonoBehaviour
{
    public TetrominoData[] tetrominoes;
    public Tile ghostTile;

    public void Start()
    {
        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }
    }
}
