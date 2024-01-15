using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    private void Awake()
    {
        BoundsVolume.OnBoundsVolumeEnter.AddListener(OnBoundsVolumeEnter);
    }

    void OnBoundsVolumeEnter(Rigidbody rigidbody){
        
        rigidbody.transform.position = Vector3.up * 2f;
        rigidbody.transform.rotation = Quaternion.identity;
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
    }
}
