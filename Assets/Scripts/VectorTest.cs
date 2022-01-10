using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorTest : MonoBehaviour
{
    public GameObject p0;
    public GameObject p1;
    public GameObject p2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(p1.transform.position, p2.transform.position, Color.green);
        Debug.DrawRay(p0.transform.position, normalCalcFromJoint(p0.transform.position, p1.transform.position, p2.transform.position) * 100.0f, Color.yellow);
    }

    public Vector3 normalCalcFromJoint(Vector3 point, Vector3 joint1, Vector3 joint2)
    {
        Vector3 vertNor = point - joint1;
        vertNor.Normalize();

        float aDis = Vector3.Distance(point, joint1);

        Vector3 jointVector = joint1 - joint2;
        jointVector.Normalize();

        float vectorAngle = 180.0f - Vector3.Angle(-vertNor, -jointVector);

        Vector3 apointVec = Vector3.zero;
        Vector3 newPointVec = Vector3.zero;

        if (vectorAngle == 90.0f)
        {
            float bDis = aDis / Mathf.Sin(90.0f / 180.0f * Mathf.PI) * Mathf.Sin((90.0f - vectorAngle) / 180.0f * Mathf.PI);

            apointVec = joint1 - point;
            newPointVec = (joint1 + bDis * -jointVector.normalized) - point;

            newPointVec += apointVec;
        }
        else
        {
            float bDis = aDis / Mathf.Sin(90.0f / 180.0f * Mathf.PI) * Mathf.Sin((90.0f - vectorAngle) / 180.0f * Mathf.PI);

            apointVec = joint1 - point;
            newPointVec = (joint1 + bDis * -jointVector) - point;
        }

        newPointVec.Normalize();

        //		newPointVec *= 2.0f;

        return newPointVec;
    }
}
