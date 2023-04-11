using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AIController2 : Board
{
    private int linesCleared;
    private bool weightsSet = false;
    private Move currentMove = null;
    public float bumpinessWeight;
    public float blocksInRightColumnWeight;
    public float pillarWeight;
    public float holesWeight;
    public float linesClearedNotTetrisWeight;
    public float linesClearedTetrisWeight;
    public float blocksAboveHolesWeight;
    public float addedHeightWeight;

    private void Start()
    {
        linesCleared = 0;
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

        //SetWeights(-5.922214f, -11.14342f, -6.940077f, -159.806f, -31.40231f, 16.82962f, -4.273068f, -1.282436f);
        SetWeights(-5.96f, -10.89f, -6.45f, -162.41f, -32.3f, 18.22f, -4.44f, -1.24f);
        SpawnPiece();
    }

    private void Update()
    {
        
        if (weightsSet && gameRunning)// && Input.GetKeyDown(KeyCode.R))
        {
            bool swapWithHeld = false;
            if (currentMove == null)
            {
                (currentMove, swapWithHeld) = GetBestMoveCurrentAndHeld();
                string heights = "heights ";
                for (int i = 0; i < 10; i++)
                {
                    MaxSoftDrop();
                    heights += $"{GetHeightOfColumn(i)} ";
                    currentPiece.ResetPosition();
                }

            }
            //Debug.Log($"{currentMove.x} {currentMove.r}");

            if (swapWithHeld)
            {
                if (heldPiece == null)
                {
                    SwapCurrentAndHeldPieces();
                    SpawnPiece();
                }
                else
                {
                    SwapCurrentAndHeldPieces();
                }
            }
            else if (currentMove.r - currentPiece.rotation == 3)
            {
                currentPiece.RotatePiece(3);
            }
            else if (currentMove.r - currentPiece.rotation != 0)
            {
                currentPiece.RotatePiece(1);
            }
            else if (currentMove.x < currentPiece.position.x)
            {
                currentPiece.MovePiece(Vector2Int.left);
            }
            else if (currentMove.x > currentPiece.position.x)
            {
                currentPiece.MovePiece(Vector2Int.right);
            }
            else
            {
                score += 2 * HardDrop();
                currentMove = null;
            }
        }
        ClearLines(GetFullLines());
        GUI();

        
        /*
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentPiece.MovePiece(Vector2Int.left);
            MaxSoftDrop();
            EvaluatePosition();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentPiece.MovePiece(Vector2Int.right);
            MaxSoftDrop();
            EvaluatePosition();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
            MaxSoftDrop();
            EvaluatePosition();
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            currentPiece.RotatePiece(3);
            MaxSoftDrop();
            EvaluatePosition();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            currentPiece.RotatePiece(1);
            MaxSoftDrop();
            EvaluatePosition();
        }
        else
        {
            //MaxSoftDrop();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            int x = currentPiece.position.x;
            int r = currentPiece.rotation;
            Move bestMove = GetBestMove();
            currentPiece.position.x = x;
            currentPiece.rotation = r;
            Debug.Log($"{bestMove.x} {bestMove.r}");
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            MaxSoftDrop();
            Debug.Log(currentPiece.position.y);
        }
        
        List<int> linesToClear = GetFullLines();
        ClearLines(linesToClear);
        GUI();
        currentPiece.position.y = 8;
       */
    }
    
    public float[] GetWeights()
    {
        return new float[] { 
            bumpinessWeight, blocksInRightColumnWeight, pillarWeight, holesWeight, 
            linesClearedNotTetrisWeight, linesClearedTetrisWeight, blocksAboveHolesWeight, addedHeightWeight
        };
    }

    private void SwapCurrentAndHeldPieces()
    {
        Piece temp = currentPiece;
        currentPiece = heldPiece;
        heldPiece = temp;
    }

    private void SwapCurrentAndNextPieces()
    {
        Piece temp = currentPiece;
        currentPiece = nextPieces[0];
        nextPieces[0] = temp;
    }
    
    private void SwapOutCurrentPiece()
    {
        if (heldPiece == null)
        {
            SwapCurrentAndNextPieces();
        }
        else
        {
            SwapCurrentAndHeldPieces();
        }
    }

    private (Move, bool) GetBestMoveCurrentAndHeld()
    {
        (Move bestMoveCurrent, float currentPieceEvaluation) = GetBestMove();
        SwapOutCurrentPiece();
        (Move bestMoveHeld, float heldPieceEvaluation) = GetBestMove();
        SwapOutCurrentPiece();
        if (currentPieceEvaluation > heldPieceEvaluation)
        {
            return (bestMoveCurrent, false);
        }
        else
        {
            return (bestMoveHeld, true);
        }
    }

    private (Move, float) GetBestMove()
    {
        // If Tetris ready
        // If all heights 4 more than right well
        if (currentPiece.tetrominoData.tetrominoName == TetrominoName.I
           && GetMinHeight() - 4 >= GetHeightOfColumn(9))
        {
            return (new Move(4, 1), float.PositiveInfinity);
        }

        float bestMoveScore = float.NegativeInfinity;
        Move bestMove = new Move(0, 0);
        string toPrint = "move evaluations ";
        for (int r = 0; r < 4; r++)
        {
            currentPiece.ResetPosition();
            currentPiece.RotatePiece(r);
            (int, int) bounds = GetHorizontalBounds();
            for (int x = bounds.Item1; x <= bounds.Item2; x++)
            {
                currentPiece.ResetPosition();
                currentPiece.RotatePiece(r);
                currentPiece.position.x = x;
                MaxSoftDrop();
                float evaluation = EvaluatePosition();
                toPrint += $"{x} {r} {evaluation}\n";
                if (evaluation > bestMoveScore)
                {
                    bestMoveScore = evaluation;
                    bestMove = new Move(x, r);
                }
            }
        }
        currentPiece.ResetPosition();
        return (bestMove, bestMoveScore);
    }

    private int GetMinHeight()
    {
        int minHeight = int.MaxValue;
        for (int i = 0; i < 9; i++)
        {
            int height = GetHeightOfColumn(i);
            if (height < minHeight)
            {
                minHeight = height;
            }
        }
        return minHeight;
    }

    private (int, int) GetHorizontalBounds()
    {
        int xBefore = currentPiece.position.x;
        (int, int) bounds = new();
        while (CurrentPieceValid())
        {
            currentPiece.MovePiece(Vector2Int.left);
        }
        bounds.Item1 = currentPiece.position.x + 1;
        currentPiece.position.x = xBefore;
        while (CurrentPieceValid())
        {
            currentPiece.MovePiece(Vector2Int.right);
        }
        bounds.Item2 = currentPiece.position.x - 1;
        currentPiece.position.x = xBefore;

        return bounds;
    }

    public void SetWeights(float bumpinessWeight,
                           float blocksInRightColumnWeight,
                           float pillarWeight,
                           float holesWeight,
                           float linesClearedNotTetrisWeight,
                           float linesClearedTetrisWeight,
                           float blocksAboveHolesWeight,
                           float addedHeightWeight)
    {
        this.bumpinessWeight = bumpinessWeight;
        this.blocksInRightColumnWeight = blocksInRightColumnWeight;
        this.pillarWeight = pillarWeight;
        this.holesWeight = holesWeight;
        this.linesClearedNotTetrisWeight = linesClearedNotTetrisWeight;
        this.linesClearedTetrisWeight = linesClearedTetrisWeight;
        this.blocksAboveHolesWeight = blocksAboveHolesWeight;
        this.addedHeightWeight = addedHeightWeight;

        weightsSet = true;
    }

    public void SetWeights(float[] weights)
    {
        bumpinessWeight = weights[0];
        blocksInRightColumnWeight = weights[1];
        pillarWeight = weights[2];
        holesWeight = weights[3];
        linesClearedNotTetrisWeight = weights[4];
        linesClearedTetrisWeight = weights[5];
        blocksAboveHolesWeight = weights[6];
        addedHeightWeight = weights[7];

        weightsSet = true;
    }

    //

    private float EvaluatePosition()
    {
        float evaluation = 0;
        evaluation += bumpinessWeight * GetBumpiness();
        evaluation += blocksInRightColumnWeight * GetBlocksInRightColumn();
        evaluation += pillarWeight * GetPillars();
        evaluation += holesWeight * GetNumberOfHoles();
        evaluation += linesClearedNotTetrisWeight * GetNonTetrisLines();
        evaluation += linesClearedTetrisWeight * GetTetrisLines();
        evaluation += blocksAboveHolesWeight * GetBlocksAboveHoles();
        evaluation += addedHeightWeight * GetAddedHeight();

        return evaluation;
    }

    private bool CellOccupied(int x, int y, bool includeCurrent = true)
    {
        if (deadCellMap[y][x] != null)
        {
            return true;
        }
        if (includeCurrent)
        {
            foreach (var cell in currentPiece.Cells())
            {
                if (cell.y + 10 == y && cell.x + 5 == x)
                {
                    return true;
                }
            }
        }
        return false;
    }


    private int GetHeightOfBoard(bool includeCurrent = true)
    {
        int maxHeight = 0;
        for (int i = 0; i < 10; i++)
        {
            int height = GetHeightOfColumn(i, includeCurrent);
            if (height > maxHeight)
            {
                maxHeight = height;
            }
        }
        return maxHeight;
    }

    private int GetHeightOfColumn(int column, bool includeCurrent = true)
    {
        for (int i = 22; i >= 0; i--)
        {
            if (CellOccupied(column, i, includeCurrent))
            {
                return i + 1;
            }
        }
        return 0;
    }

    private int GetBumpiness()
    {
        int totalBumpiness = 0;
        for (int i = 0; i < 8; i++) // Don't include right column
        {
            int k = GetBumpinessOfTwoColumns(i);
            //Debug.Log(k);
            totalBumpiness += k;
        }
        return totalBumpiness;
    }

    private int GetBumpinessOfTwoColumns(int x)
    {
        return Mathf.Abs(GetHeightOfColumn(x) - GetHeightOfColumn(x + 1));
    }

    private int GetBlocksInRightColumn()
    {
        for (int i = 22; i >= 0; i--)
        {
            if (CellOccupied(9, i))
            {
                return i + 1;
            }
        }
        return 0;
    }

    private int GetPillars()
    {
        int totalPillars = 0;
        for (int i = 0; i < 8; i++)
        {
            int bumpiness = GetBumpinessOfTwoColumns(i);
            if (bumpiness >= 3)
            {
                totalPillars += bumpiness - 2;
            }
        }
        return totalPillars;
    }

    private int GetNumberOfHoles(bool includeCurrent = true)
    {
        int holeCount = 0;
        for (int i = 0; i < 10; i++)
        {
            holeCount += GetNumberOfHolesInColumn(i, includeCurrent);
        }
        return holeCount;
    }

    private int GetNumberOfHolesInColumn(int column, bool includeCurrent = true)
    {
        int height = GetHeightOfColumn(column, includeCurrent);
        int holeCount = 0;
        for (int i = height - 1; i >= 0; i--)
        {
            if (!CellOccupied(column, i, includeCurrent))
            {
                holeCount++;
            }
        }
        return holeCount;
    }

    private int GetNonTetrisLines()
    {
        int numberOfLines = GetFullLines(true).Count;
        return numberOfLines < 4 ? numberOfLines : 0;
    }

    private int GetTetrisLines()
    {
        int numberOfLines = GetFullLines(true).Count;
        return numberOfLines >= 4 ? numberOfLines : 0;
    }

    private int GetBlocksAboveHoles()
    {
        int blocksAboveHoles = 0;
        for (int i = 0; i < 10; i++)
        {
            blocksAboveHoles += GetBlocksAboveHolesInColumn(i);
        }
        return blocksAboveHoles;
    }

    private int GetBlocksAboveHolesInColumn(int column)
    {
        int blocksAboveHoles = 0;
        bool blockFound = false;
        int blockFoundHeight = 22;
        List<int> linesToClear = GetFullLines(true);
        //Debug.Log(linesToClear.Count);
        for (int i = 22; i >= 0; i--)
        {
            if (linesToClear.Contains(i))
            {
                blockFoundHeight -= 1;
                continue;
            }
            else if (blockFound && !CellOccupied(column, i, true))
            {
                //Debug.Log($"{blockFoundHeight} {i}");
                blocksAboveHoles += blockFoundHeight - i;
            }
            else if (!blockFound && CellOccupied(column, i, true))
            {
                blockFound = true;
                blockFoundHeight = i;
            }
        }
        return blocksAboveHoles;
    }

    private int GetAddedHeight()
    {
        return GetHeightOfBoard(true) - GetHeightOfBoard(false) - GetFullLines().Count;
    }



    /*

    private int GetAggregateHeight()
    {
        int height = 0;
        for (int i = 0; i < 10; i++)
        {
            height += GetHeightOfColumn(i);
        }
        return height;
    }

    private int GetHeightOfRightWell()
    {
        return GetBumpinessOfTwoColumns(8);
    }

    

    */
    
    private void MaxSoftDrop()
    {
        while (CanMoveDown())
        {
            SoftDrop();
        }
    }

    private List<int> GetFullLines(bool includeCurrent = false)
    {
        List<int> fullLines = new List<int>();
        for (int i = 0; i < 23; i++)
        {
            bool rowFull = true;
            for (int j = 0; j < 10; j++)
            {
                if (!CellOccupied(j, i, includeCurrent))
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

    new private void ClearLines(List<int> linesToClear)
    {
        linesCleared += linesToClear.Count;
        base.ClearLines(linesToClear);
    }
}
