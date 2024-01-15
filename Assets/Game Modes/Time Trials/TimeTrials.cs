using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeTrials : MonoBehaviour
{
    Player player;
    [SerializeField] float countdownTime = 3f;
    [SerializeField] List<Checkpoint> checkpoints; // Store checkpoints in sequential order
    [SerializeField] int checkpointIndex; // Keep track of next target checkpoint

    [SerializeField] string trackName;
    [SerializeField] int totalLaps = 3;
    int currentLap = 0;
    float startTime;
    bool raceActive = false;
    float GetRaceTime() => (Time.time - startTime) - countdownTime;
    float GetCountdownTime() => countdownTime - (Time.time - startTime);
    

    private void Start()
    {
        PlayerPrefs.DeleteAll();
        PreRaceSetup();
    }

    private void Update()
    {
        if (!raceActive) return;
        if (Input.GetKeyDown(KeyCode.Z)) ResetPlayer();

        if (GetCountdownTime() > 0f){
            PlayerUI.SetText("Countdown", GetCountdownTime().ToString("0"));
        }
        else{
            PlayerUI.SetText("Countdown", "");
            PlayerUI.SetText("RaceTime", GetRaceTime().ToString("0.00"));
        }
    }

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

    void OnBoundsVolumeEnter(Rigidbody rigidbody){
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;

        int lastCheckpointIndex = checkpointIndex - 1;
        if (lastCheckpointIndex < 0) lastCheckpointIndex = checkpoints.Count - 1;

        rigidbody.transform.position = checkpoints[lastCheckpointIndex].transform.position;
        rigidbody.transform.rotation = Quaternion.LookRotation(-checkpoints[lastCheckpointIndex].transform.up);
    }

    public void ResetPlayer(){
        Rigidbody rigidbody = player.gameObject.GetComponentInChildren<Rigidbody>();

        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;

        int lastCheckpointIndex = checkpointIndex - 1;
        if (lastCheckpointIndex < 0) lastCheckpointIndex = checkpoints.Count - 1;

        rigidbody.transform.position = checkpoints[lastCheckpointIndex].transform.position;
        rigidbody.transform.rotation = Quaternion.LookRotation(-checkpoints[lastCheckpointIndex].transform.up);
    }

    private void PreRaceSetup(){
        player = FindObjectOfType<Player>();
        player.SetControlEnabled(false);

        Checkpoint.OnCheckpointPassed.AddListener(OnCheckpointPassed);
        BoundsVolume.OnBoundsVolumeEnter.AddListener(OnBoundsVolumeEnter);

        foreach(Checkpoint checkpoint in checkpoints){
            checkpoint.SetCheckpointEnabled(false);
        }
        checkpoints[checkpointIndex].SetCheckpointEnabled(true);

        startTime = Time.time;
        raceActive = true;
        
        StartCountdown();
    }

    private void StartCountdown(){
        Invoke("StartRace", countdownTime);
    }

    private void StartRace(){
        player.SetControlEnabled(true);
    }

    private void EndRace(){
        raceActive = false;
        player.SetControlEnabled(false);
        
        //HighScore();
        Leaderboard();
    }

    private void Restart(){
        SceneManager.LoadScene("TimeTrials");
    }

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

    private void Leaderboard(){
        int leaderboardCount = PlayerPrefs.GetInt($"{trackName}_leaderboardCount", 0);
        List<float> scores = new List<float>();
        for(int i = 0; i < leaderboardCount; i++){
            scores.Add(PlayerPrefs.GetFloat($"{trackName}_score[{i}]"));
        }
        scores.Add(GetRaceTime());
        leaderboardCount = scores.Count;
        scores.Sort();
        //scores.Reverse();
        PlayerPrefs.SetFloat($"{trackName}_score[{scores.Count - 1}]", GetRaceTime());
        PlayerPrefs.SetInt($"{trackName}_leaderboardCount", leaderboardCount);

        string leaderboard = "";
        for (int i = 0; i < 5 && i < leaderboardCount; i++){
            leaderboard += $"{i + 1}.   {scores[i].ToString("0.000\n")}";
        }
        
        PlayerUI.SetText("Leaderboard", leaderboard);
        Invoke("Restart", 5f);
    }
}
