using UnityEngine;
using UnityEngine.Tilemaps;

public enum TetrominoName
{
    I, T, O, J, L, S, Z
}


[System.Serializable]
public struct TetrominoData
{
    public TetrominoName tetrominoName;
    public Tile tile;
    public Vector2Int[] cells;
    public Vector2 rotationCentre;
    public Vector2Int spawnLocation;

    [HideInInspector] public Vector2Int[,] rotationCells;
    [HideInInspector] public Vector2Int[,] wallKicks;

    public void Initialize()
    {
        // Calculate rotation matrix
        CalculateRotations();

        // Calculate wall kicks
        CalculateWallKicks();
    }

    private void CalculateRotations()
    {
        rotationCells = new Vector2Int[4, cells.Length];
        for (int i = 0; i < cells.Length; i++)
        {
            Vector2Int cell = cells[i];
            // 0 degrees
            rotationCells[0, i] = cell;

            // 90 degrees CW
            rotationCells[1, i] = new Vector2Int(
                (int)(cell.y + rotationCentre.x - rotationCentre.y),
                (int)(-cell.x + rotationCentre.x + rotationCentre.y)
                );

            // 180 degrees
            rotationCells[2, i] = new Vector2Int(
                (int)(-cell.x + 2 * rotationCentre.x),
                (int)(-cell.y + 2 * rotationCentre.y)
                );

            // 270 degrees CW
            rotationCells[3, i] = new Vector2Int(
                (int)(-cell.y + rotationCentre.x + rotationCentre.y),
                (int)(cell.x - rotationCentre.x + rotationCentre.y)
                );
        }
    }

    private void CalculateWallKicks()
    {
        switch (tetrominoName)
        {
            case TetrominoName.I:
                wallKicks = new Vector2Int[,]
                {
                    { new Vector2Int(0, 0), new Vector2Int(-2, 0), new Vector2Int( 1, 0), new Vector2Int(-2,-1), new Vector2Int( 1, 2) },
                    { new Vector2Int(0, 0), new Vector2Int( 2, 0), new Vector2Int(-1, 0), new Vector2Int( 2, 1), new Vector2Int(-1,-2) },
                    { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int( 2, 0), new Vector2Int(-1, 2), new Vector2Int( 2,-1) },
                    { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int(-2, 0), new Vector2Int( 1,-2), new Vector2Int(-2, 1) },
                    { new Vector2Int(0, 0), new Vector2Int( 2, 0), new Vector2Int(-1, 0), new Vector2Int( 2, 1), new Vector2Int(-1,-2) },
                    { new Vector2Int(0, 0), new Vector2Int(-2, 0), new Vector2Int( 1, 0), new Vector2Int(-2,-1), new Vector2Int( 1, 2) },
                    { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int(-2, 0), new Vector2Int( 1,-2), new Vector2Int(-2, 1) },
                    { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int( 2, 0), new Vector2Int(-1, 2), new Vector2Int( 2,-1) }
                };
                break;

            case TetrominoName.O:
                wallKicks = new Vector2Int[,]
                {
                    { new Vector2Int(0, 0) },
                    { new Vector2Int(0, 0) },
                    { new Vector2Int(0, 0) },
                    { new Vector2Int(0, 0) },
                    { new Vector2Int(0, 0) },
                    { new Vector2Int(0, 0) },
                    { new Vector2Int(0, 0) },
                    { new Vector2Int(0, 0) },
                };
                break;
            default:
                wallKicks = new Vector2Int[,]
                {
                    { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1, 1), new Vector2Int(0,-2), new Vector2Int(-1,-2) },
                    { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int( 1,-1), new Vector2Int(0, 2), new Vector2Int( 1, 2) },
                    { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int( 1,-1), new Vector2Int(0, 2), new Vector2Int( 1, 2) },
                    { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1, 1), new Vector2Int(0,-2), new Vector2Int(-1,-2) },
                    { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int( 1, 1), new Vector2Int(0,-2), new Vector2Int( 1,-2) },
                    { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1,-1), new Vector2Int(0, 2), new Vector2Int(-1, 2) },
                    { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1,-1), new Vector2Int(0, 2), new Vector2Int(-1, 2) },
                    { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int( 1, 1), new Vector2Int(0,-2), new Vector2Int( 1,-2) }
                };
                break;
        }
    }
}