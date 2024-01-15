using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Car))]
[RequireComponent(typeof(Rigidbody))]
public class FlipVolume : MonoBehaviour
{
    Car car;
    Rigidbody rigidbody;
    [SerializeField] float traceDistance = 1f;
    [SerializeField] float flipForce = 1f;
    void Awake(){
        rigidbody = GetComponent<Rigidbody>();
        car = GetComponent<Car>();
    }

    private void Update()
    {
        if (car.GetGrounded()) return;

        if (Physics.Raycast(transform.position, transform.up, traceDistance)){
            rigidbody.AddForce(-transform.up * flipForce, ForceMode.Impulse);
        }
    }
}
