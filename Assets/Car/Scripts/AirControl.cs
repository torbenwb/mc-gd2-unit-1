using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Car))]
public class AirControl : MonoBehaviour
{
    Car car;
    Rigidbody rigidbody;

    [Header("Jump")]
    [SerializeField] float jumpStrength;
    [SerializeField] float airMod;
    [SerializeField] int airJumps;
    [SerializeField] int maxAirJumps;
    [SerializeField] ParticleSystem jumpFX;

    [Header("Air Rotation")]
    float yawAxis, rollAxis, pitchAxis;
    [SerializeField] float yawRate;
    [SerializeField] float pitchRate;
    [SerializeField] float rollRate;
    [SerializeField] float rotationDamping;
    [SerializeField] Vector2 delay = new Vector2(0f, 1f);

    float yawRotation;
    float pitchRotation;
    float rollRotation;

    public float YawRotation {get => yawRotation;}
    public float PitchRotation {get => pitchRotation;}
    public float RollRotation {get => rollRotation;}
    


    private void Awake()
    {
        car = GetComponent<Car>();
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (car.GetGrounded()) {
            airJumps = maxAirJumps;
            delay.x = delay.y;
        }
        else{
            if (delay.x > 0f) delay.x -= Time.fixedDeltaTime;
            else{
                ApplyAirRotationForce();
            }
            
        }
    }

    public void Jump(){
        ApplyJumpForce();
    }

    public void Pitch(float pitchAxis){
        this.pitchAxis = Mathf.Clamp(pitchAxis, -1, 1);
    }

    public void Yaw(float yawAxis){
        this.yawAxis = Mathf.Clamp(yawAxis, -1, 1);
    }

    public void Roll(float rollAxis){
        this.rollAxis = Mathf.Clamp(rollAxis, -1, 1);
    }

    private void ApplyJumpForce(){
        if (car.GetGrounded()){
            yawRotation = pitchRotation = rollRotation = 0f;
            rigidbody.AddForce(transform.up * jumpStrength, ForceMode.Impulse);
            jumpFX.Play();
        }
        else{
            if (Physics.Raycast(transform.position, transform.up, 2f)){
                rigidbody.AddForce(-transform.up * jumpStrength, ForceMode.Impulse);
            }
            if (airJumps > 0){
                rigidbody.AddForce(transform.up * jumpStrength * airMod, ForceMode.Impulse);
                jumpFX.Play();
                airJumps--;
            }
        }
    }

    private void ApplyAirRotationForce(){
        float pitchVelocity = Vector3.Dot(transform.right, rigidbody.angularVelocity);
        float yawVelocity = Vector3.Dot(transform.up, rigidbody.angularVelocity);
        float rollVelocity = Vector3.Dot(transform.forward, rigidbody.angularVelocity);

        float yawTorque = (Mathf.Abs(yawAxis) > 0f) ? yawAxis * yawRate : -yawVelocity * rotationDamping;
        float rollTorque = (Mathf.Abs(rollAxis) > 0f) ? rollAxis * rollRate : -rollVelocity * rotationDamping;
        float pitchTorque = (Mathf.Abs(pitchAxis) > 0f) ? pitchAxis * pitchRate : -pitchVelocity * rotationDamping;

        rigidbody.AddTorque(transform.up * yawTorque);
        rigidbody.AddTorque(transform.forward * rollTorque);
        rigidbody.AddTorque(transform.right * pitchTorque);


        yawRotation = (Mathf.Abs(yawAxis) > 0f) ? yawAxis * yawRate : 0f;
        rollRotation = (Mathf.Abs(rollAxis) > 0f) ? rollAxis * rollRate : 0f;
        pitchRotation = (Mathf.Abs(pitchAxis) > 0f) ? pitchAxis * pitchRate : 0f;
    }

}
