using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBodyTarget : MonoBehaviour
{
    public Rigidbody rigidbody;
    

    // Update is called once per frame
    void Update()
    {
        
        transform.position = rigidbody.position;
        Vector3 velocity = rigidbody.velocity;
        velocity.y = 0f;
        if (velocity.magnitude > 0f){
            if (Vector3.Dot(transform.forward, velocity) < 0f){
                transform.rotation = transform.rotation = Quaternion.LookRotation(rigidbody.transform.forward);
            }
            else{ transform.rotation = Quaternion.LookRotation(velocity);}
        }
        else{
            transform.rotation = Quaternion.LookRotation(rigidbody.transform.forward);
        }
        
        
    }
}
