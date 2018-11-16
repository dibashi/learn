using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Map))]
public class MapModelCreator : Editor {
	public override void OnInspectorGUI() {
	    DrawDefaultInspector ();
	    if (GUILayout.Button("Create Map Model")) {
			Map map = target as Map;
	    	map.CreateModel();
		}
	}
}
