using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SimpleBuoyancy {

    [CustomEditor(typeof(Buoyancy))]
    public class BuoyancyEditor : Editor {

        private MeshFilter meshFilter;
        private FieldInfo numberOfRandomPointsField;
        private FieldInfo hideGizmosField;
        
        // FIXME value is not saved
        private int numberOfRandomPoints;
        // FIXME value is not saved
        private bool hideGizmos;

        private void OnEnable() {
            numberOfRandomPointsField =
                typeof(Buoyancy).GetField("numberOfRandomPoints", BindingFlags.NonPublic | BindingFlags.Instance);
            if (numberOfRandomPointsField != null)
                numberOfRandomPoints = (int)numberOfRandomPointsField.GetValue(target);

            hideGizmosField = typeof(Buoyancy).GetField("hideGizmos", BindingFlags.NonPublic | BindingFlags.Instance);
            if (hideGizmosField != null)
                hideGizmos = (bool)hideGizmosField.GetValue(target);

            FieldInfo meshFilterField =
                typeof(Buoyancy).GetField("meshFilter", BindingFlags.NonPublic | BindingFlags.Instance);
            if (meshFilterField != null)
                meshFilter = (MeshFilter)meshFilterField.GetValue(target);
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            Buoyancy buoyancy = (Buoyancy)target;

            EditorGUILayout.Space();

            if (GUILayout.Button("Calculate Volume"))
                buoyancy.Volume = MeshMetrics.CalculateVolume(meshFilter.sharedMesh);

            if (GUILayout.Button("Calculate Surface Area"))
                buoyancy.SurfaceArea = MeshMetrics.CalculateSurfaceArea(meshFilter.sharedMesh);

            EditorGUILayout.Space();

            numberOfRandomPoints = EditorGUILayout.IntField("Number of Random Points", numberOfRandomPoints);

            if (numberOfRandomPointsField != null)
                numberOfRandomPointsField.SetValue(buoyancy, numberOfRandomPoints);

            if (GUILayout.Button("Select Random Volume Points"))
                buoyancy.VolumePoints =
                    MeshMetrics.SampleRandomPointsInsideMesh(meshFilter.sharedMesh, numberOfRandomPoints).ToList();

            if (GUILayout.Button("Select Random Surface Points"))
                buoyancy.SurfacePoints = MeshMetrics
                    .SampleRandomPointsOnMesh(meshFilter.sharedMesh, numberOfRandomPoints)
                    .ToList();

            hideGizmos = EditorGUILayout.Toggle("Hide Gizmos", hideGizmos);

            if (hideGizmosField != null)
                hideGizmosField.SetValue(buoyancy, hideGizmos);

            if (GUI.changed)
                EditorUtility.SetDirty(buoyancy);
        }
    }
}