using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Buoyancy))]
public class BuoyancyEditor : Editor {
    
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        
        EditorGUILayoutExtensions.HorizontalLine(Color.gray);
        
        Buoyancy buoyancy = (Buoyancy)target;
        
        if (GUILayout.Button("Calculate Volume")) {
            MeshFilter meshFilter = buoyancy.GetComponent<MeshFilter>();
            buoyancy.Volume = MeshMetrics.CalculateVolume(meshFilter.sharedMesh);
            EditorUtility.SetDirty(buoyancy);
        }
        
        if (GUILayout.Button("Calculate Surface Area")) {
            MeshFilter meshFilter = buoyancy.GetComponent<MeshFilter>();
            buoyancy.SurfaceArea = MeshMetrics.CalculateSurfaceArea(meshFilter.sharedMesh);
            EditorUtility.SetDirty(buoyancy);
        }
        
        EditorGUILayoutExtensions.HorizontalLine(Color.gray);

        if (GUILayout.Button("Select Random Volume Points")) {
            MeshFilter meshFilter = buoyancy.GetComponent<MeshFilter>();
            if (!meshFilter)
                return;
            buoyancy.VolumePoints = MeshMetrics.SampleRandomPointsInsideMesh(meshFilter, buoyancy.NumberOfRandomPoints).ToList();
            EditorUtility.SetDirty(buoyancy);
        }
        
        if (GUILayout.Button("Select Random Surface Points")) {
            MeshFilter meshFilter = buoyancy.GetComponent<MeshFilter>();
            if (!meshFilter)
                return;
            buoyancy.SurfacePoints = MeshMetrics.SampleRandomPointsOnMesh(meshFilter.sharedMesh, buoyancy.NumberOfRandomPoints).ToList();
            EditorUtility.SetDirty(buoyancy);
        }
    }
}