using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompactHashing
{

    private int NumParticles;
    private float width;
    private float height;
    public Dictionary<int, List<int>> spatialHashingInfo;

    public CompactHashing(int numParticles, float width, float height)
    {
        NumParticles = numParticles;
        this.width = width;
        this.height = height;
        spatialHashingInfo = new Dictionary<int, List<int>>
        {
            { 0, new List<int>() }
        };
    }

    #region GridPartitioning
    public Vector2[] SelectSurroundingCells(Vector2 particlePosition)
    {
        Vector2[] nearCells = new Vector2[9];
        //TRY if returning the keys work as well

        Vector2 centerCell = GetCellFromPosition(particlePosition);

        //Todo: Check if those Cells are out of the limits

        //nearCells[0] -> contains the particle position cell AKA -> the center one
        //nearCells[1-8] -> the near ones
        nearCells[0] = centerCell;
        nearCells[1] = centerCell + new Vector2(1, 0); //Right
        nearCells[2] = centerCell + new Vector2(1, -1);
        nearCells[3] = centerCell + new Vector2(0, -1); // Bottom
        nearCells[4] = centerCell + new Vector2(-1, -1);
        nearCells[5] = centerCell + new Vector2(-1, 0); // Left
        nearCells[6] = centerCell + new Vector2(-1, 1);
        nearCells[7] = centerCell + new Vector2(0, 1); // Up
        nearCells[8] = centerCell + new Vector2(1, 1);

        //Once we have all the keys we can use it to go to the secondary list of indices and iterate for each particle if it is inside of the smoothing radius

        return nearCells;
    }

    public Vector2 GetCellFromPosition(Vector2 position)
    {
        Vector2 cellCoord = Vector2.zero;
        cellCoord.x = Mathf.RoundToInt(position.x / width);
        cellCoord.y = Mathf.RoundToInt(position.y / height);

        return cellCoord;
    }

    public uint HashingCell(Vector2 cell)
    {
        uint cellHashed = 0;

        uint p1 = (uint)cell.x * 73856093; // Prime Numbers
        uint p2 = (uint)cell.y * 19349663; // Prime Numbers

        cellHashed = p1 ^ p2;

        return cellHashed;
    }

    public uint GetKeyFromHashedCell(uint cellHashed)
    {
        uint key = 0;

        key = cellHashed % (uint)NumParticles;

        return key;
    }
    #endregion

    public void ClearSpatialHashingLists()
    {
        List<int> keysToRemove = new List<int>();

        foreach (var cellKeys in spatialHashingInfo.Keys)
        {
            if (spatialHashingInfo[cellKeys].Count == 0)
            {
                keysToRemove.Add(cellKeys);
            }
        }

        foreach (var key in keysToRemove)
        {
            spatialHashingInfo.Remove(key);
        }

        foreach (var particlesLists in spatialHashingInfo.Values)
        {
            particlesLists.Clear();
        }
    }

}
