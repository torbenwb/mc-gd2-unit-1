using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarLookTarget : MonoBehaviour
{
    [SerializeField] Rigidbody rigidbody;
    [SerializeField] float offsetMultiplier = 1f;
    [SerializeField] Vector2 offsetClamp = new Vector2(1,2);
    

    // Update is called once per frame
    void Update()
    {
        Vector3 offset = rigidbody.velocity;
        offset.y = 0f;
        if (offset.magnitude <= 0f){
            transform.position = rigidbody.transform.position + rigidbody.transform.forward;
            return;
        }
        if (Vector3.Dot(rigidbody.transform.forward, offset) < 0f) offset = rigidbody.transform.forward;
        if (offset.magnitude < offsetClamp.x) offset = (offset.normalized * offsetClamp.x);
        if (offset.magnitude > offsetClamp.y) offset = (offset.normalized * offsetClamp.y);
        
        
        transform.position = rigidbody.transform.position + (offset * offsetMultiplier);
    }
}
