using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceList : MonoBehaviour
{
    public TetrominoData[] tetrominoes;

    public void Start()
    {
        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }
    }
}
