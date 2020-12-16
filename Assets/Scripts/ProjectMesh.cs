using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectMesh : MonoBehaviour
{
	[SerializeField] Mesh targetMesh;
	[SerializeField] Transform lightDirection;
	[SerializeField] Transform plainCorner;

	[Space]
	[SerializeField] bool useClip = false;
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
				var projection = Calc_Projection(targetMesh, lightDirection.position, 
					projectData[i].plains[j].position, projectData[i].plains[j].forward, 
					plainCorner.position);

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
				for (int l = transform.childCount-1; l >= projection.Count; --l)
				{
					if (Application.isPlaying)
						Destroy(transform.GetChild(l).gameObject);
					else
						DestroyImmediate(transform.GetChild(l).gameObject);
				}
			}
		}
	}

	List<List<Vector2>> Calc_Projection(Mesh mesh, Vector3 direction, Vector3 plain, Vector3 plainNormal, Vector3 clip)
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
			if (useClip)
			{
				bool aOut = IsOutSide(a);
				bool bOut = IsOutSide(b);
				bool cOut = IsOutSide(c);
				if (aOut && bOut && cOut)
				{
					continue;
				}
				else
				if (aOut && bOut ||
					bOut && cOut ||
					aOut && cOut)
				{
					var out1 = aOut ? a : bOut ? b : c;
					var out2 = cOut ? c : bOut ? b : a;
					var in1 = !aOut ? a : !bOut ? b : c;

					var dClip = (clip - in1);

					var dVert = (out1 - in1);
					var tVert = dClip.x / dVert.x;
					out1 = in1 + dVert * tVert;

					dVert = (out2 - in1);
					tVert = dClip.x / dVert.x;
					out2 = in1 + dVert * tVert;

					points.Add(out1);
					points.Add(out2);
					points.Add(in1);
					ret.Add(points);
				}
				else
				if (aOut || bOut || cOut)
				{
					var out1 = aOut ? a : bOut ? b : c;
					var in1 = !aOut ? a : !bOut ? b : c;
					var in2 = !cOut ? c : !bOut ? b : a;


					var dClip = (clip - in1);
					var dVert = (out1 - in1);
					var tVert = dClip.x / dVert.x;
					var nVert1 = in1 + dVert * tVert;

					points.Add(nVert1);
					points.Add(in1);
					points.Add(in2);
					ret.Add(points);

					dClip = (clip - in2);
					dVert = (out1 - in2);
					tVert = dClip.x / dVert.x;
					var nVert2 = in2 + dVert * tVert;

					points.Clear();
					points.Add(nVert1);
					points.Add(nVert2);
					points.Add(in2);
					ret.Add(points);
				}
				else
				{
					points.Add(a);
					points.Add(b);
					points.Add(c);
					ret.Add(points);
				}
			}else
			{
				points.Add(a);
				points.Add(b);
				points.Add(c);
				ret.Add(points);
			}
		}
		return ret;
	}

	Vector3 Calc_Projection(Vector3 v, Vector3 d, Vector3 p, Vector3 n)
	{
		return (v + Vector3.Dot((p - v), n) / Vector3.Dot(d, n) * d);
	}

	bool IsOutSide(Vector2 v)
	{
		return (v.x < plainCorner.position.x);
	}

	void Triangle_Clip(ref Vector3 a, ref Vector3 b, ref Vector3 c, ref Vector3 clip)
	{

	}
}
