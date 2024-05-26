using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleBuoyancy {

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Buoyancy : MonoBehaviour {

        [field: SerializeField] public float Volume { get; set; }
        [field: SerializeField] public float SurfaceArea { get; set; }
        [field: SerializeField] public float DragCoefficient { get; set; }

        [field: SerializeField] public List<Vector3> VolumePoints { get; set; }
        [field: SerializeField] public List<Vector3> SurfacePoints { get; set; }

        private bool hideGizmos;
        private int numberOfRandomPoints;

        private MeshFilter meshFilter;
        private Rigidbody rb;
        private IWaterSurfaceData waterSurfaceData;

        private void OnValidate() {
            meshFilter = GetComponent<MeshFilter>();
            rb = GetComponent<Rigidbody>();
            VolumePoints ??= new List<Vector3>();
            SurfacePoints ??= new List<Vector3>();
        }

        private void Awake() {
            if (VolumePoints.Count == 0)
                Debug.LogWarning("Volume points are not set, please set them in the inspector.");
            if (SurfacePoints.Count == 0)
                Debug.LogWarning("Surface points are not set, please set them in the inspector.");
        }

        private void OnTriggerEnter(Collider other) {
            IWaterSurfaceData newFluid = other.GetComponent<IWaterSurfaceData>();
            if (newFluid == null)
                return;

            waterSurfaceData = newFluid;
        }

        private void OnTriggerExit(Collider other) {
            if (other.GetComponent<IWaterSurfaceData>() != waterSurfaceData)
                return;

            waterSurfaceData = null;
        }

        private void OnDrawGizmosSelected() {
            if (hideGizmos)
                return;

            Gizmos.color = Color.red;
            foreach (Vector3 volumePoint in VolumePoints)
                Gizmos.DrawSphere(transform.TransformPoint(volumePoint), 0.03f);

            Gizmos.color = Color.green;
            foreach (Vector3 surfacePoint in SurfacePoints)
                Gizmos.DrawSphere(transform.TransformPoint(surfacePoint), 0.03f);
        }

        private void FixedUpdate() {
            ArchimedesPrinciple();
        }

        private void ArchimedesPrinciple() {
            if (waterSurfaceData == null)
                return;

            ApplyBuoyancy();
            ApplyDrag();
        }

        private void ApplyBuoyancy() {
            // apply buoyancy force
            // F = -density * gravity * volume

            List<Vector3> pointsBelowWater = VolumePoints.Where(volumePoint =>
                transform.TransformPoint(volumePoint).y < waterSurfaceData.GetWaveHeight(volumePoint)).ToList();

            Vector3 submergedMassCenter =
                transform.TransformPoint(MeshMetrics.CalculateCenterOfPoints(pointsBelowWater));

            Vector3 buoyancyForce = Physics.gravity * (-waterSurfaceData.FluidDensity * Volume *
                pointsBelowWater.Count / VolumePoints.Count);

            rb.AddForceAtPosition(buoyancyForce, submergedMassCenter, ForceMode.Force);
        }

        private void ApplyDrag() {
            // get all points below water surface
            // apply drag force to every point
            // F = density * 1/2 * velocity^2 * drag coefficient * surface area

            List<Vector3> submergedPoints = SurfacePoints
                .Where(t => transform.TransformPoint(t).y < waterSurfaceData.GetWaveHeight(t)).ToList();

            foreach (Vector3 point in submergedPoints) {
                float velocitySquared = rb.GetRelativePointVelocity(point).magnitude *
                                        rb.GetRelativePointVelocity(point).magnitude;
                float dragForce = -waterSurfaceData.FluidDensity * 0.5f * velocitySquared * DragCoefficient /
                                  SurfacePoints.Count;
                rb.AddForceAtPosition(dragForce * rb.GetPointVelocity(transform.TransformPoint(point)),
                    transform.TransformPoint(point), ForceMode.Force);
            }
        }
    }
}