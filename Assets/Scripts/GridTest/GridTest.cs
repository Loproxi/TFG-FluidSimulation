using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class GridTest : MonoBehaviour
{

    public int rows, columns;

    Hashtable spatialRepresentation = new Hashtable();

    [SerializeField] SP_Tile sP_Tile;
    [SerializeField] SP_Particle sP_Particle;

    SP_Tile[] grid;
    SP_Particle[] _particles;

    private int NumTotalOfParticles = 9;

    // Start is called before the first frame update
    void Start()
    {
        CreateGrid();
        SpawnParticles();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CreateGrid()
    {
        grid = new SP_Tile[rows * columns];
        _particles = new SP_Particle[rows * columns];

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

    public void SpawnParticles()
    {
        _particles = new SP_Particle[NumTotalOfParticles];

        for (int i = 0; i < NumTotalOfParticles; i++)
        {
            Vector2 randomPos = RandomPosInBounds(0.5f);
            _particles[i] = Instantiate(sP_Particle, new Vector3(randomPos.x, randomPos.y), Quaternion.identity);
            _particles[i].position = randomPos;
            _particles[i].name = $"Particle: {i}";
            Vector2 _cell = GetCellFromPosition(_particles[i].position);

            Debug.Log($"Particle: {i}" + "Cell: " + _cell);
        }
    }

    private Vector2 GetCellFromPosition(Vector2 position)
    {
        Vector2 cellCoord = Vector2.zero;
        cellCoord.x = Mathf.RoundToInt(position.x / sP_Tile.width);
        cellCoord.y = Mathf.RoundToInt(position.y / sP_Tile.height);

        return cellCoord;
    }

    static private int HashingCell(Vector2 cell)
    {
        int cellHashed = 0;

        int p1 = Mathf.RoundToInt(cell.x * 73856093); // Prime Numbers
        int p2 = Mathf.RoundToInt(cell.y * 19349663); // Prime Numbers

        cellHashed = p1 ^ p2;

        return cellHashed;
    }

    private int FromCellHashedToKey(int cellHashed)
    {
        int key = 0;

        key = cellHashed % spatialRepresentation.Count;

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
