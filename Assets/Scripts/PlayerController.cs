using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class PlayerController : Board
{
    public void Start()
    {
        gameRunning = true;
        if (pieceList == null)
        {
            pieceList = this.gameObject.GetComponentInChildren<PieceList>();
        }
        pieceList.Start();
        if (tilemap == null)
        {
            tilemap = this.gameObject.GetComponentInChildren<Tilemap>();
        }
        deadCellMap = new List<Tile[]>();
        for (int i = 0; i < 23; i++) // 3 Cells above top to allow pieces there
        {
            deadCellMap.Add(new Tile[10]);
        }

        SpawnPiece();
        gravityTimer = Time.time + 0.95f;
    }

    public void Update()
    {
        Debug.Log(currentPiece.position.y);
        if (gameRunning)
        {
            HandlePlayerInputs();
            HandleGravity();
            HandleLockDelay();
            if (!gameRunning)
            {
                return;
            }
            GUI();
            List<int> fullLines = GetFullLines();
            ClearLines(fullLines);
        }
    }

    private void HandlePlayerInputs()
    {
        // Holding
        if (Input.GetKeyDown("c"))
        {
            Hold();
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
                //score--;
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

}
