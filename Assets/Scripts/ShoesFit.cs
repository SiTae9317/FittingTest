using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoesFit : MonoBehaviour
{
    [HideInInspector]
    public GameObject charObj;
    [HideInInspector]
    public GameObject shoes;
    private GameObject[] shoesAnkle;
    private GameObject[] ankle;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //if(Input.GetKeyDown(KeyCode.S))
        //{
        //    setShoes();
        //    genMesh();
        //}
    }

    public void genMesh()
    {
        SkinnedMeshRenderer smr = shoes.GetComponent<SkinnedMeshRenderer>();
        Mesh mesh = smr.sharedMesh;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv = new List<Vector2>();
        List<BoneWeight> boneWeights = new List<BoneWeight>();
        List<Matrix4x4> bindposes = new List<Matrix4x4>();
        List<Matrix4x4> boneMatrices = new List<Matrix4x4>();

        Transform[] bones = smr.bones;
        Transform rootBone = smr.rootBone;

        for (int i = 0; i < bones.Length; i++)
        {
            boneMatrices.Add(bones[i].localToWorldMatrix);
        }

        SkinnedMeshRenderer charSmr = charObj.GetComponent<SkinnedMeshRenderer>();
        Mesh charMesh = charSmr.sharedMesh;
        Transform[] charBones = charSmr.bones;
        List<Matrix4x4> charBindposes = new List<Matrix4x4>();
        charBindposes.AddRange(charMesh.bindposes);

        vertices.AddRange(mesh.vertices);
        normals.AddRange(mesh.normals);
        triangles.AddRange(mesh.triangles);
        uv.AddRange(mesh.uv);
        boneWeights.AddRange(mesh.boneWeights);
        bindposes.AddRange(mesh.bindposes);

        //Debug.Log(bindposes.Count + " " + bones.Length);
        //Debug.Log(charBindposes.Count + " " + charBones.Length);

        //for (int i = 0; i < bindposes.Count; i++)
        //{
        //    Debug.Log(bones[i].name == charBones[i].name);
        //}

        for (int i = 0; i < boneMatrices.Count; i++)
        {
            boneMatrices[i] *= bindposes[i];
        }

        for (int i = 0; i < vertices.Count; i++)
        {
            BoneWeight weight = boneWeights[i];

            Matrix4x4 bm0 = boneMatrices[weight.boneIndex0];
            Matrix4x4 bm1 = boneMatrices[weight.boneIndex1];
            Matrix4x4 bm2 = boneMatrices[weight.boneIndex2];
            Matrix4x4 bm3 = boneMatrices[weight.boneIndex3];

            Matrix4x4 vertexMatrix = new Matrix4x4();

            for (int n = 0; n < 16; n++)
            {
                vertexMatrix[n] =
                    bm0[n] * weight.weight0 +
                    bm1[n] * weight.weight1 +
                    bm2[n] * weight.weight2 +
                    bm3[n] * weight.weight3;
            }

            vertices[i] = vertexMatrix.MultiplyPoint3x4(vertices[i]);
            normals[i] = vertexMatrix.MultiplyVector(normals[i]);
        }

        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices.ToArray();
        newMesh.normals = normals.ToArray();
        newMesh.triangles = triangles.ToArray();
        newMesh.uv = uv.ToArray();
        newMesh.boneWeights = boneWeights.ToArray();
        newMesh.bindposes = charBindposes.ToArray();//bindposes.ToArray();

        smr.rootBone = rootBone;
        smr.bones = charBones;
        smr.sharedMesh = newMesh;

        GameObject destroyObj = shoes.transform.parent.gameObject;

        shoes.transform.parent = charObj.transform.parent;
        Destroy(destroyObj);

        //GameObject go = new GameObject();
        //go.AddComponent<MeshFilter>().mesh = newMesh;
        //go.AddComponent<MeshRenderer>().material = smr.material;
    }

    public void setShoes()
    {
        ankle = new GameObject[2];
        shoesAnkle = new GameObject[2];

        foreach(Transform tf in charObj.GetComponent<SkinnedMeshRenderer>().bones)
        {
            if (tf.name.Equals("L_Ankle"))
            {
                ankle[0] = tf.gameObject;
            }
            else if (tf.name.Equals("R_Ankle"))
            {
                ankle[1] = tf.gameObject;
            }
        }

        foreach (Transform tf in shoes.GetComponent<SkinnedMeshRenderer>().bones)
        {
            if(tf.name.Equals("L_Ankle"))
            {
                shoesAnkle[0] = tf.gameObject;
            }
            else if(tf.name.Equals("R_Ankle"))
            {
                shoesAnkle[1] = tf.gameObject;
            }
        }

        for (int i = 0; i < 2; i++)
        {
            Vector3 curAnkle = ankle[i].transform.position;

            shoesAnkle[i].transform.position = curAnkle;

            Vector3 shoesChild = shoesAnkle[i].transform.GetChild(0).position;
            Vector3 humanChild = ankle[i].transform.GetChild(0).position;

            Vector3 shoesNormal = shoesChild - curAnkle;
            Vector3 humanNormal = humanChild - curAnkle;

            float scaleVal = humanNormal.magnitude / shoesNormal.magnitude;
            scaleVal *= 1.15f;

            shoesNormal.y = 0.0f;
            humanNormal.y = 0.0f;

            shoesNormal.Normalize();
            humanNormal.Normalize();

            Debug.Log(Vector3.Angle(shoesNormal, humanNormal));
            shoesAnkle[i].transform.localEulerAngles = new Vector3(0.0f, Vector3.Angle(shoesNormal, humanNormal), 0.0f);
            shoesAnkle[i].transform.localScale *= scaleVal;
        }
    }
}
