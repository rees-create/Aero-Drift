using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class PieSign : MonoBehaviour
{
    [Tooltip("\'Cyclic\' because 1 triangle is subtracted when wrapping around 360 degrees, for algorithmic reasons." +
        " Even triangle counts are slightly more recommended")]
    [Min(3)]
    public int cyclicTriangleCount;
    public float startAngle;
    public float endAngle;
    public float refreshFPS;
    //only run if MeshFilter and MeshRenderer are attached.
    public void DrawPieMesh(int triangleCount, Vector2 angles)
    {
        Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
        int trisCount = triangleCount;
        int trisArrayCount = triangleCount * 3;
        if ((angles.y < 360 || angles.x > 0))
        {
            trisCount = trisCount - 1;
        }
        trisArrayCount = trisCount * 3;
        float theta = (angles.y - angles.x) / trisCount;
        Vector3[] vertices = new Vector3[triangleCount + 1];
        int[] triangles = new int[trisArrayCount];
        vertices[0] = Vector3.zero; //center of the pie
        triangles[0] = 0;
        for (int i = 0; i < triangleCount; i++)
        {
            if (i * 2 + 1 < triangleCount + 1)
            {
                vertices[i * 2 + 1] = new Vector3(Mathf.Cos((angles.x + theta * i * 2) * Mathf.Deg2Rad), Mathf.Sin((angles.x + theta * i * 2) * Mathf.Deg2Rad), 0);
                if (i * 2 + 2 < triangleCount + 1)
                {
                    vertices[i * 2 + 2] = new Vector3(Mathf.Cos((angles.x + theta * ((i * 2) + 1)) * Mathf.Deg2Rad), Mathf.Sin((angles.x + theta * ((i * 2) + 1)) * Mathf.Deg2Rad), 0);
                }
            }
            
            if (!(angles.y < 360 || angles.x > 0))
            {
                triangles[(i * 3 + 1) % triangles.Length] = (i + 2) % (vertices.Length) == 0 ? 1 : (i + 2) % (vertices.Length);
                triangles[(i * 3 + 2) % triangles.Length] = (i + 1) % (vertices.Length) == 0 ? 1 : (i + 1) % (vertices.Length);
            }
            else if(i < triangleCount - 1) 
            {
                triangles[(i * 3 + 1)] = i + 2;
                triangles[(i * 3 + 2)] = i + 1;
            }
            triangles[(i * 3 + 3) % triangles.Length] = 0;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
    IEnumerator UpdateMesh()
    {
        while (true)
        {
            DrawPieMesh(cyclicTriangleCount, new Vector2(startAngle, endAngle));
            yield return new WaitForSeconds(1/refreshFPS);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (refreshFPS > 0)
        {
            StartCoroutine(UpdateMesh());
        }
        else
        {
            DrawPieMesh(cyclicTriangleCount, new Vector2(startAngle, endAngle));
        }
    }

}
