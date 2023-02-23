using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{
    public PieceList pieceList;
    public Tilemap tilemap;
    public int lockDelay = 30;
    public int nextView = 5;
    public TMP_Text scoreText;

    [HideInInspector] public float score;
    [HideInInspector] public int combo = -1;
    public Piece currentPiece;
    protected List<Piece> nextPieces = new();
    protected Piece heldPiece = null;
    protected bool canHold = true;
    protected List<Tile[]> deadCellMap;
    protected float nextSoftDrop;
    protected float leftHeldStart;
    protected float rightHeldStart;
    protected float nextHorizontalMove;
    protected float gravityTimer;
    protected float lastTimeMovedDown;
    public bool gameRunning;
    [HideInInspector] public List<TetrominoName> PieceHistory = new();
  

    // -------- Game ----------------------------------------------------------------------------------
    // ------------------------------------------------------------------------------------------------
    protected void SpawnPiece()
    {
        // Handle Next Queue
        if (currentPiece != null)
        {
            SetPiece();
        }
        //Debug.Log(nextPieces);
        //Debug.Log(nextView);
        while (nextPieces.Count < nextView + 1)
        {
            List<Piece> piecesToRandomise = new();
            for (int i = 0; i < pieceList.tetrominoes.Length; i++)
            {
                piecesToRandomise.Add(new Piece(pieceList.tetrominoes[i]));
            }

            while (piecesToRandomise.Count > 0)
            {
                int randomPieceIndex = Random.Range(0, piecesToRandomise.Count);
                nextPieces.Add(piecesToRandomise[randomPieceIndex]);
                piecesToRandomise.RemoveAt(randomPieceIndex);
            }
        }

        // Spawn Piece
        canHold = true;
        currentPiece = nextPieces[0];
        PieceHistory.Add(currentPiece.tetrominoData.tetrominoName);
        nextPieces.RemoveAt(0);
        gravityTimer = Time.time + 0.95f;

        // Gameover if invalid
        if (!CurrentPieceValid())
        {
            gameRunning = false;
        }
    }

    protected void SetPiece()
    {
        foreach (Vector2Int cell in currentPiece.Cells())
        {
            deadCellMap[cell.y + 10][cell.x + 5] = currentPiece.tetrominoData.tile;
        }
    }

    // ---- Rotation ------------------------------------------------------------------------------
    protected void RotatePiece(int rotationAmount)
    {
        // rotation amount * 90 = degrees CW

        if (rotationAmount == 0)
        {
            return;
        }

        // Wall Kicks
        int wallKickTableIndex = GetWallKickTableIndex(
            currentPiece.rotation,
            (currentPiece.rotation + rotationAmount) % 4);
        // Rotate
        currentPiece.RotatePiece(rotationAmount);

        
        bool canRotate = false;
        // For each wall kick position
        for (int i = 0; i < currentPiece.tetrominoData.wallKicks.GetLength(1); i++)
        {
            Vector2Int wallKick = currentPiece.tetrominoData.wallKicks[wallKickTableIndex, i];
            currentPiece.MovePiece(wallKick);

            if (CurrentPieceValid())
            {
                canRotate = true;
                break;
            }
            // If Invalid move back
            currentPiece.MovePiece(-wallKick);
        }

        if (!canRotate)
        {
            // If can't rotate, cancel rotation
            if (rotationAmount == 1)
            {
                currentPiece.RotatePiece(3);
            }
            else
            {
                currentPiece.RotatePiece(1);
            }
        }
    }

    protected int GetWallKickTableIndex(int rotationBefore, int rotationAfter)
    {
        int[][] wallKickTableIndexTable = new int[8][] {
            new int[2] { 0, 1 },
            new int[2] { 1, 0 },
            new int[2] { 1, 2 },
            new int[2] { 2, 1 },
            new int[2] { 2, 3 },
            new int[2] { 3, 2 },
            new int[2] { 3, 0 },
            new int[2] { 0, 3 }
        };

        int[] rotation = new int[2] { rotationBefore, rotationAfter };

        for (int i = 0; i < 8; i++)
        {
            if (rotation.SequenceEqual(wallKickTableIndexTable[i]))
            {
                return i;
            }
        }
        return 0; // unreachable!
    }

    // ---- Move down -----------------------------------------------------------------------------
    protected bool SoftDrop()
    {
        if (CanMoveDown())
        {
            currentPiece.MovePiece(Vector2Int.down);
            gravityTimer += 3 / 60f;
            lastTimeMovedDown = Time.time;
            score++;
            return true;
        }
        return false;
    }

    protected void HardDrop ()
    {
        while (true)
        {
            bool movedDown = SoftDrop();
            score++; // 2 score for hard drop so add 1 more
            if (!movedDown)
            {
                SpawnPiece();
                return;
            }
        }
    }

    protected bool CanMoveDown()
    {
        currentPiece.MovePiece(Vector2Int.down);
        bool canSoftDrop = CurrentPieceValid();
        currentPiece.MovePiece(Vector2Int.up);
        return canSoftDrop;
    }

    // ---- Hold ---------------------------------------------------------------------------------
    protected bool Hold()
    {
        if (canHold)
        {
            if (heldPiece == null)
            {
                heldPiece = currentPiece;
                currentPiece = null;
                SpawnPiece();
            }
            else
            {
                Piece temp = currentPiece;
                currentPiece = heldPiece;
                currentPiece.ResetPosition();
                heldPiece = temp;
            }
            canHold = false;
            return true;
        }
        return false;
    }

    // ---- Clear Lines --------------------------------------------------------------------------
    protected List<int> GetFullLines()
    {
        List<int> fullLines = new List<int>();
        for (int i = 0; i < 20; i++)
        {
            bool rowFull = true;
            foreach (var cell in deadCellMap[i])
            {
                if (cell == null)
                {
                    rowFull = false;
                    break;
                }
            }

            if (rowFull)
            {
                fullLines.Add(i);
            }
        }

        return fullLines;
    }

    protected void ClearLines(List<int> linesToClear)
    {
        for (int i = linesToClear.Count - 1; i >= 0; i--)
        {
            deadCellMap.RemoveAt(linesToClear[i]);
            deadCellMap.Add(new Tile[10]);
        }

        combo += 1;
        switch (linesToClear.Count)
        {
            case 0:
                combo = -1;
                break;
            case 1:
                score += 100;
                break;
            case 2:
                score += 300;
                break;
            case 3:
                score += 500;
                break;
            default:
                score += 800;
                break;
        }

        if (combo >= 1)
        {
            score += 50 * combo;
        }
    }

    // ---- Valid --------------------------------------------------------------------------------
    protected bool InvalidCell(Vector2Int position)
    {
        //Debug.Log(position);
        if (position.x < -5 || position.x > 4)
        {
            return true;
        }
        if (position.y < -10 || position.y > 12)
        {
            return true;
        }
        if (deadCellMap[position.y + 10][position.x + 5] != null)
        {
            return true;
        }
        return false;
    }

    public bool CurrentPieceValid()
    {
        foreach (Vector2Int cell in currentPiece.Cells())
        {
            //Debug.Log(cell);
            if (InvalidCell(cell))
            {
                return false;
            }
        }
        return true;
    }


    // ---- GUI ----------------------------------------------------------------------------------
    protected void GUI()
    {
        // Clear Grid
        tilemap.ClearAllTiles();

        // Dead Cells
        for (int x = 0; x < deadCellMap[0].GetLength(0); x++)
        {
            for (int y = 0; y < deadCellMap.Count; y++)
            {
                if (deadCellMap[y][x] != null)
                {
                    tilemap.SetTile(new Vector3Int(x - 5, y - 10, 0), deadCellMap[y][x]);
                }
            }
        }

        // Ghost
        Vector2Int[] ghostCells = GetGhostCells();
        foreach (var cell in ghostCells)
        {
            Vector3Int position = (Vector3Int)cell;
            tilemap.SetTile(position, pieceList.ghostTile);
        }

        // Current Piece
        foreach (var cell in currentPiece.Cells())
        {
            Vector3Int position = (Vector3Int)cell;
            tilemap.SetTile(position, currentPiece.tetrominoData.tile);
        }

        // Held Piece
        if (heldPiece != null)
        {
            foreach (var cell in heldPiece.Cells())
            {
                Vector3Int position = (Vector3Int)(cell - heldPiece.position + new Vector2Int(-8, 8));
                tilemap.SetTile(position, heldPiece.tetrominoData.tile);
            }
        }
        // Next Queue
        for (int i = 0; i < nextView; i++)
        {
            foreach (var cell in nextPieces[i].Cells())
            {
                Vector3Int position = (Vector3Int)
                    (cell                               // Starting position
                    - nextPieces[i].position            // Centers
                    + new Vector2Int(8, 8 - 4 * i));    // Moves to correct place

                tilemap.SetTile(position, nextPieces[i].tetrominoData.tile);
            }
        }

        if (scoreText == null)
        {
            scoreText = gameObject.GetComponentInChildren<TMP_Text>();
        }
        scoreText.SetText(score.ToString());
    }

    protected Vector2Int[] GetGhostCells()
    {
        int distance = 0;
        while (CanMoveDown())
        {
            currentPiece.MovePiece(Vector2Int.down);
            distance++;
        }
        Vector2Int[] ghostCells = currentPiece.Cells();
        for (int i = 0; i < distance; i++)
        {
            currentPiece.MovePiece(Vector2Int.up);
        }
        return ghostCells;
    }
}