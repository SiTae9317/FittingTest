using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendRestore : MonoBehaviour
{
    public bool isMan = true;
    public GameObject head;
    //public GameObject point;

    private Dictionary<string, Dictionary<MeshVertexInfo, Vector3>> blendShapeDatas;

    //public Vector3[] notFindVertex = { new Vector3(0.00983091f, 1.699849f, 0.08566945f),
    //                            new Vector3(0.008461061f, 1.700696f, 0.08660623f),
    //                            new Vector3(- 0.01050093f, 1.72981f, 0.08300195f),
    //                            new Vector3(- 0.00806833f, 1.711305f, 0.08785563f),
    //                            new Vector3(- 0.01413967f, 1.72055f, 0.08685783f),
    //                            new Vector3(- 0.009333638f, 1.701159f, 0.08553503f),
    //                            new Vector3(- 0.0117852f, 1.704311f, 0.08536241f)};

    //public Vector2[] notFindUV = { new Vector2(0.2058365f, 0.573337f),
    //                            new Vector2(0.207878f, 0.57781f),
    //                            new Vector2(0.2065425f, 0.6466263f),
    //                            new Vector2(0.215839f, 0.6127546f),
    //                            new Vector2(0.2158589f, 0.6318749f),
    //                            new Vector2(0.225334f, 0.606358f),
    //                            new Vector2(0.226013f, 0.614817f)};

    // Start is called before the first frame update
    void Start()
    {
        //initialize();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha9))
        {
            blendCopy();
        }
    }

    public void blendCopy()
    {
        SkinnedMeshRenderer smr = head.GetComponent<SkinnedMeshRenderer>();
        Mesh mesh = smr.sharedMesh;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> triangles = new List<int>();
        List<BoneWeight> boneWeights = new List<BoneWeight>();
        List<Matrix4x4> bindposes = new List<Matrix4x4>();

        vertices.AddRange(mesh.vertices);
        normals.AddRange(mesh.normals);
        uv.AddRange(mesh.uv);
        triangles.AddRange(mesh.triangles);
        boneWeights.AddRange(mesh.boneWeights);
        bindposes.AddRange(mesh.bindposes);

        Dictionary<MeshVertexInfo, List<int>> vertToIndex = new Dictionary<MeshVertexInfo, List<int>>();

        int vertexCount = mesh.vertexCount;

        for (int i = 0; i < vertexCount; i++)
        {
            MeshVertexInfo mvi = new MeshVertexInfo(vertices[i], normals[i], uv[i], boneWeights[i]);

            if (!vertToIndex.ContainsKey(mvi))
            {
                //if(i==386)
                //{
                //    Debug.Log(mvi.GetHashCode());
                //    Debug.Log("add");
                //}
                List<int> newIndex = new List<int>();
                newIndex.Add(i);
                vertToIndex.Add(mvi, newIndex);
            }
            else
            {
                //if (i == 386)
                //{
                //    Debug.Log("not add");
                //}
                vertToIndex[mvi].Add(i);
            }

            //if (vertices[i] == notFindVertex[0])
            //{
            //    Debug.Log("find vertex " + i);
            //}
            //if (uv[i] == notFindUV[0])
            //{
            //    Debug.Log("find uv " + i);
            //}

            //if(i == 386)
            //{
            //    MeshVertexInfo mvi2 = new MeshVertexInfo(notFindVertex[0], Vector3.zero, notFindUV[0], new BoneWeight());
            //    Debug.Log(mvi2.GetHashCode());

            //    Debug.Log(vertToIndex.ContainsKey(mvi2));

            //}
        }

        //Debug.Log(vertToIndex.ContainsKey(new MeshVertexInfo(notFindVertex[0], Vector3.zero, notFindUV[0], new BoneWeight())));
        //MeshVertexInfo mvi = new MeshVertexInfo(notFindVertex[0], Vector3.zero, notFindUV[0], new BoneWeight());
        //Debug.Log(vertex)

        //Debug.Log(vertToIndex.Count + " " + vertexCount);

        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices.ToArray();
        newMesh.normals = normals.ToArray();
        newMesh.triangles = triangles.ToArray();
        newMesh.uv = uv.ToArray();
        newMesh.boneWeights = boneWeights.ToArray();
        newMesh.bindposes = bindposes.ToArray();

        HashSet<string> blendInfoName = new HashSet<string>();

        TextAsset ta = Resources.Load<TextAsset>("BlendName");
        string[] bins = ta.text.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        for(int i = 0; i < bins.Length; i++)
        {
            blendInfoName.Add(bins[i]);
        }

        foreach (KeyValuePair<string, Dictionary<MeshVertexInfo, Vector3>> current in blendShapeDatas)
        {
            string blendName = current.Key;
            Dictionary<MeshVertexInfo, Vector3> blendData = current.Value;

            Vector3[] deltaVertex = new Vector3[vertexCount];
            Vector3[] deltaNormal = new Vector3[vertexCount];
            Vector3[] deltaTangent = new Vector3[vertexCount];

            int notFoundIndex = 0;

            string biName = "base/mobile_51.";

            foreach (KeyValuePair<MeshVertexInfo, Vector3> dataCur in blendData)
            {
                MeshVertexInfo mvi = dataCur.Key;
                Vector3 blendVal = dataCur.Value;

                if(vertToIndex.ContainsKey(mvi))
                {
                    List<int> indexs = vertToIndex[mvi];
                    for(int i = 0; i < indexs.Count; i++)
                    {
                        deltaVertex[indexs[i]] = blendVal;
                    }
                }
                else
                {
                    //GameObject go = Instantiate(point);
                    //go.name = mvi.position.x + "," + mvi.position.y + "," + mvi.position.z + "," + mvi.uv.x + "," + mvi.uv.y;
                    //go.transform.parent = smr.transform;
                    //go.transform.localPosition = mvi.position;

                    notFoundIndex++;
                }
            }

            if (blendInfoName.Contains(blendName))
            {
                biName = "base/visemes_15.";
            }

            blendName = biName + blendName;

            Debug.Log(blendName + " " + notFoundIndex);

            newMesh.AddBlendShapeFrame(blendName, 100, deltaVertex, deltaNormal, deltaTangent);

            //break;
        }

        smr.sharedMesh = newMesh;

        head.transform.parent = head.transform.parent.parent.parent;
        head.name = "mesh";
    }

    public void initialize()
    {
        blendShapeDatas = new Dictionary<string, Dictionary<MeshVertexInfo, Vector3>>();

        SkinnedMeshRenderer smr = head.GetComponent<SkinnedMeshRenderer>();

        Mesh mesh = smr.sharedMesh;

        int vertexCount = mesh.vertexCount;

        int blendCount = mesh.blendShapeCount;

        Debug.Log("blend count = " + blendCount);

        Vector3[] deltaVertices = new Vector3[vertexCount];

        Vector3[] vertices = mesh.vertices;
        Vector3[] normal = mesh.normals;
        Vector2[] uv = mesh.uv;
        BoneWeight[] boneWeights = mesh.boneWeights;

        //List<List<Vector3>> useVertexData = new List<List<Vector3>>();

        for(int i = 0; i < blendCount; i++)
        {
            string blendName = mesh.GetBlendShapeName(i);
            int curFrameCount = mesh.GetBlendShapeFrameCount(i);

            mesh.GetBlendShapeFrameVertices(i, curFrameCount - 1, deltaVertices, null, null);

            //List<Vector3> useBlendVertices = new List<Vector3>();

            Dictionary<MeshVertexInfo, Vector3> curVertToBlend = new Dictionary<MeshVertexInfo, Vector3>();

            for (int j = 0; j < vertexCount; j++)
            {
                Vector3 curVertex = deltaVertices[j];

                if (curVertex != Vector3.zero)
                {
                    MeshVertexInfo mvi = new MeshVertexInfo(vertices[j], normal[j], uv[j], boneWeights[j]);
                    if(!curVertToBlend.ContainsKey(mvi))
                    {
                        curVertToBlend.Add(mvi, curVertex);
                    }
                    else
                    {
                        if(curVertToBlend[mvi] != curVertex)
                        {
                            Debug.Log("error");
                        }
                    }
                }
            }

            blendShapeDatas.Add(blendName, curVertToBlend);
        }
        Debug.Log("copy blend count = " + blendShapeDatas.Count);

        Texture2D skinTex = smr.material.mainTexture as Texture2D;
        string resourcePath = "Textures/";

        if(isMan)
        {
            resourcePath += "Man_Alpha_map";
        }
        else
        {
            resourcePath += "Woman_Alpha_map";
        }

        Texture2D alphaTex = Resources.Load(resourcePath) as Texture2D;

        List<Color> skinData = new List<Color>();
        skinData.AddRange(skinTex.GetPixels(0));
        Color[] alphaData = alphaTex.GetPixels(0);

        for (int i = 0; i < skinData.Count; i++)
        {
            Color keepColor = skinData[i];
            Color keepAlphaColor = alphaData[i];

            float alphaVal = alphaData[i].a;
            float oriVal = 1.0f - alphaVal;

            keepColor.r = (keepColor.r * oriVal) + (keepAlphaColor.r * alphaVal);
            keepColor.g = (keepColor.g * oriVal) + (keepAlphaColor.g * alphaVal);
            keepColor.b = (keepColor.b * oriVal) + (keepAlphaColor.b * alphaVal);

            skinData[i] = keepColor;
        }

        Texture2D newTex = new Texture2D(skinTex.width, skinTex.height);
        newTex.SetPixels(skinData.ToArray());
        newTex.Apply();

        smr.material.mainTexture = newTex;
    }
}
