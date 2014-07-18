using UnityEngine;
using System.Collections.Generic;
using System;

public class CubemapManager : MonoBehaviour
{
	[SerializeField]
	[HideInInspector]
	private List<LuxEnvProbe> dicKeys;

	[SerializeField]
	[HideInInspector]
	private List<LuxEnvProbe> dicValues;

	[SerializeField]
	[HideInInspector]
	private List<int> dicLastIndex;

	private Dictionary<LuxEnvProbe,LuxEnvProbe[]> linkProbes;

	bool Intersect(LuxEnvProbe probe1, LuxEnvProbe probe2)
	{
		Vector3 diff = probe1.transform.position-probe2.transform.position;
		Vector3 dist = (probe1.BoxSize+probe2.BoxSize)/2.0f;

		return (dist.x >= Mathf.Abs(diff.x) &&
		    	dist.y >= Mathf.Abs(diff.y) &&
		        dist.z >= Mathf.Abs(diff.z));
	}

	public void Calculate()
	{
		// Get All probes in scene
		LuxEnvProbe[] probes = UnityEngine.Object.FindObjectsOfType<LuxEnvProbe>();
		
		linkProbes = new Dictionary<LuxEnvProbe, LuxEnvProbe[]>();

		List<LuxEnvProbe>[] neighbors = new List<LuxEnvProbe>[probes.Length];
		
		for( int i = 0; i < probes.Length; i++ )
		{
			neighbors[i] = new List<LuxEnvProbe>();

			for( int j = 0; j < i; j++ )
			{
				if (Intersect(probes[i],probes[j]))
				{
					neighbors[i].Add(probes[j]);
					neighbors[j].Add(probes[i]);
				}
			}
		}

		for( int i = 0; i < probes.Length; i++ )
		{
			linkProbes.Add(probes[i], neighbors[i].ToArray());
		}

		dicKeys = new List<LuxEnvProbe>();
		foreach(var key in linkProbes.Keys)
			dicKeys.Add(key);

		int pos = 0;

		dicLastIndex = new List<int>();
		dicValues = new List<LuxEnvProbe>();
		foreach(var value in linkProbes.Values)
		{
			dicValues.AddRange(value);
			pos += value.Length;
			dicLastIndex.Add(pos);
		}
	}

	void Awake()
	{
		linkProbes = new Dictionary<LuxEnvProbe, LuxEnvProbe[]>();

		int pos = 0;

		for( int i = 0; i < dicKeys.Count; i++ )
		{
			List<LuxEnvProbe> values = new List<LuxEnvProbe>();

			while(pos != dicLastIndex[i])
			{
				values.Add(dicValues[pos]);
				pos++;
			}

			linkProbes.Add(dicKeys[i], values.ToArray());
		}
	}

	void Start()
	{
		//Calculate();
	}

	void Update()
	{
	}

	private LuxEnvProbe[] FilterProbes(LuxEnvProbe[] linkProbes, Transform elemTransform, Bounds elemBound)
	{
		List<LuxEnvProbe> returnProbes = new List<LuxEnvProbe>();

		foreach(var linkProbe in linkProbes)
		{
			Vector3 diff = linkProbe.transform.position-elemTransform.position;
			Vector3 dist = linkProbe.BoxSize/2.0f+elemBound.extents;
			
			if (dist.x >= Mathf.Abs(diff.x) &&
			    dist.y >= Mathf.Abs(diff.y) &&
			    dist.z >= Mathf.Abs(diff.z))
			{
				returnProbes.Add(linkProbe);
			}
		}

		return returnProbes.ToArray();
	}

	public LuxEnvProbe[] UpdateProbes(LuxEnvProbe[] currProbes, Transform elemTransform, Bounds elemBound)
	{
		List<LuxEnvProbe> returnProbes = new List<LuxEnvProbe>();

		if (currProbes != null && currProbes.Length > 0)
		{
			// if already have a list
			// build with probes already selected
			// and theirs neighbours
			// to be filtered
			returnProbes.AddRange(currProbes);

			foreach(var p in currProbes)
				foreach(var p2 in linkProbes[p])
					if (!returnProbes.Contains(p2))
						returnProbes.Add(p2);
		}
		else
		{
			// build probe list, add all probes to filter
			foreach(var probe in linkProbes.Keys)
				returnProbes.Add(probe);
		}

		return FilterProbes(returnProbes.ToArray(),elemTransform, elemBound);
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.black;

		if(linkProbes!=null)
		{
			foreach(var probe in linkProbes.Keys)
			{
				foreach(var neighbor in linkProbes[probe])
					Gizmos.DrawLine(
						probe.transform.position,
						neighbor.transform.position);
			}
		}
	}
}