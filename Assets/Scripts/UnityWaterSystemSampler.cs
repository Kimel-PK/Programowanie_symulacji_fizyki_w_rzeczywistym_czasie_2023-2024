using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace SimpleBuoyancy {
    public class UnityWaterSystemSampler : MonoBehaviour, IWaterSurfaceData {
        [field: SerializeField] public float FluidDensity { get; set; }

        private WaterSurface waterSurface;

        private void Awake() {
            waterSurface = GetComponent<WaterSurface>();
        }

        public float GetWaveHeight(Vector3 position) {
            WaterSearchParameters waterSearchParameters = new() {
                targetPosition = new Vector3(position.x, position.z),
                error = 0.01f,
                maxIterations = 8
            };

            if (waterSurface.FindWaterSurfaceHeight(waterSearchParameters, out WaterSearchResult waterSearchResult)) {
                return waterSearchResult.height;
            }

            return 0;
        }
    }
}