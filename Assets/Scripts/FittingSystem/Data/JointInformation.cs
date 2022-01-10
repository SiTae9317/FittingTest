using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class JointInformation
{
	public bool isRotationJoint = false;
	public bool isPositionJoint = false;
	public Vector3 yzVector = Vector3.zero;
	public Vector3 yxVector = Vector3.zero;
	public int childCount;
	public int index;
	public int pivot0;
	public int pivot1;
	public int pivot2;
	public int pivot3;
}