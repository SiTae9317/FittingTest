using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class fittingtest : MonoBehaviour
{
    public bool isMan = true;
    public GameObject cloth;
    public GameObject shoes;
    public GameObject avatar;
    //public RuntimeAnimatorController ac;
    public Action resultAction = null;

    private FittingManager fManager;

    private Dictionary<string, Vector3> jointToEular;

    private void Start()
    {
        //foreach(SkinnedMeshRenderer smr in avatar.GetComponentsInChildren<SkinnedMeshRenderer>())
        //{
        //    if(smr.gameObject.name == "mesh")
        //    {
        //        bindposes = smr.sharedMesh.bindposes;
        //    }
        //}
    }

    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            startFit(resultAction);
        }

        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            tPose();
        }

        //if(Input.GetKeyDown(KeyCode.Alpha3))
        //{
        //    exportTPoseData();
        //}
	}

    public void startFit(Action rac)
    {
        fManager = gameObject.AddComponent<FittingManager>();
        fManager.clothObj = cloth;
        fManager.charObj = avatar;
        fManager.shoesObj = shoes;
        fManager.isMan = isMan;
        resultAction = rac;

        fManager.fittingStart(debugFinish);
    }

    void tPose()
    {
        TextAsset ta = (TextAsset)Resources.Load("TPoseData");
        string tPose = ta.text;

        string[] tPoseDatas = tPose.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        Dictionary<string, Vector3> jointToEular = new Dictionary<string, Vector3>();

        for (int i = 0; i < tPoseDatas.Length; i++)
        {
            string[] lineData = tPoseDatas[i].Split(' ');
            jointToEular.Add(lineData[0], new Vector3(float.Parse(lineData[1]), float.Parse(lineData[2]), float.Parse(lineData[3])));
        }

        Transform[] bones = avatar.GetComponentInChildren<SkinnedMeshRenderer>().bones;

        for(int i = 0; i < bones.Length; i++)
        {
            if(jointToEular.ContainsKey(bones[i].name))
            {
                bones[i].transform.localEulerAngles = jointToEular[bones[i].name];
            }
        }
    }

    //void exportTPoseData()
    //{
    //    int childCount = srcAvatar.transform.childCount;

    //    Transform pelvis = null;

    //    for(int i = 0; i < childCount; i++)
    //    {
    //        if(srcAvatar.transform.GetChild(i).name == "skeleton")
    //        {
    //            pelvis = srcAvatar.transform.GetChild(i).GetChild(0);
    //        }
    //    }

    //    jointToEular = new Dictionary<string, Vector3>();

    //    detectChildTrs(pelvis);

    //    string exportData = "";

    //    foreach(KeyValuePair<string, Vector3> curData in jointToEular)
    //    {
    //        exportData += curData.Key + " " + curData.Value.x + " " + curData.Value.y + " " + curData.Value.z + "\r\n";
    //    }

    //    File.WriteAllText("C:\\RigData\\newTPoseData.txt", exportData);
    //}

    void detectChildTrs(Transform trs)
    {
        int childCount = trs.childCount;

        for(int i = 0; i < childCount; i++)
        {
            Transform curTrs = trs.GetChild(i);

            jointToEular.Add(curTrs.name, curTrs.localEulerAngles);

            detectChildTrs(curTrs);
        }
    }

    public void debugFinish()
    {
        Debug.Log("Fitting Finish");

        if(resultAction != null)
        {
            resultAction();
        }

        Destroy(fManager);
        Destroy(this);
    }
}
