using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class KMeansClustering : MonoBehaviour
{
    public int numberOfSpheres = 100;
    public int numberOfClusters = 3;
    public GameObject spherePrefab;
    public Vector3 spaceSize = new Vector3(10, 10, 10);
    
    private List<GameObject> spheres = new List<GameObject>();
    private Vector3[] centroids;
    private Dictionary<int, List<GameObject>> clusters = new Dictionary<int, List<GameObject>>();
    
    // Start is called before the first frame update
    void Start()
    {
        GenerateSpheres();
        InitializeCentroids();
        StartCoroutine(ClusterSpheres());
    }

    private void GenerateSpheres()
    {
        for (int i = 0; i < numberOfSpheres; i++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(-spaceSize.x, spaceSize.x),
                Random.Range(-spaceSize.y, spaceSize.y),
                Random.Range(-spaceSize.z, spaceSize.z)
                );
            GameObject sphere = Instantiate(spherePrefab, randomPosition, Quaternion.identity);
            spheres.Add(sphere);
        }
    }

    private void InitializeCentroids()
    {
        centroids = new Vector3[numberOfClusters];
        for (int i = 0; i < numberOfClusters; i++)
        {
            centroids[i] = spheres[Random.Range(0, spheres.Count)].transform.position;
        }
    }

    private IEnumerator ClusterSpheres()
    {
        bool centroidsChanged = true;

        while (centroidsChanged)
        {
            clusters.Clear();
            for (int i = 0; i < numberOfClusters; i++)
            {
                clusters[i] = new List<GameObject>();
            }

            foreach (var sphere in spheres)
            {
                int closestCentroidIndex = GetClosestCentroidIndex(sphere.transform.position);
                clusters[closestCentroidIndex].Add(sphere);
            }

            centroidsChanged = UpdateCentroids();
            VisualizeClusters();
            
            yield return null;
        }
    }

    private int GetClosestCentroidIndex(Vector3 position)
    {
        float minDistance = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < centroids.Length; i++)
        {
            float distance = Vector3.Distance(position, centroids[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private bool UpdateCentroids()
    {
        bool changed = false;
        for (int i = 0; i < numberOfClusters; i++)
        {
            if (clusters[i].Count == 0) continue;
            
            Vector3 newCentroid = Vector3.zero;
            foreach (var sphere in clusters[i])
            {
                newCentroid += sphere.transform.position;
            }
            newCentroid /= clusters[i].Count;

            if (Vector3.Distance(newCentroid, centroids[i]) < 0.01f)
            {
                changed = true;
                centroids[i] = newCentroid;
            }
        }
        return changed;
    }

    private void VisualizeClusters()
    {
        Color[] colors = {Color.blue, Color.green, Color.yellow};

        for (int i = 0; i < numberOfClusters; i++)
        {
            foreach (var sphere in clusters[i])
            {
                sphere.GetComponent<Renderer>().material.color = colors[i];   
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
