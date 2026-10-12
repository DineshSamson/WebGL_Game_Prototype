using Unity.VisualScripting;
using UnityEngine;

public class VisionCone : MonoBehaviour
{
    public GuardAIController guard;
    public int rayCount = 40;
    public float rayHeight = 0.5f;
    public float floorOffset = 0.05f;

    Mesh mesh;
    Material material;
    Vector3[] vertices;
    int[] triangles;

    private void Awake()
    {
        mesh = new Mesh();
        mesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = mesh;
        material = GetComponent<MeshRenderer>().material;

        vertices = new Vector3[rayCount +2 ];
        triangles = new int[rayCount * 3];

        for(int i = 0; i < rayCount; i++)
        {
           // Debug.Log("i "+i);
            int t = i * 3;
            triangles[t] = 0;
            triangles[t + 1] = i + 1;
            triangles[t + 2] = i + 2;
        }
      //  Debug.Log("rayCount = " + rayCount + ", triangles length = " + triangles.Length);
        mesh.vertices = vertices;
        mesh.triangles = triangles;

      
    }

    private void LateUpdate()
    {
        float range = guard.viewRange;
        float angle = guard.viewAngle;
        float step = angle / rayCount;

        Vector3 origin = guard.transform.position + Vector3.up * rayHeight;

        vertices[0] = new Vector3(0f, floorOffset, 0);

        for(int i = 1; i <= rayCount; i++)
        {
            float a = -angle * 0.5f + step * i;
            Vector3 dir = Quaternion.AngleAxis(a, Vector3.up) * guard.transform.forward;

            Vector3 point;
            if (Physics.Raycast(origin, dir, out RaycastHit hit, range, guard.wallMask))
            {
                point = hit.point - dir * hit.distance;
            }
            else
                point = origin + dir * range;

            Vector3 local = transform.InverseTransformPoint(point);
            local.y = floorOffset;
            vertices[i + 1] = local;
            //Debug.DrawRay(origin, dir * guard.viewRange, Color.green);
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();

       
    }

    public void SetColor(Color color)
    {
        material.color = color;
    }

}
