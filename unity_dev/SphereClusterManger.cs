using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class SphereClusterManger : MonoBehaviour
{
    public int numberOfClusters = 3;
    public int numberOfSpheresPerCluster = 10;
    public float clusterRadius = 5f;
    public int k = 3;
    
    public GameObject clusterSpherePrefab;
    public GameObject newSpherePrefab;
    
    private List<Vector3> spherePositions = new List<Vector3>();
    private List<Color> clusterColors = new List<Color>();
        
    // Start is called before the first frame update
    void Start()
    {
        GenerateClusters();
    }

    void GenerateClusters()
    {
        for (int i = 0; i < numberOfClusters; i++)
        {
            Vector3 clusterCenter = Random.insideUnitSphere * 10;
            Color clusterColor = Random.ColorHSV();

            for (int j = 0; j < numberOfSpheresPerCluster; j++)
            {
                Vector3 position = clusterCenter + Random.insideUnitSphere * clusterRadius;
                spherePositions.Add(position);
                clusterColors.Add(clusterColor);
                CreateSphere(position, clusterColor, clusterSpherePrefab);
            }
        }
    }

    void CreateSphere(Vector3 position, Color color, GameObject spherePrefab)
    {
        GameObject sphere = Instantiate(spherePrefab, position, Quaternion.identity);
        Renderer renderer = sphere.GetComponent<Renderer>();
        renderer.material.color = color;
    }

    public void ClassifyNewSphere(Vector3 position)
    {
        var nearestNeighbors = spherePositions.Select((p, index) => new {Position = p, Color=clusterColors[index], Distance=Vector3.Distance(position, p)})
            .OrderBy(x => x.Distance)
            .Take(k)
            .ToList();

        var mostCommonColor = nearestNeighbors
            .GroupBy(x => x.Color)
            .OrderByDescending(g => g.Count())
            .First()
            .Key;
        Debug.Log($"Classified new sphere as {position} to the color {mostCommonColor}");
        Renderer renderer = newSpherePrefab.GetComponent<Renderer>();
        renderer.material.color = mostCommonColor;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            ClassifyNewSphere(newSpherePrefab.transform.position);
        }
        
    }
}
