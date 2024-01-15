using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] Car car;
    [SerializeField] RocketBoost rocketBoost;
    [SerializeField] AirControl airControl;

    [SerializeField] bool controlEnabled = false;
    
    public void SetControlEnabled(bool newValue){
        controlEnabled = newValue;

        if (!controlEnabled){
            car.Drive(0f);
            car.Turn(0f);
            car.Brake(0f);
            rocketBoost.ToggleBoost(false);

            airControl.Pitch(0f);
            airControl.Yaw(0f);
            airControl.Roll(0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!controlEnabled) return;
        
        car.Drive(Input.GetAxisRaw("Vertical"));
        car.Turn(Input.GetAxisRaw("Horizontal"));

        if (rocketBoost) rocketBoost.ToggleBoost(Input.GetKey(KeyCode.LeftShift));

        if (airControl)
        {
            if (Input.GetKeyDown(KeyCode.Space)) airControl.Jump();
            airControl.Pitch(Input.GetAxisRaw("Vertical"));
            airControl.Yaw(Input.GetAxisRaw("Horizontal"));
            airControl.Roll(Input.GetAxisRaw("Roll"));
        }
       
    }
}
