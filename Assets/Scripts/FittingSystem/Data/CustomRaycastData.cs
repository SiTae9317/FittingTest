using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomRaycastData
{
	public string name = "";
	public bool isPant = false;
	public List<Vector3> vertices = new List<Vector3>();
	public List<Vector3> normals = new List<Vector3>();
	public List<Vector3> calcNormals = new List<Vector3>();
	public List<BoneWeight> boneWeights = new List<BoneWeight>();
	public List<CustomSkinnedMesh> csms = new List<CustomSkinnedMesh>();
	public Dictionary<Vector3, List<Vector3>> vertexToCloseVertices = new Dictionary<Vector3, List<Vector3>>();
}