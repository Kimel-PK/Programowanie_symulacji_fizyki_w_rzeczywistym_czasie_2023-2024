using UnityEngine;

public class Boat : MonoBehaviour
{
    [SerializeField] private Floater[] floaters;

    private void Awake()
    {
        foreach (Floater floater in floaters)
        {
            floater.FloaterCount = floaters.Length;
        }
    }
}
