using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexWeightInfo
{
    public WeightInfo[] weightInfo;
    public Vector2 uv;

    public VertexWeightInfo()
    {
        weightInfo = new WeightInfo[4];
        uv = Vector2.zero;
    }

    public VertexWeightInfo(WeightInfo[] cWeightInfo, Vector2 cUv)
    {
        weightInfo = cWeightInfo;
        uv = cUv;
    }

    public VertexWeightInfo(string[] boneIndexs, BoneWeight bw, Vector2 cUv)
    {
        Dictionary<int, string> indexToJoint = new Dictionary<int, string>();

        for(int i = 0; i < boneIndexs.Length; i++)
        {
            indexToJoint.Add(i, boneIndexs[i]);
        }

        weightInfo = new WeightInfo[4];

        weightInfo[0] = new WeightInfo(indexToJoint[bw.boneIndex0], bw.weight0);
        weightInfo[1] = new WeightInfo(indexToJoint[bw.boneIndex1], bw.weight1);
        weightInfo[2] = new WeightInfo(indexToJoint[bw.boneIndex2], bw.weight2);
        weightInfo[3] = new WeightInfo(indexToJoint[bw.boneIndex3], bw.weight3);
        uv = cUv;
    }
}

public class WeightInfo
{
    public WeightInfo()
    {
        name = "";
        weight = 0.0f;
    }

    public WeightInfo(string cNmae, float cWeight)
    {
        name = "";
        weight = 0.0f;
    }

    public string name;
    public float weight;
}

public class HairWeightMap : MonoBehaviour
{
    public SkinnedMeshRenderer smr;
    public SkinnedMeshRenderer smr2;
    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = smr.sharedMesh;
        Transform[] bones = smr.bones;

        List<string> boneList = new List<string>();

        for(int i = 0; i < bones.Length; i++)
        {
            boneList.Add(bones[i].name);
        }

        BoneWeight[] boneWeights = mesh.boneWeights;

        Vector2[] uv = mesh.uv;

        List<VertexWeightInfo> vwis = new List<VertexWeightInfo>();

        float uvMinX = float.MaxValue;
        float uvMaxX = float.MinValue;

        float uvMinY = float.MaxValue;
        float uvMaxY = float.MinValue;

        for (int i = 0; i < uv.Length; i++)
        {
            Vector2 curUV = uv[i];

            if(curUV.x < uvMinX)
            {
                uvMinX = curUV.x;
            }
            if (curUV.x > uvMaxX)
            {
                uvMaxX = curUV.x;
            }

            if (curUV.y < uvMinY)
            {
                uvMinY = curUV.y;
            }
            if (curUV.y > uvMaxY)
            {
                uvMaxY = curUV.y;
            }

            vwis.Add(new VertexWeightInfo(boneList.ToArray(), boneWeights[i], uv[i]));
        }

        Debug.Log(vwis.Count + " " + uvMinX + " " + uvMaxX + " " + uvMinY + " " + uvMaxY);

        Mesh mesh2 = smr2.sharedMesh;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv2 = new List<Vector2>();
        List<BoneWeight> newBoneWeight = new List<BoneWeight>();
        List<Matrix4x4> bindposes = new List<Matrix4x4>();

        vertices.AddRange(mesh2.vertices);
        normals.AddRange(mesh2.normals);
        triangles.AddRange(mesh2.triangles);
        uv2.AddRange(mesh2.uv);
        bindposes.AddRange(mesh2.bindposes);

        for (int i = 0; i < uv2.Count; i++)
        {
            Vector2 tUV = uv2[i];

            float minDis = float.MaxValue;
            BoneWeight keepBoneWeight = new BoneWeight();

            for(int j = 0; j < uv.Length; j++)
            {
                Vector2 sUV = uv[j];

                float curDis = Vector2.Distance(tUV, sUV);

                if(curDis < minDis)
                {
                    BoneWeight curBW = boneWeights[j];
                    minDis = curDis;
                    keepBoneWeight.boneIndex0 = curBW.boneIndex0;
                    keepBoneWeight.boneIndex1 = curBW.boneIndex1;
                    keepBoneWeight.boneIndex2 = curBW.boneIndex2;
                    keepBoneWeight.boneIndex3 = curBW.boneIndex3;

                    keepBoneWeight.weight0 = curBW.weight0;
                    keepBoneWeight.weight1 = curBW.weight1;
                    keepBoneWeight.weight2 = curBW.weight2;
                    keepBoneWeight.weight3 = curBW.weight3;
                }
            }

            newBoneWeight.Add(keepBoneWeight);
        }

        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices.ToArray();
        newMesh.normals = normals.ToArray();
        newMesh.triangles = triangles.ToArray();
        newMesh.uv = uv2.ToArray();
        newMesh.boneWeights = newBoneWeight.ToArray();
        newMesh.bindposes = bindposes.ToArray();

        smr2.sharedMesh = newMesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
