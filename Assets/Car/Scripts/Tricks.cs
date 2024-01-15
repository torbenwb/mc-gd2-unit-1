using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AirControl))]
public class Tricks : MonoBehaviour
{
    Car car;
    AirControl airControl;
    int score = 0;
    private void Awake()
    {
        car = GetComponent<Car>();
        airControl = GetComponent<AirControl>();
    }
    
    public float pitchRotation, rollRotation, yawRotation;
    bool grounded = false;
    [SerializeField] float[] landingAngles;
    [SerializeField] int[] landingMultipliers;
    [SerializeField] string[] landingText;
    [SerializeField] int pitchMod = 3;
    [SerializeField] int yawMod = 1;
    [SerializeField] int rollMod = 2;

    int currentCombo(){
        return Mathf.RoundToInt((pitchMod * Mathf.Abs(Mathf.Deg2Rad * pitchRotation)) + (yawMod * Mathf.Abs(Mathf.Deg2Rad * yawRotation)) + (rollMod * Mathf.Abs(Mathf.Deg2Rad * rollRotation)));
    }
    int comboCounter = 0;
    public float airTime = 0f;
    public float airTimeThreshold = 0.2f;

    private void OnCollisionEnter(Collision other)
    {
        landTrick();
    }

    void landTrick(){
        if (airTime < airTimeThreshold){
            airTime = 0f; 
            return;
        }
        Debug.Log("Tricks Collision Enter");
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, 2f)){
            Vector3 normal = hit.normal;
            float angle = Vector3.Angle(transform.up, normal);

            int multiplier = getLandingMultiplier(angle);
        }
        else{
            PlayerUI.SetText("Landing", "So close!");
            Invoke("clearLandingText", 1f);
        }
        pitchRotation = rollRotation = yawRotation = 0f;
        airTime = 0f;
    }

    int getLandingMultiplier(float angle){
        int index = 0;
        for(int i = 0; i < landingAngles.Length - 1; i++){
            index = i;
            if (angle < landingAngles[i + 1]){
                
                break;
            }
        }
        if (index == landingAngles.Length) return 0;
        PlayerUI.SetText("Landing", landingText[index]);
        int scoreAdd = currentCombo() * landingMultipliers[index];
        score += scoreAdd;

        PlayerUI.SetText("Score", score.ToString());
        Invoke("clearLandingText", 1f);
        return landingMultipliers[index];
    }

    void clearLandingText(){
        PlayerUI.SetText("Landing", "");
    }

    // Update is called once per frame
    void Update()
    {
        if (car.GetGrounded()){
            if (airTime > airTimeThreshold) landTrick();
            else{
                PlayerUI.SetText("Combo", "");
                comboCounter = 0;
                airTime = 0f;
            }
            
            return;
        }
        airTime += Time.deltaTime;
        if (airTime < airTimeThreshold) return;
        pitchRotation += airControl.PitchRotation;
        rollRotation += airControl.RollRotation;
        yawRotation += airControl.YawRotation;

        if (currentCombo() > comboCounter) comboCounter++;
        else comboCounter = currentCombo();
        PlayerUI.SetText("Combo", comboCounter.ToString());
        
    }
}
