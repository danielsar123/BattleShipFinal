using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Erosion : MonoBehaviour
{
    public float massPerIteration = 0.2f;
    float maxMass = 1f;
    public int iterations = 200000;
    Terrain terrain;
    float[,] savedHeightMap;

    public void Run()
    {
        //Saves the heightmap before erosion
        SaveHeight();

        //Initialize data
        terrain = GetComponent<Terrain>();
        int heightmapSize = terrain.terrainData.heightmapResolution;
        float[,] heightMap = terrain.terrainData.GetHeights(0, 0, heightmapSize, heightmapSize);

        //Simulate all droplets
        for (int i = 0; i < iterations - 1; i++)
        {
            //Get random positions in terrain space
            int x = Random.Range(1, terrain.terrainData.heightmapResolution - 1);
            int z = Random.Range(1, terrain.terrainData.heightmapResolution - 1);

            Vector2Int coords = new Vector2Int(x, z);
            float mass = 0;
            Vector2Int previousCoords = new Vector2Int(0, 0);

            //Simulate single droplet falling
            for (int j = 0; j < 20; j++)
            {
                heightData min = new heightData();
                min.value = 10000;

                //Get smallest adjacent point on heightmap
                for (int _x = -1; _x <= 1; _x++)
                {
                    for (int _z = -1; _z <= 1; _z++)
                    {
                        heightData data = new heightData();

                        if (coords.x + _x < 0 || coords.y + _z < 0 || coords.x + _x > heightmapSize - 1 || coords.y + _z > heightmapSize - 1 || previousCoords == new Vector2Int(coords.x + _x, coords.y + _z))
                        {
                            data.value = 8000;
                        }
                        else
                        {
                            data.value = heightMap[coords.x + _x, coords.y + _z];
                            data.x = coords.x + _x;
                            data.z = coords.y + _z;
                        }

                        if (data.value < min.value && data.value < heightMap[coords.x, coords.y])
                        {
                            min = data;
                        }
                    }
                }

                //Droplet pick up the mass it erodes
                float addedMass = Mathf.Abs(heightMap[coords.x, coords.y] - heightMap[min.x, min.z]);
                
                //Early exit conditions
                if (min.value > heightMap[coords.x, coords.y] || mass >= maxMass)
                {
                    break;
                }

                //Finalize erosion
                mass += addedMass;

                heightMap[coords.x, coords.y] -= addedMass * massPerIteration;

                //Smooth height
                int erosionWidth = 1;
                float smoothHeight = 0;
                for (int _x = -erosionWidth; _x <= erosionWidth; _x++)
                {
                    for (int _z = -erosionWidth; _z <= erosionWidth; _z++)
                    {
                        if (coords.x + _x > 0 && coords.y + _z > 0 && coords.x + _x < heightmapSize - 1 && coords.y + _z < heightmapSize - 1)
                        {
                            smoothHeight += heightMap[coords.x + _x, coords.y + _z];
                        }
                    }
                }
                smoothHeight /= 9;
                for (int _x = -erosionWidth; _x <= erosionWidth; _x++)
                {
                    for (int _z = -erosionWidth; _z <= erosionWidth; _z++)
                    {
                        if (coords.x + _x > 0 && coords.y + _z > 0 && coords.x + _x < heightmapSize - 1 && coords.y + _z < heightmapSize - 1)
                        {
                            heightMap[coords.x + _x, coords.y + _z] = smoothHeight;
                        }
                    }
                }

                previousCoords = coords;
                coords = new Vector2Int(min.x, min.z);
            }

            //Droplet deposits all mass it has picked up while falling
            int depositWidth = 10;
            for (int _x = -depositWidth; _x <= depositWidth; _x++)
            {
                for (int _z = -depositWidth; _z <= depositWidth; _z++)
                {
                    if (coords.x + _x > 0 && coords.y + _z > 0 && coords.x + _x < heightmapSize - 1 && coords.y + _z < heightmapSize - 1)
                    {
                        float falloff = 1 - Mathf.Clamp01(Vector2.Distance(new Vector2(_x, _z), Vector2.zero) / depositWidth);
                        heightMap[coords.x + _x, coords.y + _z] += (mass * Mathf.Clamp01(massPerIteration) * falloff) / (depositWidth * 50f);
                    }
                }
            }
        }

        //Set heightmap data
        terrain.terrainData.SetHeights(0, 0, heightMap);
        terrain.terrainData.size = new Vector3(terrain.terrainData.size.x - 1, terrain.terrainData.size.y, terrain.terrainData.size.z);
        terrain.terrainData.size = new Vector3(terrain.terrainData.size.x + 1, terrain.terrainData.size.y, terrain.terrainData.size.z);
    }

    public void SaveHeight()
    {
        terrain = GetComponent<Terrain>();
        int heightmapSize = terrain.terrainData.heightmapResolution;
        savedHeightMap = terrain.terrainData.GetHeights(0, 0, heightmapSize, heightmapSize);
    }

    public void RestoreHeight()
    {
        terrain = GetComponent<Terrain>();
        terrain.terrainData.SetHeights(0, 0, savedHeightMap);
        terrain.terrainData.size = new Vector3(terrain.terrainData.size.x - 1, terrain.terrainData.size.y, terrain.terrainData.size.z);
        terrain.terrainData.size = new Vector3(terrain.terrainData.size.x + 1, terrain.terrainData.size.y, terrain.terrainData.size.z);
    }
}

public struct heightData
{
    public float value;
    public int x;
    public int z;
}
