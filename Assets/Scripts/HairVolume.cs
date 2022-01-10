using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HairVolume : MonoBehaviour
{
    public SkinnedMeshRenderer hair;
    public SkinnedMeshRenderer head;
    public GameObject headObj;
    public GameObject neckObj;
    public Vector3 headJoint;
    public Vector3 neckJoint;
    public List<Vector3> keepVertices;
    public float maxDis = 0.75f;
    public List<float> alphas;
    public float offset = 0.9999f;
    public int smoothLevel = 1;
    private Mesh newMesh;
    public float[] textureAlpha;
    private int width = 0;
    private int height = 0;
    private int standardPix = 256;
    // Start is called before the first frame update
    void Start()
    {
        //textureInit();
        //initialize();
        //Graphics.DrawMesh(hair.sharedMesh, transform.localToWorldMatrix, hair.material, gameObject.layer);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.V))
        {
            textureInit();
            initialize();
            calcVolumeHair();
            setWeight();
        }
    }

    public void startHairVol()
    {
        textureInit();
        initialize();
        calcVolumeHair();
        setWeight();
    }

    void setWeight()
    {
        GenerateHairWeight ghw = gameObject.AddComponent<GenerateHairWeight>();
        ghw.hair = hair.gameObject;
        ghw.initialize2();
        Destroy(ghw);
        Destroy(this);
    }

    //void calcVolumeHair()
    //{
    //    int vertexCount = newMesh.vertexCount;
    //    Vector3[] vertices = newMesh.vertices;

    //    Vector3[] headVertices = head.sharedMesh.vertices;
    //    int headVertCount = headVertices.Length;

    //    for (int i = 0; i < vertexCount; i++)
    //    {
    //        Vector3 curPosition = keepVertices[i];
    //        Vector3 normal = curPosition - headJoint;
    //        //normal.y *= 0.1f;
    //        normal.Normalize();

    //        float addOffset = 1.0f;

    //        //if(curPosition.y < headJoint.y)
    //        //{
    //        //    addOffset = Mathf.Max(0.0f, curPosition.y - neckJoint.y);

    //        //    addOffset /= (headJoint.y - neckJoint.y);
    //        //}

    //        addOffset = Mathf.Min(0.1f, Mathf.Max(0.0f, curPosition.y - headJoint.y));

    //        addOffset /= 0.1f;// (headJoint.y - neckJoint.y);

    //        float curAlpha = alphas[i];// < offset ? 0.0f : 
    //        //if (curAlpha < offset)
    //        //{
    //        //    vertices[i] = curPosition;
    //        //}
    //        //else
    //        //{
    //        //    vertices[i] = curPosition + normal * (maxDis / 100.0f) * curAlpha;
    //        //}
    //        //curAlpha -= offset;
    //        curAlpha = Mathf.Max(0.0f, curAlpha - offset);
    //        curAlpha /= (1.0f - offset);
    //        vertices[i] = curPosition + normal * (maxDis / 100.0f) * curAlpha * addOffset;
    //        //vertices[i] = headJoint + normal * curAlpha;
    //    }
    //    newMesh.vertices = vertices;
    //    Graphics.DrawMesh(newMesh, transform.localToWorldMatrix, hair.material, gameObject.layer);
    //}

    void calcVolumeHair()
    {
        int vertexCount = newMesh.vertexCount;
        Vector3[] vertices = newMesh.vertices;

        Vector3[] headVertices = head.sharedMesh.vertices;
        int headVertCount = headVertices.Length;

        for (int i = 0; i < vertexCount; i++)
        {
            Vector3 curPosition = keepVertices[i];
            Vector3 normal = curPosition - headJoint;
            //normal.y *= 0.1f;
            normal.Normalize();

            float addOffset = 1.0f;

            addOffset = Mathf.Min(0.1f, Mathf.Max(0.0f, curPosition.y - headJoint.y));

            addOffset /= 0.1f;

            float curAlpha = alphas[i];
            curAlpha = Mathf.Max(0.0f, curAlpha - offset);
            curAlpha /= (1.0f - offset);
            vertices[i] = curPosition + normal * (maxDis / 100.0f) * curAlpha * addOffset;
            vertices[i] += normal * 0.001f;
        }
        newMesh.vertices = vertices;
        hair.sharedMesh = newMesh;
        //Graphics.DrawMesh(newMesh, transform.localToWorldMatrix, hair.material, gameObject.layer);
    }

    void textureInit()
    {
        Texture2D tex = hair.material.mainTexture as Texture2D;
        width = tex.width;
        height = tex.height;

        textureAlpha = new float[standardPix * standardPix];
        float[] newTexAlpha = new float[standardPix * standardPix];

        //textureAlpha = new List<float>();
        //List<float> newTexAlpha = new List<float>();

        int ratioWidth = width / standardPix;
        int ratioHeight = height / standardPix;
        
        for (int y = 0; y < height; y++)
        {
            int yIndex = y / ratioHeight;
            yIndex *= standardPix;

            for (int x = 0; x < width; x++)
            {
                float alphaVal = tex.GetPixel(x, y).a;

                int curIndex = yIndex + (x / ratioWidth);

                textureAlpha[curIndex] += alphaVal;
            }
        }

        for (int i = 0; i < textureAlpha.Length; i++)
        {
            textureAlpha[i] /= (ratioWidth * ratioHeight);
        }

        for (int y = 0; y < standardPix; y++)
        {
            for(int x = 0; x < standardPix; x++)
            {
                float avgVal = 0.0f;
                int pixCount = 0;

                for(int yy = y - smoothLevel; yy <= y + smoothLevel; yy++)
                {
                    for (int xx = x - smoothLevel; xx <= x + smoothLevel; xx++)
                    {
                        int calcX = Mathf.Min(standardPix - 1, Mathf.Max(0, xx));
                        int calcY = Mathf.Min(standardPix - 1, Mathf.Max(0, yy));

                        int curIndex = calcY * 256 + calcX;
                        //avgIndexs.Add(curIndex);

                        avgVal += textureAlpha[curIndex];
                        pixCount++;
                    }
                }

                avgVal /= (float)pixCount;

                newTexAlpha[y * standardPix + x] = avgVal;

                //if (avgVal != 1.0f)
                //{
                //    avgVal = 0.0f;
                //}

                //for (int i = 0; i < avgIndexs.Count; i++)
                //{
                //    newTexAlpha[avgIndexs[i]] = avgVal;
                //}
            }
        }

        textureAlpha = newTexAlpha;
    }

    void initialize()
    {
        keepVertices = new List<Vector3>();

        //hair.gameObject.SetActive(false);
        headJoint = headObj.transform.position;
        neckJoint = neckObj.transform.position;
        //float diffY = headJoint.y - neckObj.transform.position.y;
        //headJoint.y += diffY;
        newMesh = new Mesh();

        Mesh hairMesh = hair.sharedMesh;
        Mesh headMesh = head.sharedMesh;

        Vector3[] headVertices = headMesh.vertices;
        int headVertCount = headVertices.Length;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> triangles = new List<int>();
        List<BoneWeight> boneWeights = new List<BoneWeight>();
        List<Matrix4x4> bindposes = new List<Matrix4x4>();

        keepVertices.AddRange(hairMesh.vertices);
        vertices.AddRange(hairMesh.vertices);
        normals.AddRange(hairMesh.normals);
        uv.AddRange(hairMesh.uv);
        triangles.AddRange(hairMesh.triangles);
        boneWeights.AddRange(hairMesh.boneWeights);
        bindposes.AddRange(hairMesh.bindposes);

        alphas = new List<float>();
        //Texture2D tex = hair.material.mainTexture as Texture2D;
        //width = tex.width;
        //height = tex.height;
        //Debug.Log(tex.name);
        for (int i = 0; i < uv.Count; i++)
        {
            int curIndex = (int)(uv[i].y * standardPix) * standardPix + (int)(uv[i].x * standardPix);
            alphas.Add(textureAlpha[curIndex]);
        }

        newMesh.vertices = vertices.ToArray();
        newMesh.normals = normals.ToArray();
        newMesh.uv = uv.ToArray();
        newMesh.triangles = triangles.ToArray();
        newMesh.boneWeights = boneWeights.ToArray();
        newMesh.bindposes = bindposes.ToArray();

        newMesh.RecalculateNormals();
    }
}
