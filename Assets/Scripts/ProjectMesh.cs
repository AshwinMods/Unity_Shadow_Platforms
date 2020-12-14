using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectMesh : MonoBehaviour
{
	[SerializeField] Mesh targetMesh;
	[SerializeField] Transform lightDirection;
	[SerializeField] Transform plainCorner;

	[Space]
	[SerializeField] bool project = false;
	[System.Serializable]
	struct ProjectData
	{
#if UNITY_EDITOR
		public string name;
#endif
		public Transform[] plains;
		[Header("runtime")]
		public List<List<Vector2>> paths;
	}
	[SerializeField] ProjectData[] projectData;
	private void OnDrawGizmos()
	{
		if (project  && lightDirection && plainCorner)
		{
			project = false;
			Calc_Projections();
		}
	}

	void Calc_Projections()
	{
		if (!targetMesh || !lightDirection || !plainCorner)
			return;

		for (int i = 0; i < projectData.Length; i++)
		{
			for (int j = 0; j < projectData[i].plains.Length; j++)
			{
				var projection = Calc_Projection(targetMesh, lightDirection.position, projectData[i].plains[j].position, projectData[i].plains[j].forward);
				for (int k = 0; k < projection.Count; k++)
				{
					Transform t;
					GameObject o;
					PolygonCollider2D c;
					if (transform.childCount > k)
					{
						t = transform.GetChild(k);
						o = t.gameObject;
						c = o.GetComponent<PolygonCollider2D>();
					}
					else
					{
						o = new GameObject(k.ToString());
						t = o.transform;
						o.transform.SetParent(transform);
						c = o.AddComponent<PolygonCollider2D>();
						c.pathCount = 1;
					}
					t.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
					c.SetPath(0, projection[k]);
				}
				for (int l = projection.Count; l < transform.childCount; l++)
				{
					if (Application.isPlaying)
						Destroy(transform.GetChild(l).gameObject);
					else
						DestroyImmediate(transform.GetChild(l).gameObject);
				}
			}
		}
	}

	List<List<Vector2>> Calc_Projection(Mesh mesh, Vector3 direction, Vector3 plain, Vector3 plainNormal)
	{
		List<List<Vector2>> ret = new List<List<Vector2>>(mesh.triangles.Length / 3);
		for (int j = 0; j < mesh.triangles.Length; j+=3)
		{
			var v = transform.TransformPoint(mesh.vertices[mesh.triangles[j]]);
			var a = Calc_Projection(v, (v - direction), plain, plainNormal);

			v = transform.TransformPoint(mesh.vertices[mesh.triangles[j + 1]]);
			var b = Calc_Projection(v, (v - direction), plain, plainNormal);

			v = transform.TransformPoint(mesh.vertices[mesh.triangles[j + 2]]);
			var c = Calc_Projection(v, (v - direction), plain, plainNormal);

			List<Vector2> points = new List<Vector2>(3);
			points.Add(a);
			points.Add(b);
			points.Add(c);
			ret.Add(points);
		}
		return ret;
	}

	Vector3 Calc_Projection(Vector3 v, Vector3 d, Vector3 p, Vector3 n)
	{
		return (v + Vector3.Dot((p - v), n) / Vector3.Dot(d, n) * d);
	}
}
