using UnityEngine;
using UnityEngine.Tilemaps;

public class Piece
{
    public Vector2Int position;
    public int rotation;
    public TetrominoData tetrominoData;

    public Piece(TetrominoData _tetrominoData)
    {
        tetrominoData = _tetrominoData;
        position = tetrominoData.spawnLocation;
        rotation = 0;
    }

    public Piece(TetrominoData _tetrominoData, Vector2Int _position)
    {
        tetrominoData = _tetrominoData;
        position = _position;
        rotation = 0;
    }

    public Piece(TetrominoData _tetrominoData, int _rotation)
    {
        tetrominoData = _tetrominoData;
        position = tetrominoData.spawnLocation;
        rotation = _rotation;
    }

    public Piece(TetrominoData _tetrominoData, Vector2Int _position, int _rotation)
    {
        tetrominoData = _tetrominoData;
        position = _position;
        rotation = _rotation;
    }

    public void ResetPosition()
    {
        position = tetrominoData.spawnLocation;
    }

    public void MovePiece(Vector2Int vector)
    {
        position += vector;
    }

    public void RotatePiece(int amountToRotate)
    {
        // 1 = 90deg
        rotation += amountToRotate;
        rotation %= 4;
    }

    public Vector2Int[] Cells()
    {
        Vector2Int[] cells = new Vector2Int[tetrominoData.cells.Length];
        for (int i = 0; i < tetrominoData.cells.Length; i++)
        {
            cells[i] = position + tetrominoData.rotationCells[rotation, i];
        }
        return cells;
    }
}
