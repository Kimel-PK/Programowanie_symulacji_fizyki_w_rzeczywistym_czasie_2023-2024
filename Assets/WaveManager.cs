using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;
    [SerializeField] private WaterSurface waterSurface;

    private void Awake()
    {
        Instance = this;
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
