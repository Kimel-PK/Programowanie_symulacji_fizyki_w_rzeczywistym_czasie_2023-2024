using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleBuoyancy {

    public static class MeshMetrics {

        public static float CalculateVolume(Mesh mesh) {
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            float volume = 0f;

            for (int i = 0; i < triangles.Length; i += 3) {
                Vector3 v0 = vertices[triangles[i]];
                Vector3 v1 = vertices[triangles[i + 1]];
                Vector3 v2 = vertices[triangles[i + 2]];

                volume += SignedVolumeOfTriangle(v0, v1, v2);
            }

            return Mathf.Abs(volume);
        }

        public static float CalculateSurfaceArea(Mesh mesh) {
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            float area = 0f;

            for (int i = 0; i < triangles.Length; i += 3) {
                Vector3 v0 = vertices[triangles[i]];
                Vector3 v1 = vertices[triangles[i + 1]];
                Vector3 v2 = vertices[triangles[i + 2]];

                area += AreaOfTriangle(v0, v1, v2);
            }

            return area;
        }

        // FIXME returns points outside the mesh
        public static IEnumerable<Vector3> SampleRandomPointsInsideMesh(Mesh mesh, int count) {
            Bounds bounds = mesh.bounds;
            Vector3[] points = new Vector3[count];

            int i = 0;
            int failsafe = 0;
            while (i < count) {
                failsafe++;
                if (failsafe > 1000) {
                    Debug.LogWarning("Failed to generate random points inside the mesh.");
                    break;
                }

                Vector3 point = new(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y),
                    Random.Range(bounds.min.z, bounds.max.z)
                );

                // FIXME
                /*
                if (!IsPointInsideMesh(point, mesh))
                    continue;
                */

                points[i] = point;
                i++;
            }

            return points;
        }

        public static IEnumerable<Vector3> SampleRandomPointsOnMesh(Mesh mesh, int count) {
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector3[] points = new Vector3[count];

            // Calculate the area of each triangle
            float[] cumulativeAreas = new float[triangles.Length / 3];
            float totalArea = 0f;

            for (int i = 0; i < triangles.Length; i += 3) {
                Vector3 v0 = vertices[triangles[i]];
                Vector3 v1 = vertices[triangles[i + 1]];
                Vector3 v2 = vertices[triangles[i + 2]];

                float area = Vector3.Cross(v1 - v0, v2 - v0).magnitude * 0.5f;
                totalArea += area;
                cumulativeAreas[i / 3] = totalArea;
            }

            // Sample points
            for (int i = 0; i < count; i++) {
                float randomArea = Random.value * totalArea;
                int triangleIndex = System.Array.BinarySearch(cumulativeAreas, randomArea);
                if (triangleIndex < 0)
                    triangleIndex = ~triangleIndex;

                Vector3 v0 = vertices[triangles[triangleIndex * 3]];
                Vector3 v1 = vertices[triangles[triangleIndex * 3 + 1]];
                Vector3 v2 = vertices[triangles[triangleIndex * 3 + 2]];

                points[i] = SamplePointInTriangle(v0, v1, v2);
            }

            return points;
        }

        public static Vector3 CalculateCenterOfPoints(List<Vector3> points) {
            if (points.Count == 0)
                return Vector3.zero;
            Vector3 center = points.Aggregate(Vector3.zero, (current, point) => current + point);
            center /= points.Count;
            return center;
        }

        private static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3) {
            return Vector3.Dot(p1, Vector3.Cross(p2, p3)) / 6f;
        }

        private static float AreaOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3) {
            return Vector3.Cross(p1 - p2, p1 - p3).magnitude / 2f;
        }

        // FIXME
        private static bool IsPointInsideMesh(Vector3 point, Mesh mesh) {
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            int intersectionCount = 0;

            for (int i = 0; i < triangles.Length; i += 3) {
                Vector3 v0 = vertices[triangles[i]];
                Vector3 v1 = vertices[triangles[i + 1]];
                Vector3 v2 = vertices[triangles[i + 2]];

                Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
                Vector3 toPoint = point - v0;

                // Check if the point is on the positive side of the triangle's plane
                if (!(Vector3.Dot(normal, toPoint) > 0f))
                    continue;

                // Perform ray-triangle intersection test
                if (IntersectsTriangle(point, v0, v1, v2))
                    intersectionCount++;
            }

            // If the number of intersections is odd, the point is inside the mesh
            return intersectionCount % 2 != 0;
        }

        private static bool IntersectsTriangle(Vector3 point, Vector3 v0, Vector3 v1, Vector3 v2) {
            const float EPSILON = 0.000001f;

            Vector3 rayDirection = Vector3.up; // Assuming the ray direction is always up
            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;
            Vector3 h = Vector3.Cross(rayDirection, edge2);
            float a = Vector3.Dot(edge1, h);

            if (a is > -EPSILON and < EPSILON)
                return false; // This ray is parallel to this triangle.

            float f = 1.0f / a;
            Vector3 s = point - v0;
            float u = f * Vector3.Dot(s, h);

            if (u is < 0.0f or > 1.0f)
                return false;

            Vector3 q = Vector3.Cross(s, edge1);
            float v = f * Vector3.Dot(rayDirection, q);

            if (v < 0.0f || u + v > 1.0f)
                return false;

            // At this stage, we can compute t to find out where the intersection point is on the line.
            float t = f * Vector3.Dot(edge2, q);
            return t > EPSILON;
        }

        private static Vector3 SamplePointInTriangle(Vector3 v0, Vector3 v1, Vector3 v2) {
            float u = Random.value;
            float v = Random.value;

            if (u + v > 1) {
                u = 1 - u;
                v = 1 - v;
            }

            return v0 + u * (v1 - v0) + v * (v2 - v0);
        }
    }
}