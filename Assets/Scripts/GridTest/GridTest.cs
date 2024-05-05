using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class GridTest : MonoBehaviour
{

    public int rows, columns;

    private int[] cellsHashed;
    private List<List<int>> cellData;

    [SerializeField] SP_Tile sP_Tile;
    [SerializeField] SP_Particle sP_Particle;

    SP_Tile[] grid;
    SP_Particle[] _particles;

    private int NumTotalOfParticles = 32;

    // Start is called before the first frame update
    void Start()
    {
        CreateGrid();
        SpawnParticles();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSpatialHashing(_particles);
    }

    private void CreateGrid()
    {
        grid = new SP_Tile[rows * columns];
        _particles = new SP_Particle[NumTotalOfParticles];
        cellsHashed = new int[NumTotalOfParticles];

        AllocateCellData();

        for (uint i = 0; i < columns; i++)
        {
            for (uint j = 0; j < rows; j++)
            {
                var tileRef = Instantiate(sP_Tile, new Vector3(i * sP_Tile.width, j * sP_Tile.height), Quaternion.identity);
                tileRef.position = new Vector2(i * sP_Tile.width, j * sP_Tile.height);
                tileRef.name = $"Tile {i} {j}";
            }
        }
    }

    private void AllocateCellData()
    {
        cellData = new List<List<int>>();

        int numCells = rows * columns;

        for (int i = 0; i < numCells; i++)
        {

            cellData.Add(new List<int>());

        }
    }

    public void SpawnParticles()
    {
        _particles = new SP_Particle[NumTotalOfParticles];

        for (int i = 0; i < NumTotalOfParticles; i++)
        {
            Vector2 randomPos = RandomPosInBounds(0.25f);
            _particles[i] = Instantiate(sP_Particle, new Vector3(randomPos.x, randomPos.y), Quaternion.identity);
            _particles[i].position = randomPos;
            _particles[i].name = $"Particle: {i}";
   
        }
    }

    private void ClearIndicesData()
    {
        for (int i = 0; i < cellData.Count; i++)
        {
            cellData[i].Clear();
        }
    }

    private void UpdateSpatialHashing(SP_Particle[] sp_particles)
    {

        //Clear the secondary list
        ClearIndicesData();

        for (int i = 0; i < sp_particles.Length; i++)
        {
            
            Vector2 cell = GetCellFromPosition(sp_particles[i].position);
            int key = GetKeyFromHashedCell(HashingCell(cell));

            Debug.Log($"ParticleIndex: {i} , Cell: {cell.x},{cell.y} = key:{key}" );

            cellsHashed[i] = key;

            cellData[key].Add(i);

        }

        for (int i = 0; i < sp_particles.Length; i++)
        {

            Vector2[] neighbourCells = SelectSurroundingCells(sp_particles[i].position);

            IterateNeighboursInsideRadius(neighbourCells, i);
        }
        

    }

    private Vector2[] SelectSurroundingCells(Vector2 particlePosition)
    {
        Vector2[] nearCells = new Vector2[9];
        //TRY if returning the keys work as well

        Vector2 centerCell = GetCellFromPosition(particlePosition);

        //nearCells[0] -> contains the particle position cell AKA -> the center one
        //nearCells[1-8] -> the near ones
        nearCells[0] = centerCell;
        nearCells[1] = centerCell + new Vector2(1,0); //Right
        nearCells[2] = centerCell + new Vector2(1,-1);
        nearCells[3] = centerCell + new Vector2(0,-1); // Bottom
        nearCells[4] = centerCell + new Vector2(-1,-1);
        nearCells[5] = centerCell + new Vector2(-1, 0); // Left
        nearCells[6] = centerCell + new Vector2(-1, 1);
        nearCells[7] = centerCell + new Vector2(0, 1); // Up
        nearCells[8] = centerCell + new Vector2(1, 1);

        //Once we have all the keys we can use it to go to the secondary list of indices and iterate for each particle if it is inside of the smoothing radius

        return nearCells;
    }

    void IterateNeighboursInsideRadius(Vector2[] nearCells, int particleIndex)
    {

        float radius = sP_Tile.width;
        float radius2 = radius * radius;
        SP_Particle particle = _particles[particleIndex];

        for (int i = 1; i < nearCells.Length; i++)
        {

            int key = GetKeyFromHashedCell(HashingCell(nearCells[i]));

            if (cellData[key].Count == 0) continue;

            //TODO: Sometimes if the nearCell Coords are negative the key is negative also and that produces that cellData doesnt work because there are no negative index

            for (int j = 0; j < cellData[key].Count; j++)
            {
                
                int neighbourIndex = cellData[key][j];

                if ((particle.position - _particles[neighbourIndex].position).sqrMagnitude <= radius2)
                {
                    //Compute Density of those

                    Debug.Log($"ParticleIndex: {i}, NeighbourIndex {neighbourIndex} , key:{key}");
                }

            }

        }

    }

    private Vector2 GetCellFromPosition(Vector2 position)
    {
        Vector2 cellCoord = Vector2.zero;
        cellCoord.x = Mathf.RoundToInt(position.x / sP_Tile.width);
        cellCoord.y = Mathf.RoundToInt(position.y / sP_Tile.height);

        return cellCoord;
    }

    private int HashingCell(Vector2 cell)
    {
        int cellHashed = 0;

        int p1 = (int)cell.x * 73856093; // Prime Numbers
        int p2 = (int)cell.y * 19349663; // Prime Numbers

        cellHashed = p1 ^ p2;

        return cellHashed;
    }

    private int GetKeyFromHashedCell(int cellHashed)
    {
        int key = 0;

        key = cellHashed % cellsHashed.Length;

        return key;
    }

    private Vector2 RandomPosInBounds(float particleRadius)
    {
        float width = columns * sP_Tile.width;
        float height = rows * sP_Tile.height;
        //Taking into account the particle radius in bounds
        float minX = 0.0f + particleRadius;
        float maxX = 0.0f + width - particleRadius;
        float minY = 0.0f + particleRadius;
        float maxY = 0.0f + height - particleRadius;

        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);

        return new Vector2(x, y);

    }
}
