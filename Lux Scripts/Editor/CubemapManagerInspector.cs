using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CubemapManager))]
public class CubemapManagerInspector : Editor
{
	public override void OnInspectorGUI ()
	{
		// render the default inspector
		base.OnInspectorGUI ();

		if (GUILayout.Button("Update CubemapManager"))
		{
			((CubemapManager)target).Calculate();
		}
	}
}
