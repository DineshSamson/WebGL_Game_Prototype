using Unity.VisualScripting;
using UnityEngine;

public class VisionCone : MonoBehaviour
{
    public GuardAIController guard;
    public int rayCount = 40;
    public float rayHeight = 0.5f;
    public float floorOffset = 0.05f;

    public int edgeIterations = 8;
    public float edgeDistanceThreshold = 1f;
    public float wallPadding = 0.02f;

    struct CastInfo
    {
        public bool hit;
        public float dist;
        public Vector3 point;
    }

    Mesh mesh;
    Material material;
    Vector3[] vertices;
    int[] triangles;

    Vector3[] points;
    int maxPoints;

    private void Awake()
    {
        mesh = new Mesh();
        mesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = mesh;

        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        material = mr.material;

        maxPoints = rayCount * 3 + 3;
        points = new Vector3[maxPoints];
        vertices = new Vector3[maxPoints + 1];
        triangles = new int[(maxPoints -1 ) * 3];

        for(int i = 0; i < maxPoints -1; i++)
        {
            int t = i * 3;
            triangles[t] = 0;
            triangles[t + 1] = i + 1;
            triangles[t + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        /*
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

      */
    }

    private void LateUpdate()
    {
        //float range = guard.viewRange;
        float angle = guard.viewAngle;
        float step = angle / rayCount;

        Vector3 origin = guard.transform.position + Vector3.up * rayHeight;

        int count = 0;
        CastInfo prev = default;

        for(int i = 0; i <= rayCount; i++) 
        {
            float a = -angle * 0.5f + step * i;
            CastInfo cur = Cast(origin, a);

            if(i > 0)
            {
                bool edge = prev.hit != cur.hit ||
                    (prev.hit && cur.hit && Mathf.Abs(prev.dist - cur.dist) > edgeDistanceThreshold);
         

                if(edge && count + 2 < maxPoints -1)
                {
                    FindEdge(origin, a - step, a, prev, cur, out Vector3 nearPoint, out Vector3 farPoint);
                    points[count++] = nearPoint;
                    points[count++] = farPoint;

                }
            }

            if (count < maxPoints)
                points[count++] = cur.point;

            prev = cur;
        }

        vertices[0] = new Vector3(0f, floorOffset, 0f);
        Vector3 last = Vector3.zero;
        for(int i =0; i < count; i++)
        {
            Vector3 local = transform.InverseTransformPoint(points[i]);
            local.y = floorOffset;
            vertices[i +1] = local;
            last = local;
        }

        for(int i = count; i < maxPoints; i++)
        {
            vertices[i + 1] = last;
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();

        /*
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

       */
    }

    CastInfo Cast(Vector3 origin, float angle)
    {
        Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * guard.transform.forward;
        CastInfo c;

        if (Physics.Raycast(origin, dir, out RaycastHit h, guard.viewRange, guard.wallMask))
        {
            c.hit = true;
            c.dist = h.distance;
            c.point = origin + dir * Mathf.Max(0f, h.distance - wallPadding);
        }
        else
        {
            c.hit = false;
            c.dist = guard.viewRange;
            c.point = origin + dir * guard.viewRange;
        }
           
        return c;
    }

    void FindEdge(Vector3 origin, float minAngle, float maxAngle, CastInfo minCast, CastInfo maxCast, out Vector3 minPoint, out Vector3 maxPoint)
    {
        minPoint = minCast.point;
        maxPoint = maxCast.point;

        for(int i = 0; i < edgeIterations; i++) 
        {
            float mid = (minAngle + maxAngle) * 0.5f;

            CastInfo c = Cast(origin, mid);

            bool differs = c.hit != minCast.hit || (c.hit && minCast.hit && Mathf.Abs(minCast.dist - c.dist) > edgeDistanceThreshold);

            if(!differs)
            {
                minAngle = mid;
                minPoint = c.point;
            }
            else
            {
                maxAngle = mid;
                maxPoint = c.point;
            }
        }
    }


    public void SetColor(Color color)
    {
        material.color = color;
    }

}
