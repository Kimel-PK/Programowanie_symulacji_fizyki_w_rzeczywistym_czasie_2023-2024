using System;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class WaterSurfaceData : MonoBehaviour
{
    [field: SerializeField] public float FluidDensity { get; private set; } 
    private WaterSurface waterSurface;

    private void Awake() {
        waterSurface = GetComponent<WaterSurface>();
    }

    public float GetWaveHeight(Vector3 position)
    {
        WaterSearchParameters waterSearchParameters = new () {
            targetPosition = new Vector3 (position.x, position.z),
            error = 0.01f,
            maxIterations = 8
        };

        if (waterSurface.FindWaterSurfaceHeight(waterSearchParameters, out WaterSearchResult waterSearchResult)) {
            return waterSearchResult.height;
        }
        return 0;
    }
}