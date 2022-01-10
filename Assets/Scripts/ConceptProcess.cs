using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ConceptProcess : MonoBehaviour
{
    public GameObject controlBody;
    public GameObject cam;
    public SkinnedMeshRenderer oriModel;
    public SkinnedMeshRenderer[] cloth;

    public SkinnedMeshRenderer smileBlend;
    public int smileIndex;

    public RuntimeAnimatorController[] anis;

    private List<GameObject> chars;

    public int[] cloth0 = new int []{ 0 };
    public int[] cloth1 = new int[] { 1, 2, 3 };
    public int[] cloth2 = new int[] { 1, 2, 4 };

    public List<int[]> clothIndexs = null;

    public float[] actionDelay;

    public Queue<IEnumerator> actionQueue;

    // Start is called before the first frame update
    void Start()
    {
        actionQueue = new Queue<IEnumerator>();
        init();
        setIndexs(0);

        //for(int i = 0; i < actionDelay.Length; i++)
        //{
        //    actionQueue.Enqueue(delayAction(actionDelay[i], null));
        //}

        actionQueue.Enqueue(delayAction(actionDelay[0], objectEnable(0.0f)));
        actionQueue.Enqueue(delayAction(actionDelay[1], customBody()));
        actionQueue.Enqueue(delayAction(actionDelay[2], heightBlend()));
        actionQueue.Enqueue(delayAction(actionDelay[3], weightBlend()));
        actionQueue.Enqueue(delayAction(actionDelay[4], returnBody()));
        actionQueue.Enqueue(delayAction(actionDelay[5], rotateZero()));
        actionQueue.Enqueue(delayAction(actionDelay[6], clothChangeAni(0.0f, 1)));
        actionQueue.Enqueue(delayAction(actionDelay[6], clothChangeAni(0.0f, 2)));
        actionQueue.Enqueue(delayAction(actionDelay[7], rotateBack()));

        StartCoroutine(corQueue());
    }

    IEnumerator rotateBack()
    {
        Vector3 keepRot = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 keepPos = new Vector3(0.0f, 0.10f, 0.0f);

        Vector3 targetRot = new Vector3(0.0f, -15.0f, 0.0f);
        Vector3 targetPos = new Vector3(0.0f, 0.0f, 0.0f);

        float speed = 2.0f;
        Vector3 normal = targetRot - keepRot;

        float dis = Vector3.Distance(targetRot, keepRot);
        normal.Normalize();

        Debug.Log(dis);

        float sumtime = 0.0f;
        while (sumtime < 1.0f / speed)
        {
            yield return null;
            //controlBody.transform.localEulerAngles = keepRot + normal * dis * Mathf.Pow(sumtime * speed, 2.0f);//Vector3.Lerp(controlBody.transform.localEulerAngles, targetRot, Mathf.Pow(sumtime * speed, 2.0f));
            cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, Mathf.Pow(sumtime * speed, 2.0f));
            sumtime += Time.deltaTime;
            Debug.Log(sumtime);
        }
        //controlBody.transform.localEulerAngles = Vector3.Lerp(controlBody.transform.localEulerAngles, Vector3.zero, 1.0f);
        cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, 1.0f);
    }

    IEnumerator rotateZero()
    {
        Vector3 keepRot = new Vector3(0.0f, -15.0f, 0.0f);
        Vector3 keepPos = cam.transform.position;

        Vector3 targetRot = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 targetPos = new Vector3(0.0f, 0.10f, 0.0f);

        float speed = 2.0f;
        Vector3 normal = targetRot - keepRot;

        float dis = Vector3.Distance(targetRot, keepRot);
        normal.Normalize();

        Debug.Log(dis);

        float sumtime = 0.0f;
        while (sumtime < 1.0f / speed)
        {
            yield return null;
            controlBody.transform.localEulerAngles = keepRot + normal * dis * Mathf.Pow(sumtime * speed, 2.0f);//Vector3.Lerp(controlBody.transform.localEulerAngles, targetRot, Mathf.Pow(sumtime * speed, 2.0f));
            cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, Mathf.Pow(sumtime * speed, 2.0f));
            sumtime += Time.deltaTime;
            Debug.Log(sumtime);
        }
        controlBody.transform.localEulerAngles = Vector3.Lerp(controlBody.transform.localEulerAngles, Vector3.zero, 1.0f);
        cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, 1.0f);
    }

    IEnumerator objectEnable(float tarTime)
    {
        float sumtime = 0.0f;
        while (sumtime < tarTime)
        {
            yield return null;
        }

        controlBody.SetActive(true);
    }

    IEnumerator customBody()
    {
        Vector3 keepRot = new Vector3(0.0f, -15.0f, 0.0f);
        Vector3 keepPos = cam.transform.position;

        Vector3 targetRot = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 targetPos = new Vector3(0.0f, -0.35f, 1.3f);

        float speed = 2.0f;
        Vector3 normal = targetRot - keepRot;

        float dis = Vector3.Distance(targetRot, keepRot);
        normal.Normalize();

        Debug.Log(dis);

        float sumtime = 0.0f;
        while (sumtime < 1.0f / speed)
        {
            yield return null;
            controlBody.transform.localEulerAngles = keepRot + normal * dis * Mathf.Pow(sumtime * speed, 2.0f);//Vector3.Lerp(controlBody.transform.localEulerAngles, targetRot, Mathf.Pow(sumtime * speed, 2.0f));
            cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, Mathf.Pow(sumtime * speed, 2.0f));
            sumtime += Time.deltaTime;
            Debug.Log(sumtime);
        }
        controlBody.transform.localEulerAngles = Vector3.Lerp(controlBody.transform.localEulerAngles, Vector3.zero, 1.0f);
        cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, 1.0f);
    }

    IEnumerator heightBlend()
    {
        float speed = 0.5f;
        float sumtime = 0.0f;
        while (sumtime < 1.0f / speed)
        {
            yield return null;

            oriModel.SetBlendShapeWeight(0, 25.0f * sumtime * speed);
            sumtime += Time.deltaTime;
        }

        while (sumtime * speed > 0.0f)
        {
            yield return null;

            oriModel.SetBlendShapeWeight(0, 25.0f * sumtime * speed);
            sumtime -= Time.deltaTime;
        }
        oriModel.SetBlendShapeWeight(0, 0.0f);
    }

    IEnumerator smileBlendStart()
    {
        float sumtime = 0.0f;
        while (sumtime < 1.0f)
        {
            sumtime += Time.deltaTime;
            smileBlend.SetBlendShapeWeight(smileIndex, 100.0f * sumtime);
            yield return null;
        }
    }

    IEnumerator clothChangeAni(float tarTime, int index)
    {
        float sumtime = 0.0f;
        while (sumtime < tarTime)
        {
            smileBlend.SetBlendShapeWeight(smileIndex, 100.0f * sumtime);
            yield return null;
        }

        if(index == 1)
        {
            StartCoroutine(smileBlendStart());
        }

        setIndexs(index);
        controlBody.GetComponent<Animator>().runtimeAnimatorController = anis[index - 1];
    }

    IEnumerator weightBlend()
    {
        float speed = 0.7f;
        float sumtime = 0.0f;
        while (sumtime * speed < 1.0f)
        {
            yield return null;

            oriModel.SetBlendShapeWeight(2, 100.0f * sumtime * speed);
            sumtime += Time.deltaTime;
        }

        while (sumtime * speed > 0.0f)
        {
            yield return null;

            oriModel.SetBlendShapeWeight(2, 100.0f * sumtime * speed);
            sumtime -= Time.deltaTime;
        }
        oriModel.SetBlendShapeWeight(2, 0.0f);
    }

    IEnumerator returnBody()
    {
        Vector3 keepRot = controlBody.transform.localEulerAngles;
        Vector3 keepPos = cam.transform.position;

        Vector3 targetRot = new Vector3(0.0f, -15.0f, 0.0f);
        Vector3 targetPos = new Vector3(0.0f, 0.0f, 0.0f);

        float speed = 2.0f;

        Vector3 normal = targetRot - keepRot;

        float dis = Vector3.Distance(targetRot, keepRot);

        normal.Normalize();

        float sumtime = 0.0f;
        while (sumtime < 1.0f / speed)
        {
            yield return null;
            controlBody.transform.localEulerAngles = keepRot + normal * dis * Mathf.Pow(sumtime * speed, 2.0f);//Vector3.Lerp(controlBody.transform.localEulerAngles, targetRot, Mathf.Pow(sumtime * speed, 2.0f));
            cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, Mathf.Pow(sumtime * speed, 2.0f));
            sumtime += Time.deltaTime;
            Debug.Log(sumtime);
        }
        controlBody.transform.localEulerAngles = Vector3.Lerp(controlBody.transform.localEulerAngles, targetRot, 1.0f);
        cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, 1.0f);
    }

    IEnumerator corQueue()
    {
        while(actionQueue.Count > 0)
        {
            yield return StartCoroutine(actionQueue.Dequeue());
        }
    }

    IEnumerator delayAction(float tarTime, IEnumerator func)
    {
        float saveTime = 0.0f;

        while (saveTime < tarTime)
        {
            yield return null;
            saveTime += Time.deltaTime;
            //Debug.Log(saveTime + "/" + tarTime);
        }

        if(func != null)
        {
            StartCoroutine(func);
        }
    }

    void init()
    {
        chars = new List<GameObject>();
        chars.Add(oriModel.transform.gameObject);

        clothIndexs = new List<int[]>();
        clothIndexs.Add(cloth0);
        clothIndexs.Add(cloth1);
        clothIndexs.Add(cloth2);

        Transform[] bones = oriModel.bones;
        Transform rootBone = oriModel.rootBone;
        List<Matrix4x4> keepBindPoses = new List<Matrix4x4>();

        keepBindPoses.AddRange(oriModel.sharedMesh.bindposes);

        Dictionary<string, int> jointToIndex = new Dictionary<string, int>();

        for (int i = 0; i < bones.Length; i++)
        {
            jointToIndex.Add(bones[i].name, i);
        }

        for (int i = 0; i < cloth.Length; i++)
        {
            Mesh curMesh = cloth[i].sharedMesh;
            Transform[] curBones = cloth[i].bones;

            Dictionary<int, string> indexToJoint = new Dictionary<int, string>();

            for (int j = 0; j < curBones.Length; j++)
            {
                indexToJoint.Add(j, curBones[j].name);
            }

            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uv = new List<Vector2>();
            List<BoneWeight> boneWeights = new List<BoneWeight>();
            List<Matrix4x4> bindposes = new List<Matrix4x4>();

            vertices.AddRange(curMesh.vertices);
            normals.AddRange(curMesh.normals);
            triangles.AddRange(curMesh.triangles);
            uv.AddRange(curMesh.uv);
            boneWeights.AddRange(curMesh.boneWeights);
            bindposes.AddRange(keepBindPoses.ToArray());

            for (int j = 0; j < boneWeights.Count; j++)
            {
                BoneWeight bw = boneWeights[j];
                bw.boneIndex0 = jointToIndex[indexToJoint[bw.boneIndex0]];
                bw.boneIndex1 = jointToIndex[indexToJoint[bw.boneIndex1]];
                bw.boneIndex2 = jointToIndex[indexToJoint[bw.boneIndex2]];
                bw.boneIndex3 = jointToIndex[indexToJoint[bw.boneIndex3]];

                boneWeights[j] = bw;
            }

            GameObject newGo = new GameObject();
            SkinnedMeshRenderer smr = newGo.AddComponent<SkinnedMeshRenderer>();
            smr.receiveShadows = false;

            Mesh newMesh = new Mesh();
            newMesh.vertices = vertices.ToArray();
            newMesh.normals = normals.ToArray();
            newMesh.triangles = triangles.ToArray();
            newMesh.uv = uv.ToArray();
            newMesh.bindposes = bindposes.ToArray();
            newMesh.boneWeights = boneWeights.ToArray();

            for (int j = 0; j < curMesh.blendShapeCount; j++)
            {
                if(smileBlend == null)
                {
                    smileBlend = smr;
                }

                int vertCount = curMesh.vertexCount;

                Vector3[] deltaVertices = new Vector3[vertCount];
                Vector3[] deltaNormals = new Vector3[vertCount];
                Vector3[] deltaTangent = new Vector3[vertCount];

                curMesh.GetBlendShapeFrameVertices(j, 0, deltaVertices, deltaNormals, deltaTangent);

                if (curMesh.GetBlendShapeName(j).Contains("Smile_M"))
                {
                    smileIndex = j;
                }

                newMesh.AddBlendShapeFrame(curMesh.GetBlendShapeName(j), 100.0f, deltaVertices, deltaNormals, deltaTangent);
            }
            

            smr.sharedMesh = newMesh;
            smr.rootBone = rootBone;
            smr.bones = bones;

            newGo.transform.parent = oriModel.transform.parent;
            smr.material = cloth[i].material;

            chars.Add(newGo);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            setIndexs(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            setIndexs(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            setIndexs(2);
        }
    }

    void setIndexs(int curIndex)
    {
        for(int i = 0; i < chars.Count; i++)
        {
            chars[i].SetActive(false);
        }
        int[] indices = clothIndexs[curIndex];

        for (int i = 0; i < indices.Length; i++)
        {
            chars[indices[i]].SetActive(true);
        }
    }
}
