using System.Collections.Generic;
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
    private Piece[] nextPieces;
    private Tile[,] deadCellMap;
    private float nextSoftDrop;
    private float leftHeldStart;
    private float rightHeldStart;
    private float nextHorizontalMove;
    private float gravityTimer;
    private float lastTimeMovedDown;

    public void Start()
    {
        pieceList = this.gameObject.GetComponentInChildren<PieceList>();
        deadCellMap = new Tile[10, 20];

        SpawnPiece();
        gravityTimer = Time.time + 0.95f;
    }

    public void Update()
    {
        Debug.Log(lastTimeMovedDown);
        HandleInputs();
        HandleGravity();
        HandleLockDelay();
        GUI();
    }

    private void SpawnPiece()
    {
        if (currentPiece != null)
        {
            SetPiece();
        }
     //   while (nextPieces.Length < nextView + 1)
       // {
            
       // }
        int randomPieceIndex = Random.Range(0, pieceList.tetrominoes.Length);
        currentPiece = new Piece(pieceList.tetrominoes[randomPieceIndex]);
        gravityTimer = Time.time + 0.95f;
    }

    private void SetPiece()
    {
        foreach (Vector2Int cell in currentPiece.Cells())
        {
            deadCellMap[cell.x + 5, cell.y + 10] = currentPiece.tetrominoData.tile;
        }
    }

    private void HandleInputs()
    {
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
        for (int x = 0; x < deadCellMap.GetLength(1); x++)
        {
            for (int y = 0; y < deadCellMap.GetLength(0); y++)
            {
                if (deadCellMap[y, x] != null)
                {
                    tilemap.SetTile(new Vector3Int(y - 5, x - 10, 0), deadCellMap[y, x]);
                }
            }
        }

        // Current Piece
        foreach (var cell in currentPiece.Cells())
        {
            Vector3Int position = (Vector3Int)cell;
            tilemap.SetTile(position, currentPiece.tetrominoData.tile);
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
        if (deadCellMap[position.x + 5, position.y + 10] != null)
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
}
