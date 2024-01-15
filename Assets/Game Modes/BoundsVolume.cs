using UnityEngine;
using UnityEngine.Events;

public class BoundsVolume : MonoBehaviour
{
    public static UnityEvent<Rigidbody> OnBoundsVolumeEnter = new UnityEvent<Rigidbody>();

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.attachedRigidbody.gameObject.CompareTag("Car")){
            OnBoundsVolumeEnter.Invoke(other.attachedRigidbody);
        }
    }
}
