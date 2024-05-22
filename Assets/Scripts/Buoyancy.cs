using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Buoyancy : MonoBehaviour {
    
    [field: SerializeField] public float Volume { get; set; }
    [field: SerializeField] public float SurfaceArea { get; set; }
    [field: SerializeField] public float DragCoefficient { get; set; }
    [field: SerializeField] public int NumberOfRandomPoints { get; set; }
    
    [field: SerializeField] public List<Vector3> VolumePoints { get; set; }
    [field: SerializeField] public List<Vector3> SurfacePoints { get; set; }
    
    private MeshFilter meshFilter;
    private Mesh mesh;
    private Rigidbody rb;
    private Collider c;
    private WaterSurfaceData waterSurfaceData;
    
    private Vector3 lastFrameBuoyancyCenter;
    private Vector3 lastFrameBuoyancyForce;
    private Vector3[] lastFrameDragForces;
    
    private void OnValidate() {
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.sharedMesh;
        rb = GetComponent<Rigidbody>();
        c = GetComponent<Collider>();
        VolumePoints ??= new List<Vector3>();
        SurfacePoints ??= new List<Vector3>();
        lastFrameDragForces = new Vector3[SurfacePoints.Count];
    }

    private void Awake() {
        if (VolumePoints.Count == 0)
            VolumePoints = MeshMetrics.SampleRandomPointsInsideMesh(meshFilter, NumberOfRandomPoints).ToList();
        if (SurfacePoints.Count == 0)
            SurfacePoints = MeshMetrics.SampleRandomPointsOnMesh(mesh, NumberOfRandomPoints).ToList();
    }

    private void OnTriggerEnter(Collider other) {
        WaterSurfaceData newFluid = other.GetComponent<WaterSurfaceData>();
        if (!newFluid)
            return;
        
        waterSurfaceData = newFluid;
    }
    
    private void OnTriggerExit(Collider other) {
        if (other.GetComponent<WaterSurfaceData>() != waterSurfaceData)
            return;
        
        waterSurfaceData = null;
        lastFrameBuoyancyCenter = Vector3.zero;
        lastFrameBuoyancyForce = Vector3.zero;
        lastFrameDragForces = new Vector3[SurfacePoints.Count];
    }

    private void FixedUpdate() {
        ApplyBuoyancy();
        // ApplyDrag();
        
        double terminalVelocity = Math.Sqrt(2 * rb.mass * Physics.gravity.magnitude / (waterSurfaceData ? waterSurfaceData.FluidDensity : 1.12f * SurfaceArea / 6 * DragCoefficient));
        if (rb.velocity.magnitude > terminalVelocity)
            rb.velocity = rb.velocity.normalized * (float)terminalVelocity;
    }
    
    private void ApplyBuoyancy() {
        if (!waterSurfaceData)
            return;
        
        // Get all points below water surface
        List<Vector3> pointsBelowWater = VolumePoints.Where(volumePoint => transform.TransformPoint(volumePoint).y < waterSurfaceData.GetWaveHeight(volumePoint)).ToList();
        // calculate center
        Vector3 submergedMassCenter = transform.TransformPoint(MeshMetrics.CalculateCenterOfPoints(pointsBelowWater));
        lastFrameBuoyancyCenter = submergedMassCenter;
        // apply buoyancy force
        // F = -density * gravity * volume
        Vector3 buoyancyForce = Physics.gravity * (-waterSurfaceData.FluidDensity * Volume * pointsBelowWater.Count / VolumePoints.Count);
        Debug.Log(buoyancyForce.magnitude);
        lastFrameBuoyancyForce = buoyancyForce;
        rb.AddForceAtPosition(buoyancyForce, submergedMassCenter, ForceMode.Impulse);
    }
    
    private void ApplyDrag() {
        if (!waterSurfaceData)
            return;
        
        // Get all points below water surface
        // apply drag force to every point
        // F = density * 1/2 * velocity^2 * drag coefficient * surface area

        int submergedPoints = SurfacePoints.Count(t => transform.TransformPoint(t).y < waterSurfaceData.GetWaveHeight(t));
        float submergedArea = SurfaceArea * submergedPoints / SurfacePoints.Count;

        for (int i = 0 ; i < SurfacePoints.Count; i++) {
            rb.AddForceAtPosition(-lastFrameDragForces[i], transform.TransformPoint(SurfacePoints[i]));
            rb.angularVelocity = Vector3.zero;
            if (transform.TransformPoint(SurfacePoints[i]).y > waterSurfaceData.GetWaveHeight(SurfacePoints[i])) {
                lastFrameDragForces[i] = Vector3.zero;
                continue;
            }
            
            float velocitySquared = rb.GetPointVelocity(SurfacePoints[i]).magnitude * rb.GetPointVelocity(SurfacePoints[i]).magnitude;
            float dragForce = -waterSurfaceData.FluidDensity * 0.5f * velocitySquared * DragCoefficient / SurfacePoints.Count;
            lastFrameDragForces[i] = rb.GetPointVelocity(SurfacePoints[i]).normalized * dragForce;
            rb.AddForceAtPosition(rb.GetPointVelocity(SurfacePoints[i]).normalized * dragForce, transform.TransformPoint(SurfacePoints[i]));
        }
    }

    private void OnDrawGizmosSelected() {
        
        Gizmos.color = new Color(1f, .0f, 1f);
        Gizmos.DrawSphere(lastFrameBuoyancyCenter, 0.05f);
        
        Gizmos.color = Color.red;
        foreach (Vector3 volumePoint in VolumePoints) {
            Gizmos.DrawSphere(transform.TransformPoint(volumePoint), 0.05f);
        }
        
        Gizmos.DrawLine(lastFrameBuoyancyCenter, lastFrameBuoyancyCenter + lastFrameBuoyancyForce);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + rb.velocity);
        
        return;
        
        Gizmos.color = Color.green;
        foreach (Vector3 surfacePoint in SurfacePoints) {
            Gizmos.DrawSphere(transform.TransformPoint(surfacePoint), 0.05f);
        }
        
        for (int i = 0; i < lastFrameDragForces.Length; i++) {
            Gizmos.DrawLine(transform.TransformPoint(SurfacePoints[i]), transform.TransformPoint(SurfacePoints[i]) + transform.TransformPoint(lastFrameDragForces[i]));
        }
    }
}