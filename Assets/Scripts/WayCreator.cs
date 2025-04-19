using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayCreator : MonoBehaviour
{
    public Color lineColor = Color.white;
    public List<Transform> path_objs = new List<Transform>();
    public float sizePoints = 0.01f;
    public bool generateRandomPath = false;
    public int numberOfPoints = 5;
    public Vector3 generationArea = new Vector3(10, 0, 10); // Área XZ para generación

    Transform[] pointsArray;

    // Método para generar puntos aleatorios
    void Start()
    {
        if (generateRandomPath)
        {
            GenerateRandomPoints();
        }
    }

    void GenerateRandomPoints()
    {
        // Limpiar puntos previos
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        path_objs.Clear();

        // Generar nuevos puntos aleatorios
        for (int i = 0; i < numberOfPoints; i++)
        {
            GameObject point = new GameObject("point " + i);
            point.transform.parent = this.transform;

            // Generar posición aleatoria dentro del área
            float randX = Random.Range(-generationArea.x / 2, generationArea.x / 2);
            float randZ = Random.Range(-generationArea.z / 2, generationArea.z / 2);
            float y = this.transform.position.y; // Mantener altura fija si quieres

            point.transform.localPosition = new Vector3(randX, y, randZ);
            path_objs.Add(point.transform);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = lineColor;
        pointsArray = GetComponentsInChildren<Transform>();
        path_objs.Clear();

        foreach (Transform path_obj in pointsArray)
        {
            if (path_obj != this.transform)
            {
                path_objs.Add(path_obj);
            }
        }

        for (int i = 0; i < path_objs.Count; i++)
        {
            Vector3 position = path_objs[i].position;
            if (i > 0)
            {
                Vector3 previous = path_objs[i - 1].position;
                Gizmos.DrawLine(previous, position);
                Gizmos.DrawWireSphere(position, sizePoints);
            }
        }
    }
}



// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class WayCreator : MonoBehaviour
// {
//     public Color lineColor=Color.white;
//     public List<Transform> path_objs=new List<Transform>();
//     public float sizePoints=0.01f;
//     Transform[] pointsArray; 
//     private void OnDrawGizmos()
//     {
//         Gizmos.color=lineColor;
//         pointsArray=GetComponentsInChildren<Transform>();
//         path_objs.Clear();

//         foreach(Transform path_obj in pointsArray)
//         {
//             if(path_obj!=this.transform)
//             {
//                 path_objs.Add(path_obj);
//             }
//         }

//         for(int i=0;i<path_objs.Count;i++)
//         {
//             Vector3 position=path_objs[i].position;
//             if(i>0)
//             {
//                 Vector3 previous=path_objs[i-1].position;    
//                 Gizmos.DrawLine(previous,position);
//                 Gizmos.DrawWireSphere(position,sizePoints);          
//             }
//         }

//     }
// }
