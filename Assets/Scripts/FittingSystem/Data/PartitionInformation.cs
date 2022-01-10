using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartitionInformation
{
	public Dictionary<string, Vector3> jointPosition = new Dictionary<string, Vector3>();
	public List<CustomSkinnedMesh> csms = new List<CustomSkinnedMesh>();
	public Dictionary<int, List<int>> vertexToTriangle = new Dictionary<int, List<int>>();
	public Dictionary<string, List<List<int>>> jointVertexIndex = new Dictionary<string, List<List<int>>> ();
	public Dictionary<int, string> indexToName = new Dictionary<int, string>();
	public Dictionary<string, int> nameToIndex = new Dictionary<string, int>();
	public List<Matrix4x4> oriBoneMatrices = new List<Matrix4x4> ();
	public Dictionary<Vector3, HashSet<Vector3>> vertexToCloseVertexs = new Dictionary<Vector3, HashSet<Vector3>>();
	public Material mat;
}