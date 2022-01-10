using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointBoundary
{
    Vector3 srcPos;
    Vector3 tarPos;
    Vector3 tarNormal;

    float minX;
    float minY;
    float minZ;

    float maxX;
    float maxY;
    float maxZ;

    public PointBoundary(Vector3 cSrcPos, Vector3 cTarPos)
    {
        srcPos = cSrcPos;
        tarPos = cTarPos;

        tarNormal = tarPos - srcPos;
        tarNormal.Normalize();

        Vector3 normal = tarPos - srcPos;
        Vector3 newPos = tarPos + normal;

        minX = Mathf.Min(srcPos.x, newPos.x);
        minY = Mathf.Min(srcPos.y, newPos.y);
        minZ = Mathf.Min(srcPos.z, newPos.z);

        maxX = Mathf.Max(srcPos.x, newPos.x);
        maxY = Mathf.Max(srcPos.y, newPos.y);
        maxZ = Mathf.Max(srcPos.z, newPos.z);
    }

    public bool isInner(Vector3 curPos)
    {
        return (minX <= curPos.x && curPos.x <= maxX) &&
            (minY <= curPos.y && curPos.y <= maxY) &&
            (minZ <= curPos.z && curPos.z <= maxZ);
    }

    public float isInnerDot(Vector3 curPos)
    {
        float returnVal = -1.0f;

        //if (isInner(curPos))
        //{
            Vector3 keepNormal = curPos - srcPos;
            keepNormal.Normalize();

            returnVal = Vector3.Dot(tarNormal, keepNormal);
        //}

        return returnVal;
    }

    public void draw()
    {
        Debug.DrawLine(new Vector3(minX, minY, minZ), new Vector3(maxX, minY, minZ), Color.yellow);
        Debug.DrawLine(new Vector3(minX, minY, minZ), new Vector3(minX, maxY, minZ), Color.yellow);
        Debug.DrawLine(new Vector3(minX, minY, minZ), new Vector3(minX, minY, maxZ), Color.yellow);

        Debug.DrawLine(new Vector3(maxX, maxY, maxZ), new Vector3(minX, maxY, maxZ), Color.yellow);
        Debug.DrawLine(new Vector3(maxX, maxY, maxZ), new Vector3(maxX, minY, maxZ), Color.yellow);
        Debug.DrawLine(new Vector3(maxX, maxY, maxZ), new Vector3(maxX, maxY, minZ), Color.yellow);
    }
}

public class GenPointBox : MonoBehaviour
{
    public GameObject head;
    public GameObject body;
    public GameObject hair;
    Vector3 headPos;

    // Start is called before the first frame update
    void Start()
    {
        headPos = head.transform.position;

        Mesh bodyMesh = Initialize(body);
        Mesh hairMesh = Initialize(hair);

        hair.GetComponent<SkinnedMeshRenderer>().sharedMesh = hairWeightCopy(hairMesh, bodyMesh);
    }

    // Update is called once per frame
    void Update()
    {
        ;
    }

    Mesh hairWeightCopy(Mesh hairMesh, Mesh bodyMesh)
    {
        Vector3[] hairVertices = hairMesh.vertices;
        BoneWeight[] hairBoneWeight = hairMesh.boneWeights;

        Vector3[] bodyVertices = bodyMesh.vertices;
        BoneWeight[] bodyBoneWeight = bodyMesh.boneWeights;

        int hairCount = hairVertices.Length;
        int bodyCount = bodyVertices.Length;

        for (int i = 0; i < hairCount; i++)
        {
            PointBoundary pb = new PointBoundary(headPos, hairVertices[i]);

            float maxDot = float.MinValue;
            int maxIndex = -1;

            for(int j = 0; j < bodyCount; j++)
            {
                float curDot = pb.isInnerDot(bodyVertices[j]);

                if(maxDot < curDot)
                {
                    maxDot = curDot;
                    maxIndex = j;
                }
            }

            hairBoneWeight[i] = bodyBoneWeight[maxIndex];
        }

        hairMesh.boneWeights = hairBoneWeight;

        return hairMesh;
    }

    Mesh Initialize(GameObject curObj)
    {
        SkinnedMeshRenderer smr = curObj.GetComponent<SkinnedMeshRenderer>();

        Mesh mesh = smr.sharedMesh;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv = new List<Vector2>();
        List<BoneWeight> boneWeights = new List<BoneWeight>();
        List<Matrix4x4> bindposes = new List<Matrix4x4>();

        vertices.AddRange(mesh.vertices);
        normals.AddRange(mesh.normals);
        triangles.AddRange(mesh.triangles);
        uv.AddRange(mesh.uv);
        boneWeights.AddRange(mesh.boneWeights);
        bindposes.AddRange(mesh.bindposes);

        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices.ToArray();
        newMesh.normals = normals.ToArray();
        newMesh.triangles = triangles.ToArray();
        newMesh.uv = uv.ToArray();
        newMesh.boneWeights = boneWeights.ToArray();
        newMesh.bindposes = bindposes.ToArray();

        smr.sharedMesh = newMesh;

        return newMesh;
    }
}
