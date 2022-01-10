using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class HashIndices
{
    public int index0;
    public int index1;
    public int index2;

    public int oriIndex0;
    public int oriIndex1;
    public int oriIndex2;

    public HashIndices(int curIndex0, int curIndex1, int curIndex2)
    {
        oriIndex0 = curIndex0;
        oriIndex1 = curIndex1;
        oriIndex2 = curIndex2;

        List<int> sortIndex = new List<int>();

        sortIndex.Add(oriIndex0);
        sortIndex.Add(oriIndex1);
        sortIndex.Add(oriIndex2);

        sortIndex.Sort();

        index0 = sortIndex[0];
        index1 = sortIndex[1];
        index2 = sortIndex[2];
    }

    public override int GetHashCode()
    {
        return index0.GetHashCode() | index1.GetHashCode() | index2.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        HashIndices other = obj as HashIndices;
        return index0 == other.index0 && index1 == other.index1 && index2 == other.index2;
    }
}

public class MeshVertexInfo
{
    public Vector3 position;
    public Vector3 normal;
    public Vector2 uv;
    public BoneWeight boneWeight;

    public MeshVertexInfo(Vector3 curPos, Vector3 curNor, Vector2 curUV, BoneWeight curBW)
    {
        position = curPos;
        normal = curNor;
        uv = curUV;
        boneWeight = curBW;
    }

    //public override int GetHashCode()
    //{
    //    return position.GetHashCode() | normal.GetHashCode() | uv.GetHashCode();
    //}

    //public override bool Equals(object obj)
    //{
    //    MeshVertexInfo other = obj as MeshVertexInfo;
    //    return position == other.position && normal == other.normal && uv == other.uv;
    //}

    public override int GetHashCode()
    {
        //return position.GetHashCode() | uv.GetHashCode();
        //return (position + new Vector3(uv.x, uv.y, 0.0f)).GetHashCode();
        return uv.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        MeshVertexInfo other = obj as MeshVertexInfo;
        return position == other.position && uv == other.uv;
        //return position == other.position && normal == other.normal && uv == other.uv;
    }
}

public class MeshDataCombine : MonoBehaviour
{
    public GameObject head;

    Dictionary<string, List<string>> closedRigData;
    Dictionary<string, List<string>> groupJoints;
    List<string> remainJoint;

    Dictionary<string, GameObject> nameToGameObject;

    string[] standardJoint = { "Head", "L_Wrist", "R_Wrist", "L_Foot", "R_Foot" };

    // Start is called before the first frame update
    void Start()
    {
        //HashSet<MeshVertexInfo> hash = new HashSet<MeshVertexInfo>();
        //MeshVertexInfo one = new MeshVertexInfo { position = new Vector3(1.0f, 2.0f, 3.0f), normal = new Vector3(3.0f, 2.0f, 1.0f), uv = new Vector2(0.5f, 0.5f) };
        //MeshVertexInfo copyOfOne = new MeshVertexInfo { position = new Vector3(1.0f, 2.0f, 3.0f), normal = new Vector3(3.0f, 2.0f, 1.0f), uv = new Vector2(0.5f, 0.5f) };
        //MeshVertexInfo copyOfOne1 = new MeshVertexInfo { position = new Vector3(3.0f, 2.0f, 1.0f), normal = new Vector3(1.0f, 2.0f, 3.0f), uv = new Vector2(0.5f, 0.5f) };
        //Debug.Log(one.GetHashCode() + " " + hash.Add(one));
        //Debug.Log(copyOfOne.GetHashCode() + " " + hash.Add(copyOfOne));
        //Debug.Log(copyOfOne1.GetHashCode() + " " + hash.Add(copyOfOne1));

        //HashSet<HashIndices> his = new HashSet<HashIndices>();

        //Debug.Log(his.Add(new HashIndices(0, 1, 2)));
        //Debug.Log(his.Add(new HashIndices(0, 1, 2)));
        //Debug.Log(his.Add(new HashIndices(1, 0, 2)));
        //Debug.Log(his.Add(new HashIndices(1, 2, 0)));
        //Debug.Log(his.Add(new HashIndices(2, 0, 1)));
        //Debug.Log(his.Add(new HashIndices(2, 1, 0)));`

    }

    // Update is called once per frame
    void Update()
    {
        //if(Input.GetKeyDown(KeyCode.Alpha4))
        //{
        //    groupCombine();
        //}
    }

    public void groupCombine()
    {
        remainJoint = new List<string>();
        groupJoints = new Dictionary<string, List<string>>();

        nameToGameObject = new Dictionary<string, GameObject>();

        setClosedRigData();

        int childCount = gameObject.transform.childCount;

        for(int i = 0; i < childCount; i++)
        {
            Transform trs = gameObject.transform.GetChild(i);

            if(trs.name != "mesh")
            {
                nameToGameObject.Add(trs.name, trs.gameObject);
                remainJoint.Add(trs.name);
            }
        }

        //string jointDatas = File.ReadAllText("C:\\RigData\\jointdatas.txt");

        //string[] splitJoints = jointDatas.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        //remainJoint.AddRange(splitJoints);

        //StartCoroutine(testCor());

        standardGrouping();
    }

    void setCombineData()
    {
        Dictionary<MeshVertexInfo, int> newVertices = new Dictionary<MeshVertexInfo, int>();
        HashSet<HashIndices> newTriangles = new HashSet<HashIndices>();

        int vertCount = 0;

        List<Mesh> meshes = new List<Mesh>();

        Transform[] bones = null;
        Transform rootBone = null;
        Material combineMat = null;
        List<Matrix4x4> bindPoses = new List<Matrix4x4>();

        foreach (SkinnedMeshRenderer smr in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            meshes.Add(smr.sharedMesh);

            if(bones == null)
            {
                bones = smr.bones;
                rootBone = smr.rootBone;
                bindPoses.AddRange(smr.sharedMesh.bindposes);
                combineMat = smr.material;
            }
        }

        List<int> sortKeepIndices = new List<int>();

        int sameFaceCount = 0;

        for(int i = 0; i < meshes.Count; i++)
        {
            int beforeCount = newVertices.Count;

            Vector3[] vertices = meshes[i].vertices;
            Vector3[] normals = meshes[i].normals;
            Vector2[] uv = meshes[i].uv;
            int[] triangles = meshes[i].triangles;
            BoneWeight[] boneWeights = meshes[i].boneWeights;

            for (int j = 0; j < triangles.Length; j += 3)
            {
                sortKeepIndices.Clear();

                for (int k = 0; k < 3; k++)
                {
                    int curIndex = triangles[j + k];

                    MeshVertexInfo mvi = new MeshVertexInfo(vertices[curIndex], normals[curIndex], uv[curIndex], boneWeights[curIndex]);

                    if (newVertices.ContainsKey(mvi))
                    {
                        curIndex = newVertices[mvi];
                    }
                    else
                    {
                        newVertices.Add(mvi, vertCount);

                        curIndex = vertCount;

                        vertCount++;
                    }

                    sortKeepIndices.Add(curIndex);
                }

                if(!newTriangles.Add(new HashIndices(sortKeepIndices[0], sortKeepIndices[1], sortKeepIndices[2])))
                {
                    sameFaceCount++;
                }
            }
        }

        List<Vector3> cbVertices = new List<Vector3>();
        List<Vector3> cbNormals = new List<Vector3>();
        List<int> cbTriangles = new List<int>();
        List<Vector2> cbUV = new List<Vector2>();
        List<BoneWeight> cbBoneWeights = new List<BoneWeight>();

        foreach(KeyValuePair<MeshVertexInfo, int> newVertData in newVertices)
        {
            MeshVertexInfo mvi = newVertData.Key;

            cbVertices.Add(mvi.position);
            cbNormals.Add(mvi.normal);
            cbUV.Add(mvi.uv);
            cbBoneWeights.Add(mvi.boneWeight);
        }

        foreach(HashIndices hi in newTriangles)
        {
            cbTriangles.Add(hi.oriIndex0);
            cbTriangles.Add(hi.oriIndex1);
            cbTriangles.Add(hi.oriIndex2);
        }

        Mesh newCombineMesh = new Mesh();

        newCombineMesh.vertices = cbVertices.ToArray();
        newCombineMesh.normals = cbNormals.ToArray();
        newCombineMesh.triangles = cbTriangles.ToArray();
        newCombineMesh.uv = cbUV.ToArray();
        newCombineMesh.boneWeights = cbBoneWeights.ToArray();
        newCombineMesh.bindposes = bindPoses.ToArray();

        newCombineMesh.RecalculateNormals();

        GameObject newObj = new GameObject();
        newObj.transform.parent = transform;
        newObj.name = "combineNewChar";

        for(int i = 0; i < meshes.Count; i++)
        {
            Destroy(meshes[i]);
        }

        foreach (SkinnedMeshRenderer smr in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            Destroy(smr.gameObject);
        }

        SkinnedMeshRenderer newSmr = newObj.AddComponent<SkinnedMeshRenderer>();
        newSmr.sharedMesh = newCombineMesh;
        newSmr.material = combineMat;
        newSmr.bones = bones;
        newSmr.rootBone = rootBone;

        Debug.Log("same face = " + sameFaceCount);
        Debug.Log(vertCount + " " + newTriangles.Count);
    }

    void setCombineData(string rootName, GameObject[] meshObjects)
    {
        Dictionary<MeshVertexInfo, int> newVertices = new Dictionary<MeshVertexInfo, int>();
        HashSet<HashIndices> newTriangles = new HashSet<HashIndices>();

        int vertCount = 0;

        List<Mesh> meshes = new List<Mesh>();

        Transform[] bones = null;
        Transform rootBone = null;
        Material combineMat = null;
        List<Matrix4x4> bindPoses = new List<Matrix4x4>();

        for(int i = 0; i < meshObjects.Length; i++)
        {
            SkinnedMeshRenderer smr = meshObjects[i].GetComponent<SkinnedMeshRenderer>();

            meshes.Add(smr.sharedMesh);

            if (bones == null)
            {
                bones = smr.bones;
                rootBone = smr.rootBone;
                bindPoses.AddRange(smr.sharedMesh.bindposes);
                combineMat = smr.material;
            }
        }

        List<int> sortKeepIndices = new List<int>();

        int sameFaceCount = 0;

        for (int i = 0; i < meshes.Count; i++)
        {
            int beforeCount = newVertices.Count;

            Vector3[] vertices = meshes[i].vertices;
            Vector3[] normals = meshes[i].normals;
            Vector2[] uv = meshes[i].uv;
            int[] triangles = meshes[i].triangles;
            BoneWeight[] boneWeights = meshes[i].boneWeights;

            for (int j = 0; j < triangles.Length; j += 3)
            {
                sortKeepIndices.Clear();

                for (int k = 0; k < 3; k++)
                {
                    int curIndex = triangles[j + k];

                    MeshVertexInfo mvi = new MeshVertexInfo(vertices[curIndex], normals[curIndex], uv[curIndex], boneWeights[curIndex]);

                    if (newVertices.ContainsKey(mvi))
                    {
                        curIndex = newVertices[mvi];
                    }
                    else
                    {
                        newVertices.Add(mvi, vertCount);

                        curIndex = vertCount;

                        vertCount++;
                    }

                    sortKeepIndices.Add(curIndex);
                }

                if (!newTriangles.Add(new HashIndices(sortKeepIndices[0], sortKeepIndices[1], sortKeepIndices[2])))
                {
                    sameFaceCount++;
                }
            }
        }

        List<Vector3> cbVertices = new List<Vector3>();
        List<Vector3> cbNormals = new List<Vector3>();
        List<int> cbTriangles = new List<int>();
        List<Vector2> cbUV = new List<Vector2>();
        List<BoneWeight> cbBoneWeights = new List<BoneWeight>();

        foreach (KeyValuePair<MeshVertexInfo, int> newVertData in newVertices)
        {
            MeshVertexInfo mvi = newVertData.Key;

            cbVertices.Add(mvi.position);
            cbNormals.Add(mvi.normal);
            cbUV.Add(mvi.uv);
            cbBoneWeights.Add(mvi.boneWeight);
        }

        foreach (HashIndices hi in newTriangles)
        {
            cbTriangles.Add(hi.oriIndex0);
            cbTriangles.Add(hi.oriIndex1);
            cbTriangles.Add(hi.oriIndex2);
        }

        Mesh newCombineMesh = new Mesh();

        newCombineMesh.vertices = cbVertices.ToArray();
        newCombineMesh.normals = cbNormals.ToArray();
        newCombineMesh.triangles = cbTriangles.ToArray();
        newCombineMesh.uv = cbUV.ToArray();
        newCombineMesh.boneWeights = cbBoneWeights.ToArray();
        newCombineMesh.bindposes = bindPoses.ToArray();

        newCombineMesh.RecalculateNormals();

        GameObject newObj = new GameObject();
        newObj.transform.parent = transform;
        newObj.name = rootName;

        for (int i = 0; i < meshes.Count; i++)
        {
            Destroy(meshes[i]);
        }


        for (int i = 0; i < meshObjects.Length; i++)
        {
            Destroy(meshObjects[i]);
        }

        SkinnedMeshRenderer newSmr = newObj.AddComponent<SkinnedMeshRenderer>();
        newSmr.sharedMesh = newCombineMesh;
        newSmr.material = combineMat;
        newSmr.bones = bones;
        newSmr.rootBone = rootBone;

        Debug.Log("same face = " + sameFaceCount);
        Debug.Log(vertCount + " " + newTriangles.Count);

        if(rootName == "Head")
        {
            head = newObj;
        }
    }

    void setClosedRigData()
    {
        closedRigData = new Dictionary<string, List<string>>();

        //string data = File.ReadAllText("C:\\RigData\\newClosedRigData.txt");

        TextAsset ta = Resources.Load("newClosedRigData2") as TextAsset;
        string data = ta.text;

        string[] crds = data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < crds.Length; i++)
        {
            string[] crd = crds[i].Split(' ');

            List<string> closedData = new List<string>();

            for (int j = 0; j < crd.Length; j++)
            {
                closedData.Add(crd[j]);
            }

            closedRigData.Add(crd[0], closedData);
        }

        Dictionary<string, List<string>>.Enumerator crdEnum = closedRigData.GetEnumerator();

        while (crdEnum.MoveNext())
        {
            string deb = crdEnum.Current.Key;
            deb += " = {";

            for (int i = 0; i < crdEnum.Current.Value.Count; i++)
            {
                if (!closedRigData.ContainsKey(crdEnum.Current.Value[i]))
                {
                    Debug.Log("error");
                }
                deb += crdEnum.Current.Value[i];
                deb += " ";
            }
            deb += "}";
            Debug.Log(deb);
        }
    }

    void standardGrouping()
    {
        int index = 0;

        while (remainJoint.Count > 0 && index < standardJoint.Length)
        {
            string curName = standardJoint[index];

            List<string> rootJointData = new List<string>();

            groupJoints.Add(curName, rootJointData);

            findJoints(curName, curName);

            index++;
        }

        Debug.Log(remainJoint.Count + " " + groupJoints.Count);

        //if(remainJoint.Count > 0)
        //{
        //    Debug.Log(remainJoint[0]);
        //    groupJoints.Add("Others", remainJoint);
        //}

        foreach (KeyValuePair<string, List<string>> current in groupJoints)
        {
            List<string> grouping = current.Value;
            List<GameObject> gos = new List<GameObject>();

            string groupData = current.Key + " {";

            for (int i = 0; i < grouping.Count; i++)
            {
                gos.Add(nameToGameObject[grouping[i]]);

                groupData += grouping[i];
                if (i + 1 < grouping.Count)
                {
                    groupData += ", ";
                }
            }
            groupData += "}";

            setCombineData(current.Key, gos.ToArray());

            gos.Clear();

            Debug.Log(groupData);
        }
    }

    void findJoints(string rootJoint, string tarName)
    {
        List<string> crdJoint = closedRigData[tarName];

        for (int i = 0; i < crdJoint.Count; i++)
        {
            for (int j = 0; j < remainJoint.Count; j++)
            {
                if (crdJoint[i] == remainJoint[j])
                {
                    groupJoints[rootJoint].Add(crdJoint[i]);
                    remainJoint.RemoveAt(j);

                    if (rootJoint != crdJoint[i])
                    {
                        //Debug.Log(crdJoint[i]);
                        findJoints(rootJoint, crdJoint[i]);
                    }

                    break;
                }
            }
        }
    }
}
