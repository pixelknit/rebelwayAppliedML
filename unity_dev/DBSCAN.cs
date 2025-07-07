using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DBSCAN : MonoBehaviour
{
    public GameObject spherePrefab;

    public float epsilon = 2f;

    public int minPoints = 3;
    public int numSpheres = 100;
    
    private List<Sphere> spheres = new List<Sphere>();
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numSpheres; i++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(-10f, 10f),
                Random.Range(-10f, 10f),
                Random.Range(-10f, 10f)
                );

            GameObject sphereObj = Instantiate(spherePrefab, randomPosition, Quaternion.identity);
            Sphere sphere = new Sphere(sphereObj, randomPosition);
            spheres.Add(sphere);
        }
        PerformDBSCAN();
        UpdateSphereColors();
    }

    void PerformDBSCAN()
    {
        int clusterId = 0;
        foreach (var sphere in spheres)
        {
            if (!sphere.visited)
            {
                sphere.visited = true;
                List<Sphere> neighbors = GetNeighbors(sphere);

                if (neighbors.Count > minPoints)
                {
                    clusterId++;
                    ExpandCluster(sphere, neighbors, clusterId);
                }
                else
                {
                    sphere.clusterId = -1;
                }
            }
        }
    }

    List<Sphere> GetNeighbors(Sphere sphere)
    {
        List<Sphere> neighbors = new List<Sphere>();
        foreach (var otherSphere in spheres)
        {
            if (Vector3.Distance(sphere.position, otherSphere.position) <= epsilon)
            {
                neighbors.Add(otherSphere);
            }
        }
        return neighbors;
    }

    void ExpandCluster(Sphere sphere, List<Sphere> neighbors, int clusterId)
    {
        sphere.clusterId = clusterId;
        Queue<Sphere> toProcess = new Queue<Sphere>(neighbors);

        while (toProcess.Count > 0)
        {
            Sphere current = toProcess.Dequeue();
            if (!current.visited)
            {
                current.visited = true;
                List<Sphere> currentNeighbors = GetNeighbors(current);
                if (currentNeighbors.Count >= minPoints)
                {
                    foreach (var neighbor in currentNeighbors)
                    {
                        if (!toProcess.Contains(neighbor))
                        {
                            toProcess.Enqueue(neighbor);
                        }
                    }
                }
            }

            if (current.clusterId == -1)
            {
                current.clusterId = clusterId;
            }
        }
        
    }

    void UpdateSphereColors()
    {
        foreach (var sphere in spheres)
        {
            Color clusterColor = GetClusterColor(sphere.clusterId);
            sphere.sphereObject.GetComponent<Renderer>().material.color = clusterColor;
        }
    }

    Color GetClusterColor(int clusterId)
    {
        if (clusterId == -1)
        {
            return Color.black;
        }

        switch (clusterId)
        {
            case 1 :
                return Color.blue;
            case 2 :
                return Color.green;
            case 3:
                return Color.yellow;
            default:
                return Color.cyan;
                // return new Color(Random.value, Random.value, Random.value);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private class Sphere
    {
        public GameObject sphereObject;
        public Vector3 position;
        public bool visited;
        public int clusterId;

        public Sphere(GameObject sphereObject, Vector3 position)
        {
            this.sphereObject = sphereObject;
            this.position = position;
            this.visited = false;
            this.clusterId = 0;
        }
    }
}
