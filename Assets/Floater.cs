using UnityEngine;

public class Floater : MonoBehaviour
{
    public int FloaterCount { get; set; }

    [SerializeField] private Rigidbody rb;
    [SerializeField] private float depthBeforeSubmerged = 1f;
    [SerializeField] private float displacementAmount = 3f;
    [SerializeField] private float waterDrag = 0.99f;
    [SerializeField] private float waterAngularDrag = 0.5f;


    private void FixedUpdate()
    {
        Vector3 position = transform.position;
        float waveHeight = WaveManager.Instance.GetWaveHeight(position);

        rb.AddForceAtPosition(Physics.gravity / FloaterCount, position, ForceMode.Acceleration);

        if (!(position.y < waveHeight))
            return;
        
        float displacementMultiplier = Mathf.Clamp01((waveHeight - position.y) / depthBeforeSubmerged) * displacementAmount;
        rb.AddForceAtPosition(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMultiplier, 0f), position, ForceMode.Acceleration);
        rb.AddForce(-rb.velocity * (displacementMultiplier * waterDrag * Time.fixedDeltaTime), ForceMode.VelocityChange);
        rb.AddForce(-rb.angularVelocity * (displacementMultiplier * waterAngularDrag * Time.fixedDeltaTime), ForceMode.VelocityChange);
    }
}
