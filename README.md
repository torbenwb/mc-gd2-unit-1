# Rocket_Kart_Racing

In this project my goal was to create a racing game using a physics driven car model while incorporating the player abilities from Rocket League specifically the abilities to jump / air jump, control the rotation of the car in the air, and to rocket boost in the car's forward direction. With the car and abilities, I created a simple time trials game mode designed to require the use of these abilities.

You can play the current WebGL build of the game [here](https://torbenwb.github.io/RocketKartRacing_WebGL/)

- Programmed car physics including suspension spring force, longitudinal and lateral friction.
- Programmed rocket air control gameplay systems using rigidbody physics.
- Programmed extensible checkpoint and racing system.

## Physics Driven Car

![](https://github.com/torbenwb/MC_Rocket_Kart_Racing/blob/main/ReadMe_Images/Chapter_1.gif)

The first step in building any racing / car based action game is the car. In this project the car is controlled by a very simple public interface which allows the player to `Drive()`, `Turn()` and `Brake()`, however internally the motion and rotation of the car is controlled through a series of forces:

* **Suspension Force**: Calculates an upwards spring force based on how close the car is to the ground.
* **Longitudinal Force**: Forward / backwards force based on a combination of user input to drive forward and backwards and a friction coefficient to stop / slow the car's movement when on the ground.
* **Lateral Force**: Right/Left force provides force oppositional to lateral velocity based on friction coefficient.
* **Turning Force**: Converts user input and forward velocity into torque to turn the car at a certain rate while driving forward / backward.

### Suspension Force 

Suspension force is calculated per wheel touching the ground. For each wheel, the `Car` script performs a downwards raycast at the wheel's position. The distance between the desired end of the raycast and the hit point determines the `offset`. The greater the `offset` the more spring force applied to push the car back up.

```cs
private void ApplySuspensionForce()
  {
      bool tempGrounded = false;

      foreach(Transform wheel in wheels)
      {
          Vector3 origin = wheel.position;
          Vector3 direction = -wheel.up;
          RaycastHit hit;
          float offset = 0f;

          if (Physics.Raycast(origin,direction,out hit, wheelRadius)){
              tempGrounded = true;

              Vector3 end = origin + (direction * wheelRadius);
              offset = (end - hit.point).magnitude;

              float pointVelocity = Vector3.Dot(wheel.up, rigidbody.GetPointVelocity(wheel.position));
              float suspensionForce = (springStrength * offset) + (-pointVelocity * springDamping);
              rigidbody.AddForceAtPosition(wheel.up * suspensionForce, wheel.position);

              wheel.GetChild(0).transform.localPosition = Vector3.up * offset;
          }
      }

      grounded = tempGrounded;
  }
```

### Longitudinal Force

The longitudinal force is calculated by determining whether or not the player is trying to move forward / backward. If so, then friction is ignored and instead we apply force porportional to the player's input axis and the ratio of remaining speed. This ratio is defined by how fast the car is currently moving in its forwad direction. If the car is already moving at max speed, then no force will be applied.

When the player is not applying input, the force applied will be proportional to the current velocity of the car multiplied by the longitudinal friction coefficient.

```cs
private void ApplyLongitudinalForce()
{
    Vector3 force = Vector3.zero;
    float forwardVelocity = Vector3.Dot(transform.forward, rigidbody.velocity);
    float maxSpeedRatio = (1 - (Mathf.Abs(forwardVelocity) / maxSpeed));

    if (Mathf.Abs(driveAxis) > 0){
       force = transform.forward * driveAxis * maxSpeed * maxSpeedRatio;
    }
    else{
        force = transform.forward * -forwardVelocity * longitudinalFriction;
    }

    rigidbody.AddForce(force);
}
```

### Lateral Force

The lateral force is calculated similarly to the longitudinal force except without user input, as such oppositional force is constantly applied.

```cs
private void ApplyLateralForce()
{
    float rightVelocity = Vector3.Dot(transform.right, rigidbody.velocity);
    rigidbody.AddForce(transform.right * -rightVelocity * lateralFriction);
}
```

### Turning Force

The final force to apply is the turning force (torque) which is proportional to the car's current forward velocity. In a more accurate physics simulation the torque would be calculated by applying forces at the position of each wheel but since I'm aiming for a more arcadey feel I decided to opt for a simpler approximation.

```cs
private void ApplyTurningForce()
{
    float forwardVelocity = Vector3.Dot(transform.forward, rigidbody.velocity);
    float rotationalVelocity = Vector3.Dot(transform.up, rigidbody.angularVelocity);

    float torque = forwardVelocity * turnAxis * (Mathf.Deg2Rad * steeringAngle);
    torque += -rotationalVelocity * turnDamping;

    rigidbody.AddTorque(transform.up * torque);
}
```

## Player Abilities

![](https://github.com/torbenwb/MC_Rocket_Kart_Racing/blob/main/ReadMe_Images/Chapter_2.gif)

After implementing the car controller, it was time to move on to the Rocket League player abilities. To avoid making changes to the `Car` script, these abilities are implemented in the `AirControl` and `RocketBoost` scripts.

I reverse engineered these mechanics as follows:

### Jump / Air Jump

In Rocket League, the player has access to a jump when on the ground as well as a single, less powerful jump while in the air. This air jump resets when the player touches the ground. Jump force is applied as an impulse force in the car's upwards direction so if the car is rotated in any way that will affect the direction in which force is applied.

```cs
private void ApplyJumpForce(){
    if (car.GetGrounded()){
        yawRotation = pitchRotation = rollRotation = 0f;
        rigidbody.AddForce(transform.up * jumpStrength, ForceMode.Impulse);
        jumpFX.Play();
    }
    else{
        if (airJumps > 0){
            rigidbody.AddForce(transform.up * jumpStrength * airMod, ForceMode.Impulse);
            jumpFX.Play();
            airJumps--;
        }
    }
}
```

### Rotation Control

To control the rotation of the car in the air, we have to determine how quickly the car is already rotating in each direction. If the player is attempting to rotate further in that direction, torque is applied proportional the corresponding rotation rate, if the player is not attempting to rotate in that direction then oppositional damping force is applied instead.

```cs
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
```

### Rocket Boost

The boost ability is intended not only to help the player achieve higher speeds but also to control their movement while in the air. As such, the boost should propel the player forward in whatever direction they are currently facing. The rocket boost requires a boost resource which depletes while in use, but can be replenished by pickups on the track.

#### `RocketBoost`

```cs
private void FixedUpdate()
{
    PlayerUI.SetImageFill("Boost Meter Fill", boost / maxBoost);
    if (!active || boost <= 0f) {
        particleSystem.Stop();
        return;
    }

    float forwardVelocity = Vector3.Dot(transform.forward, rigidbody.velocity);
    float speedRatio = (1 - (forwardVelocity / forceStrength));
    rigidbody.AddForce(transform.forward * forceStrength * speedRatio);
    boost -= Time.fixedDeltaTime;
}
```

#### `Pickup`

The boost pickup script is intended to be easily reusable for other pickups so instead of getting a reference to a `RocketBoost` component and calling `MaxBoost()` it sends a message using the `GameObject.SendMessage()` method.

```cs
private void OnTriggerEnter(Collider other)
{
    if (active && other.attachedRigidbody.gameObject.CompareTag(validTag)){
        other.attachedRigidbody.gameObject.SendMessage(messageName, SendMessageOptions.DontRequireReceiver);
        ToggleActive(false);
        StartCoroutine(Respawn());
        Instantiate(particleSystemPrefab,transform.position, transform.rotation);
    }
}
```

## Time Trials 

![](https://github.com/torbenwb/MC_Rocket_Kart_Racing/blob/main/ReadMe_Images/Chapter_3.gif)

After implementing all of the player's core abilities and mechanics, it's time to move on to a game mode that requires the player to use their abilities. The Time Trials game mode was chosen because it can be easily implemented without adding support for AI racers or mulitplayer. One of the main design goals when implementing this was to eliminate all coupling between scripts and the current game mode. In short: the game mode script may rely on specific scripts / components but no other scripts should rely on the game mode.

For example: the Time Trials game mode uses checkpoints, but we might want to reuse these checkpoints in another game mode so we want to make sure to design the checkpoints with this in mind.

The first step in creating the Time Trials game mode was determining the basic progression of each round as well as the player's goal / win condition.

* **Win Condition**: Reach the end of the race faster than the previous fastest time.
* **Progression**: 
  * Pre race setup
  * Start countdown
  * Start race
  * End race

To keep things organized I turned the progression described above into series of methods for the `TimeTrials` script which are called at the beginning of each phase.

* `PreRaceSetup` performs all necessary setup such as getting a reference to the player and disabling control until the end of the countdown.
* `StartCountdown` pretty much just invokes `StartRace` after a certain amount of seconds.
* `StartRace` enables player control.
* `EndRace` is called when the player passes the first checkpoint for the final time, depending on the number of laps.

The `Checkpoint` and `BoundsVolume` scripts are designed to mark the progression of the player and the bounds of the level respectively. Each of them are entirely game mode agnostic and simple detect when an object of the correct type (object with tag `Car`) has entered their trigger volume, and if so they fire an event to which the game mode is listening.

#### `Checkpoint`

```cs
private void OnTriggerEnter(Collider other)
{
    if (!checkpointEnabled) return;

    if (other.attachedRigidbody.CompareTag("Car")){
        OnCheckpointPassed.Invoke(this, other.attachedRigidbody.gameObject);

        SetCheckpointEnabled(false);
    }
}
```

#### `TimeTrials`

Whenever a checkpoints is passed it's disabled, the checkpoint index is incremented / looped and then the next checkpoint is enabled.

```cs
void OnCheckpointPassed(Checkpoint checkpoint, GameObject gameObject){
    checkpoint.SetCheckpointEnabled(false);

    // If passed first checkpoint - new lap.
    if (checkpointIndex == 0){
        currentLap++;
        if (currentLap > totalLaps) EndRace();
    }

    checkpointIndex++;
    if (checkpointIndex >= checkpoints.Count) checkpointIndex = 0;
    checkpoints[checkpointIndex].SetCheckpointEnabled(true);
}
```

## Polish and UI

![](https://github.com/torbenwb/MC_Rocket_Kart_Racing/blob/main/ReadMe_Images/Chapter_4.gif)

With the game mode and player abilities implemented it was time to add some finishing touches and user interface as well as saving / loading the player's high score at the end of each race.

### Player UI

My goal with the `PlayerUI` script was to make it as easy as possible for the various scripts that needed to update UI information to do so. To implement this I added a Dictionary with a list of `Text` components mapped to a given `string` key. Any other script need simply call `SetText()` providing the key and the updated text and then all `Text` components corresponding to that key will be updated.

```cs
public static void SetText(string key, string newText){
    if (textComponentMap.ContainsKey(key)){
        List<Text> l = textComponentMap[key];
        int i = 0;
        while(i < l.Count){
            if(l[i]){
                l[i].text = newText;
                i++;
            }
            else{
                l.RemoveAt(i);
            }
        }
    }
}
```

This same method was applied later to create the boost meter which displays the player's remaining boost resource. Here instead of `Text` components we're updating the `fillAmount` of various images.

```cs
public static void SetImageFill(string key, float fill){
    if (imageComponentMap.ContainsKey(key)){
        List<Image> l = imageComponentMap[key];
        int i = 0;
        while(i < l.Count){
            if(l[i]){
                l[i].fillAmount = fill;
                i++;
            }
            else{
                l.RemoveAt(i);
            }
        }
    }
}
```

Since these dictionaries are static and may persist between scenes, each time we update we also check if any of the components in the collection have become `null` and if so they are removed from the collection.

### High Score 

To avoid writing more complex save/load systems I opted to instead use the `PlayerPrefs` class to save the player's best time with player preferences. At the end of each race, the player's high score is loaded using `GetFloat` and the `trackName` (this allows for multiple tracks in the future). The current high score is compared against the player's new race time, and if the new race time is less than the current high score, **OR** there isn't yet a high score, the high score is updated and saved.

```cs
private void HighScore(){
    float highScore = PlayerPrefs.GetFloat($"{trackName}_HighScore",0.0f);
    float raceTime = GetRaceTime();
    if (raceTime < highScore || highScore == 0f){
        highScore = raceTime;
        PlayerPrefs.SetFloat($"{trackName}_HighScore",highScore);
        PlayerPrefs.Save();
        PlayerUI.SetText("HighScore", $"New High Score: {highScore}");

    }
    Invoke("Restart", 3f);
}
```
