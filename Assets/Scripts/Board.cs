using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{
    public PieceList pieceList;
    public Tilemap tilemap;
    public int lockDelay = 30;
    public int nextView = 5;

    private Piece currentPiece;
    private List<Piece> nextPieces = new();
    private Piece heldPiece;
    private bool canHold = true;
    private List<Tile[]> deadCellMap;
    private float nextSoftDrop;
    private float leftHeldStart;
    private float rightHeldStart;
    private float nextHorizontalMove;
    private float gravityTimer;
    private float lastTimeMovedDown;

    public void Start()
    {
        pieceList = this.gameObject.GetComponentInChildren<PieceList>();
        deadCellMap = new List<Tile[]>();
        for (int i = 0; i < 20; i++)
        {
            deadCellMap.Add(new Tile[10]);
        }

        SpawnPiece();
        gravityTimer = Time.time + 0.95f;
    }

    public void Update()
    {
        HandleInputs();
        HandleGravity();
        HandleLockDelay();
        GUI();
        List<int> fullLines = GetFullLines();
        ClearLines(fullLines);
    }

    private void SpawnPiece()
    {
        canHold = true;
        if (currentPiece != null)
        {
            SetPiece();
        }
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

        currentPiece = nextPieces[0];
        nextPieces.RemoveAt(0);
        gravityTimer = Time.time + 0.95f;
    }

    private void SetPiece()
    {
        foreach (Vector2Int cell in currentPiece.Cells())
        {
            deadCellMap[cell.y + 10][cell.x + 5] = currentPiece.tetrominoData.tile;
        }
    }

    private void HandleInputs()
    {
        // Holding
        if (Input.GetKeyDown("c") && canHold)
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
        }

        // Horizontal Movement
        Vector2Int horizontalMovement = GetHorizontalInput();
        currentPiece.MovePiece(horizontalMovement);
        if (!CurrentPieceValid())
        {
            currentPiece.MovePiece(-horizontalMovement);
        }

        // Rotate Piece
        int rotationAmount = GetRotationInput();
        RotatePiece(rotationAmount);

        // Vertical Movement
        if (Input.GetKeyDown("space"))
        {
            HardDrop();
        }
        else if (Input.GetKeyDown("down"))
        {
            SoftDrop();
            nextSoftDrop = Time.time + 3 / 60f;
        }
        else if (Input.GetKey("down"))
        {
            if (Time.time > nextSoftDrop)
            {
                SoftDrop();
                nextSoftDrop += 3 / 60f;
            }
        }
    }

    private void HandleGravity()
    {
        if (Time.time > gravityTimer)
        {
            bool movedDown = SoftDrop();
            if (movedDown)
            {
                gravityTimer += 0.95f;
            }
        }
    }

    private void HandleLockDelay()
    {
        if (Time.time - lastTimeMovedDown >= lockDelay / 60f
            && !CanMoveDown())
        {
            SpawnPiece();
        }
    }

    private Vector2Int GetHorizontalInput()
    {
        // 5 frames first held, 1 frame after. 30fps
        if (Input.GetKeyDown("left"))
        {
            leftHeldStart = Time.time;
            nextHorizontalMove = Time.time + 5 / 30f;
            return Vector2Int.left;
        }
        else if (Input.GetKeyDown("right"))
        {
            rightHeldStart = Time.time;
            nextHorizontalMove = Time.time + 5 / 30f;
            return Vector2Int.right;
        }
        
        else if (Time.time < nextHorizontalMove)
        {
            return Vector2Int.zero;
        }

        else if (Input.GetKey("left") && Input.GetKey("right"))
        {
            nextHorizontalMove += 1 / 30f;
            if (leftHeldStart > rightHeldStart)
            {
                return Vector2Int.left;
            }
            else
            {
                return Vector2Int.right;
            }
        }
        else if (Input.GetKey("left"))
        {
            nextHorizontalMove += 1 / 30f;
            return Vector2Int.left;
        }
        else if (Input.GetKey("right"))
        {
            nextHorizontalMove += 1 / 30f;
            return Vector2Int.right;
        }
        else
        {
            return Vector2Int.zero;
        }
    }

    private int GetRotationInput()
    {
        int rotationAmount = 0;
        if (Input.GetKeyDown("z"))
        {
            rotationAmount += 3;
        }
        if (Input.GetKeyDown("x"))
        {
            rotationAmount += 1;
        }

        return rotationAmount % 4;
    }

    private int GetWallKickTableIndex(int rotationAmount)
    {
        int wallKickTableIndex;
        int rotationAfter = (rotationAmount + currentPiece.rotation) % 4;

        if (currentPiece.rotation == 0 && rotationAfter == 3)
        {
            wallKickTableIndex = 7;
        }
        else if (currentPiece.rotation == 3 && rotationAfter == 0)
        {
            wallKickTableIndex = 6;
        }
        else
        {
            wallKickTableIndex = currentPiece.rotation * 2;
            if (rotationAfter < currentPiece.rotation)
            {
                wallKickTableIndex -= 1;
            }
        }

        return wallKickTableIndex;
    }

    private void RotatePiece(int rotationAmount)
    {
        if (rotationAmount == 0)
        {
            return;
        }

        // Rotate
        currentPiece.RotatePiece(rotationAmount);

        // Wall Kicks
        int wallKickTableIndex = GetWallKickTableIndex(rotationAmount);
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
            currentPiece.RotatePiece(-rotationAmount);
        }
    }

    private void HardDrop ()
    {
        while (true)
        {
            bool movedDown = SoftDrop();
            if (!movedDown)
            {
                SpawnPiece();
                return;
            }
        }
    }

    private bool SoftDrop()
    {
        if (CanMoveDown())
        {
            currentPiece.MovePiece(Vector2Int.down);
            gravityTimer += 3 / 60f;
            lastTimeMovedDown = Time.time;
            return true;
        }
        return false;
    }

    private bool CanMoveDown()
    {
        currentPiece.MovePiece(Vector2Int.down);
        bool canSoftDrop = CurrentPieceValid();
        currentPiece.MovePiece(Vector2Int.up);
        return canSoftDrop;
    }

    private void GUI()
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
                Vector3Int position = (Vector3Int)(cell - nextPieces[i].position + new Vector2Int(8, 8 - 4 * i));
                tilemap.SetTile(position, nextPieces[i].tetrominoData.tile);
            }
        }
    }

    private bool InvalidCell(Vector2Int position)
    {
        if (position.x < -5 || position.x > 4)
        {
            return true;
        }
        if (position.y < -10 || position.y > 9)
        {
            return true;
        }
        if (deadCellMap[position.y + 10][position.x + 5] != null)
        {
            return true;
        }
        return false;
    }

    private bool CurrentPieceValid()
    {
        foreach (Vector2Int cell in currentPiece.Cells())
        {
            if (InvalidCell(cell))
            {
                return false;
            }
        }
        return true;
    }

    private List<int> GetFullLines()
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

    private void ClearLines(List<int> linesToClear)
    {
        for (int i = linesToClear.Count - 1; i >= 0; i--)
        {
            deadCellMap.RemoveAt(linesToClear[i]);
            deadCellMap.Add(new Tile[10]);
        }
    }
}