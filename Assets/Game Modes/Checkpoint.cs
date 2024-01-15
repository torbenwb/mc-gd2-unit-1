using UnityEngine;
using UnityEngine.Events;

public class Checkpoint : MonoBehaviour
{
    Animator animator;
    bool checkpointEnabled = true;
    public static UnityEvent<Checkpoint, GameObject> OnCheckpointPassed = new UnityEvent<Checkpoint, GameObject>();

    private void Awake() => animator = GetComponent<Animator>();
    
    public void SetCheckpointEnabled(bool newValue) => animator.SetBool("Checkpoint Enabled", (checkpointEnabled = newValue));
    

    private void OnTriggerEnter(Collider other)
    {
        if (!checkpointEnabled) return;

        if (other.attachedRigidbody.CompareTag("Car")){
            OnCheckpointPassed.Invoke(this, other.attachedRigidbody.gameObject);
            
            SetCheckpointEnabled(false);
        }
    }
}
