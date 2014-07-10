using UnityEngine;
using System.Collections;
using System;

public class CubemapInfluence : MonoBehaviour {

	private LuxEnvProbe[] probes;

	private CubemapManager manager;

	// Use this for initialization
	void Start () {
		manager = UnityEngine.Object.FindObjectOfType<CubemapManager>();
	}

	float coverage(LuxEnvProbe probe)
	{
		float dx = Mathf.Abs(probe.transform.position.x - transform.position.x);
		float dy = Mathf.Abs(probe.transform.position.y - transform.position.y);
		float dz = Mathf.Abs(probe.transform.position.z - transform.position.z);

		float start_dist = 0.0f;

		float end_dist_x = probe.BoxSize.x/2.0f;
		float end_dist_y = probe.BoxSize.y/2.0f;
		float end_dist_z = probe.BoxSize.z/2.0f;

		float influencex = 1.0f-Mathf.Max(0, Mathf.Min(1, (dx - start_dist) / (end_dist_x - start_dist)));
		float influencey = 1.0f-Mathf.Max(0, Mathf.Min(1, (dy - start_dist) / (end_dist_y - start_dist)));
		float influencez = 1.0f-Mathf.Max(0, Mathf.Min(1, (dz - start_dist) / (end_dist_z - start_dist)));

		return Mathf.Min(influencex,Mathf.Min(influencey,influencez));
	}

	float CalculateInfluences(out int probe1, out int probe2)
	{
		probe1 = probe2 = 0;

		int influence_count = probes.Length;

		float[] influences = new float[influence_count];
		//float[] blendfactors = new float[influence_count];

		int [] pos = new int[influence_count];

	    float sumInfluences = 0.0f;
	    float sumInvInfluences = 0.0f;
	    float sumFactors = 0.0f;

		for (int k = 0; k < probes.Length; k++)
	    {
			influences[k] = coverage(probes[k]);
			pos[k] = k;
		}

		Array.Sort<int>(pos,(i,j) =>{
			return influences[j].CompareTo(influences[i]);
		});

		if(influence_count >= 2)
		{
			influence_count = 2;
		}
		else
		{
			probe1 = probe2 = pos[0];
			return 1.0f;
		}

		for (int k = 0; k < influence_count; k++)
		{
			float influence = influences[k];

			sumInfluences += influence;
	        sumInvInfluences += 1.0f - influence;
	    }

		for (int k = 0; k < influence_count; k++)
	    {
			float influence = influences[pos[k]];

			influences[pos[k]] =
				influence/sumInfluences;
//				((1.0f - influence)/sumInvInfluences) * 
//				 (1.0f - influence/sumInfluences) / 
//					(influence_count - 1);

			sumFactors += influences[pos[k]];
	    }

		for (int k = 0; k < influence_count; k++)
	    {
			influences[pos[k]] = influences[pos[k]]/sumFactors;
	    }

		probe1 = pos[0];

		if (influence_count>=1)
			probe2 = pos[1];

		return influences[pos[0]];
	}

	// Update is called once per frame
	void Update () {
		probes = manager.UpdateProbes(probes, transform);

		if(renderer != null)
		{
			int probe1, probe2;

			float influence = CalculateInfluences(out probe1, out probe2);

			int materials = renderer.materials.Length;
			// Get all materials
			for (int j = 0; j < materials; j++) {
				if (renderer.materials[j].HasProperty("_CubemapSize"))
				{
					renderer.materials[j].SetVector("_CubemapSize", 
						new Vector4(probes[probe1].BoxSize.x*0.5f, probes[probe1].BoxSize.y*0.5f, probes[probe1].BoxSize.z*0.5f, 0));
					renderer.materials[j].SetMatrix("_CubeMatrix_Trans", probes[probe1].BoxMatrix.transpose );
					renderer.materials[j].SetMatrix("_CubeMatrix_Inv", probes[probe1].BoxMatrix.inverse );
					renderer.materials[j].SetTexture("_DiffCubeIBL", probes[probe1].DIFFCube );
					if (probes[probe1].SPECCube != null){
						renderer.materials[j].SetTexture("_SpecCubeIBL", probes[probe1].SPECCube );
					}

					Debug.Log(string.Format("{0} {1} {2}",probes[probe1].name,probes[probe2].name,influence));

					renderer.materials[j].SetFloat("_Influence", influence);

					renderer.materials[j].SetVector("_CubemapSize2", 
						new Vector4(probes[probe2].BoxSize.x*0.5f, probes[probe2].BoxSize.y*0.5f, probes[probe2].BoxSize.z*0.5f, 0));
					renderer.materials[j].SetMatrix("_CubeMatrix_Trans2", probes[probe2].BoxMatrix.transpose );
					renderer.materials[j].SetMatrix("_CubeMatrix_Inv2", probes[probe2].BoxMatrix.inverse );
					renderer.materials[j].SetTexture("_DiffCubeIBL2", probes[probe2].DIFFCube );
					if (probes[probe2].SPECCube != null){
						renderer.materials[j].SetTexture("_SpecCubeIBL2", probes[probe2].SPECCube );
					}
				}
			}
		}
		else {
			Debug.Log("does not have a mesh renderer attached to it. It has been removed.");
		}
	}

}
