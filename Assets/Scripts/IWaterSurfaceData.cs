using UnityEngine;

namespace SimpleBuoyancy {
    public interface IWaterSurfaceData {
        public float FluidDensity { get; set; }
        public float GetWaveHeight(Vector3 position);
    }
}