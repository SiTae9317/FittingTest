using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class JointSector
//{
//    public JointSector(Vector3 cHead, Vector3 cNeck, Vector3 cSpine, float )
//    {
//        head = cHead;
//        neck = cNeck;
//        spine = cSpine;
//    }

//    public BoneWeight genWeight(Vector3 curPos)
//    {
//        BoneWeight bw = new BoneWeight();

//        return bw;
//    }

//    Vector3 head;
//    Vector3 neck;
//    Vector3 spine;
//}

public class NameNPos
{
    public NameNPos(string cName, Vector3 cPos, float cDis)
    {
        name = cName;
        pos = cPos;
        dis = cDis;
    }

    public string name;
    public Vector3 pos;
    public float dis;
}

public class GenerateHairWeight : MonoBehaviour
{
    [HideInInspector]
    public GameObject hair;

    // Start is called before the first frame update
    void Start()
    {
        ;
    }

    // Update is called once per frame
    void Update()
    {
        //if(Input.GetKeyDown(KeyCode.W))
        //{
        //    initialize2();
        //}
    }

    void initialize()
    {
        SkinnedMeshRenderer smr = hair.GetComponent<SkinnedMeshRenderer>();

        Transform[] bones = smr.bones;
        Dictionary<Vector3, string> positionToName = new Dictionary<Vector3, string>();
        Dictionary<string, Vector3> nameToPosition = new Dictionary<string, Vector3>();
        Dictionary<string, int> nameToIndex = new Dictionary<string, int>();

        Vector3 keepPosition = Vector3.zero;

        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i].name.Equals("Head"))
            {
                keepPosition = bones[i].transform.position;
                break;
            }
        }

        for (int i = 0; i < bones.Length; i++)
        {
            Vector3 curPos = bones[i].position;
            string curName = bones[i].name;

            if (curName.Equals("Head") || curName.Equals("Neck") || curName.Equals("L_Collar") || curName.Equals("R_Collar") || curName.Equals("Spine3"))
            {
                curPos.z = keepPosition.z;

                Debug.Log(curPos.x + " " + curPos.y + " " + curPos.z + " " + curName + " " + i);
                positionToName.Add(curPos, curName);
                nameToPosition.Add(curName, curPos);
                nameToIndex.Add(curName, i);
            }
        }

        //keepPosition = nameToPosition["Head"];//
        //keepPosition.y += Mathf.Abs(keepPosition.y - nameToPosition["Neck"].y) / 2.0f;

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

        List<NameNPos> sortData = new List<NameNPos>();

        for (int i = 0; i < boneWeights.Count; i++)
        {
            Vector3 curVertices = vertices[i];

            sortData.Clear();

            foreach (KeyValuePair<Vector3, string> current in positionToName)
            {
                Vector3 tempVec = current.Key;
                string tempName = current.Value;

                float curDis = Vector3.Distance(tempVec, curVertices);
                //float curDis = Mathf.Abs(tempVec.y - curVertices.y);

                sortData.Add(new NameNPos(tempName, tempVec, curDis));
            }

            sortData.Sort(delegate (NameNPos lNnp, NameNPos rNnp)
            {
                if (lNnp.dis > rNnp.dis) return 1;
                else if (lNnp.dis < rNnp.dis) return -1;
                return 0;
            });

            float diffRatio = sortData[0].dis / sortData[1].dis;

            //if (0.8f < diffRatio)
            //{
                sortData[0].dis = Mathf.Max(0.0f, Mathf.Min(1.0f, (2.0f - diffRatio) / 2.0f));
                sortData[1].dis = Mathf.Max(0.0f, Mathf.Min(1.0f, diffRatio / 2.0f));
            //}
            //else
            //{
            //    sortData[0].dis = 1.0f;
            //    sortData[1].dis = 0.0f;
            //}

            //if (sortData[0].pos.y < curVertices.y)
            //{
            //    sortData[0].dis = 1.0f;
            //    sortData[1].dis = 0.0f;
            //}

            if (curVertices.y > keepPosition.y)
            {
                sortData[0].dis = 1.0f;
                sortData[1].dis = 0.0f;
            }

            BoneWeight bw = boneWeights[i];
            bw.boneIndex0 = nameToIndex[sortData[0].name];
            bw.boneIndex1 = nameToIndex[sortData[1].name];
            bw.boneIndex2 = 0;
            bw.boneIndex3 = 0;

            bw.weight0 = sortData[0].dis;
            bw.weight1 = sortData[1].dis;
            bw.weight2 = 0.0f;
            bw.weight3 = 0.0f;

            boneWeights[i] = bw;
        }

        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices.ToArray();
        newMesh.normals = normals.ToArray();
        newMesh.triangles = triangles.ToArray();
        newMesh.uv = uv.ToArray();
        newMesh.boneWeights = boneWeights.ToArray();
        newMesh.bindposes = bindposes.ToArray();

        smr.sharedMesh = newMesh;

    }

    public void initialize2()
    {
        SkinnedMeshRenderer smr = hair.GetComponent<SkinnedMeshRenderer>();

        Transform[] bones = smr.bones;
        Dictionary<Vector3, string> positionToName = new Dictionary<Vector3, string>();
        Dictionary<string, Vector3> nameToPosition = new Dictionary<string, Vector3>();
        Dictionary<string, int> nameToIndex = new Dictionary<string, int>();
        
        //for (int i = 0; i < bones.Length; i++)
        //{
        //    if (bones[i].name.Equals("Head"))
        //    {
        //        keepPosition = bones[i].transform.position;
        //        break;
        //    }
        //}

        for (int i = 0; i < bones.Length; i++)
        {
            Vector3 curPos = bones[i].position;
            string curName = bones[i].name;

            if (curName.Equals("Head") || curName.Equals("Neck") || curName.Equals("L_Collar") || curName.Equals("R_Collar") || curName.Equals("Spine3"))
            {
                //curPos.z = keepPosition.z;

                Debug.Log(curPos.x + " " + curPos.y + " " + curPos.z + " " + curName + " " + i);
                positionToName.Add(curPos, curName);
                nameToPosition.Add(curName, curPos);
                nameToIndex.Add(curName, i);
            }
        }

        //float keepZ = nameToPosition["Head"].z;

        float addVal = Mathf.Abs(nameToPosition["Head"].y - nameToPosition["Neck"].y)  * 2.0f / 3.0f;
        //Vector3 spineVec = nameToPosition["Spine3"];
        //spineVec.y += addVal * 2.0f;
        //nameToPosition["Spine3"] = spineVec;

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

        List<NameNPos> sortData = new List<NameNPos>();

        for (int i = 0; i < boneWeights.Count; i++)
        {
            Vector3 curVertices = vertices[i];

            sortData.Clear();

            foreach (KeyValuePair<Vector3, string> current in positionToName)
            {
                Vector3 tempVec = current.Key;
                string tempName = current.Value;

                //tempVec.y += addVal;
                //tempVec.z = keepZ;
                //float curDis = Vector3.Distance(tempVec, curVertices);

                float curDis = Mathf.Abs((tempVec.y + addVal) - curVertices.y);

                sortData.Add(new NameNPos(tempName, tempVec, curDis));
            }

            sortData.Sort(delegate (NameNPos lNnp, NameNPos rNnp)
            {
                if (lNnp.dis > rNnp.dis) return 1;
                else if (lNnp.dis < rNnp.dis) return -1;
                return 0;
            });

            float diffRatio = sortData[0].dis / sortData[1].dis;

            //if (0.8f < diffRatio)
            //{
            sortData[0].dis = Mathf.Max(0.0f, Mathf.Min(1.0f, (2.0f - diffRatio) / 2.0f));
            sortData[1].dis = Mathf.Max(0.0f, Mathf.Min(1.0f, diffRatio / 2.0f));
            //}
            //else
            //{
            //    sortData[0].dis = 1.0f;
            //    sortData[1].dis = 0.0f;
            //}

            //if (sortData[0].pos.y < curVertices.y)
            //{
            //    sortData[0].dis = 1.0f;
            //    sortData[1].dis = 0.0f;
            //}

            if (curVertices.y > nameToPosition["Head"].y + addVal)
            {
                sortData[0].dis = 1.0f;
                sortData[1].dis = 0.0f;
            }

            for(int j = 0; j < 2; j++)
            {
                if (sortData[j].name.Contains("Collar"))
                {
                    float left = Vector3.Distance(curVertices, nameToPosition["L_Collar"]);
                    float right = Vector3.Distance(curVertices, nameToPosition["R_Collar"]);

                    if (left <= right)
                    {
                        sortData[j].name = "L_Collar";
                    }
                    else
                    {
                        sortData[j].name = "R_Collar";
                    }

                    //float lrData = curVertices.x - nameToPosition["Head"].x;
                    //if (lrData < 0.0f)
                    //{
                    //    sortData[j].name = "L_Collar";
                    //}
                    //else
                    //{
                    //    sortData[j].name = "R_Collar";
                    //}
                }
            }

            BoneWeight bw = boneWeights[i];
            bw.boneIndex0 = nameToIndex[sortData[0].name];
            bw.boneIndex1 = nameToIndex[sortData[1].name];
            bw.boneIndex2 = 0;
            bw.boneIndex3 = 0;

            bw.weight0 = sortData[0].dis;
            bw.weight1 = sortData[1].dis;
            bw.weight2 = 0.0f;
            bw.weight3 = 0.0f;

            boneWeights[i] = bw;
        }

        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices.ToArray();
        newMesh.normals = normals.ToArray();
        newMesh.triangles = triangles.ToArray();
        newMesh.uv = uv.ToArray();
        newMesh.boneWeights = boneWeights.ToArray();
        newMesh.bindposes = bindposes.ToArray();

        smr.sharedMesh = newMesh;

    }
}
