using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class GroupJointDatas : MonoBehaviour
{
    Dictionary<string, List<string>> closedRigData;
    Dictionary<string, List<string>> groupJoints;
    List<string> remainJoint;

    string[] standardJoint = { "Head", "L_Wrist", "R_Wrist", "L_Foot", "R_Foot" };

    // Start is called before the first frame update
    void Start()
    {
        remainJoint = new List<string>();
        groupJoints = new Dictionary<string, List<string>>();

        setClosedRigData();

        string jointDatas = File.ReadAllText("C:\\RigData\\jointdatas.txt");

        string[] splitJoints = jointDatas.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        remainJoint.AddRange(splitJoints);

        StartCoroutine(testCor());
    }

    IEnumerator testCor()
    {
        int index = 0;

        while(remainJoint.Count > 0 && index < standardJoint.Length)
        {
            string curName = standardJoint[index];

            List<string> rootJointData = new List<string>();

            groupJoints.Add(curName, rootJointData);

            findJoints(curName, curName);

            yield return null;

            index++;
        }

        Debug.Log(remainJoint.Count + " " + groupJoints.Count);

        foreach(KeyValuePair<string, List<string>> current in groupJoints)
        {
            List<string> grouping = current.Value;

            string groupData = current.Key + " {";

            for(int i = 0; i < grouping.Count; i++)
            {
                groupData += grouping[i];
                if (i + 1 < grouping.Count)
                {
                    groupData += ", ";
                }
            }
            groupData += "}";

            Debug.Log(groupData);
        }
    }

    void findJoints(string rootJoint, string tarName)
    {
        List<string> crdJoint = closedRigData[tarName];

        for(int i = 0; i < crdJoint.Count; i++)
        {
            for(int j = 0; j < remainJoint.Count; j++)
            {
                if(crdJoint[i] == remainJoint[j])
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

    // Update is called once per frame
    void Update()
    {

    }

    void setClosedRigData()
    {
        closedRigData = new Dictionary<string, List<string>>();

        string data = File.ReadAllText("C:\\RigData\\newClosedRigData.txt");

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
}
