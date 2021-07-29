using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    public static float inputWindow;
    public static float prepPhase;

    public static bool gameRunning;
    static bool introDone;

    public static float speed;
    public static float baseSpeed;
    public static float groundSize;

    public GameObject startSignal;
    public GameObject prep;
    public GameObject signal;
    public Transform foreGround;

    public GameManager gameManager;
    public KiwiBehavior kiwi;
    public EagleBehavior eagle;
    public GameObject babies;
    ObstacleSpawner spawner;

    AudioSource[] soundEffects;

    public Sprite[] trampolineSprites; 
    //public Dictionary<string,Sprite> trampolineSprites = new Dictionary<string, Sprite>();
    public static Sprite color;

    float interTrialTimer = 0f;
    bool timing = false;

    void Start()
    {
        SetTrampolineColor(gameManager.trampolineColor);

        inputWindow = gameManager.inputWindowSeconds;
        prepPhase = gameManager.prepPhaseSeconds;

        babies.SetActive(false);
        soundEffects = GetComponents<AudioSource>();
        baseSpeed = gameManager.actualSpeed;
        speed = baseSpeed;
        groundSize = foreGround.transform.localScale.x;

        spawner = GetComponent<ObstacleSpawner>();

        Invoke("StartIntro", 0.5f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !gameRunning && introDone)
        {
            StartGame();
        }


        if (timing)
            interTrialTimer += Time.deltaTime;
    }

    void SetTrampolineColor(TrampolineColor trampolineColor)
    {
        foreach(Sprite sprite in trampolineSprites){
            if(sprite.name == trampolineColor.ToString()){
                color = sprite;
            }
        }
    }

    void StartIntro()
    {
        eagle.FlyOffScreen();
        kiwi.Invoke("Exclaim", 0.5f);
        kiwi.Invoke("ThinkOfBabies", 0.5f);
        Invoke("EndIntro", 2);
    }

    void EndIntro()
    {
        Signals(startSignal, true);
        introDone = true;
        
        gameManager.RunGame();
    }

    void StartGame()
    {
        timing = true;
        gameRunning = true;
        Signals(startSignal, false);
        PlaySoundEffect(0);
        kiwi.ResetThoughts();
        kiwi.Run();

        spawner.SpawnRandomObstacle();
    }

    public void onGameDecision(GameDecisionData decisionData)
    {
        if (!gameRunning)
        {
            StartGame();
        }
        else
        {
            if (decisionData.decision == TrialType.AccInput)
            {
                kiwi.Jump();
                Debug.Log("Input accepted");
            }
            else if (decisionData.decision == TrialType.FabInput)
            {
                kiwi.Jump();
                Debug.Log("Input rejected - fabricating input");
            }
            else
            {
                Debug.Log("Input rejected");
            }
        }
    }

    void Cue()
    {
        Signals(prep, true);
    }

    void Task()
    {
        Signals(prep, false);
        Signals(signal, true);
        gameManager.OpenInputWindow();
    }

    void PositiveFeedback()
    {
        kiwi.Fly();
        PlaySoundEffect(1);
        Signals(signal, false);
        speed = baseSpeed / 2;
    }

    void NegativeFeedback()
    {
        kiwi.SlowDown();
        kiwi.ThinkOfBabies();
        speed = baseSpeed * 2;
    }

    void RestartParadigm()
    {
        interTrialTimer = 0;
        timing = true;
        speed = baseSpeed;
        kiwi.Reset();
        kiwi.ResetThoughts();
        
        if(gameManager.currentTrial < gameManager.trialsTotal)
        {
            spawner.SpawnRandomObstacle();
        }
        else
        {
            babies.SetActive(true);
            Invoke("StopMoving", 3.5f * (baseSpeed / 10));
        }
    }

    void StopMoving()
    {
        gameRunning = false;
        gameManager.EndGame();
        kiwi.SeeBabies(babies.transform.position);
        babies.GetComponentInChildren<EagleBehavior>().Attack();
    }

    void EndGame()
    {
        StopSoundEffect(0);
        kiwi.Idle();
    }

    void PlaySoundEffect(int i)
    {
        soundEffects[i].Play();
    }

    void StopSoundEffect(int i)
    {
        soundEffects[i].Stop();
    }

    void Signals(GameObject toEnable, bool on)
    {
        toEnable.SetActive(on);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        switch (other.tag)
        {
            case "Cue":
                timing = false;
                gameManager.interTrialTimer = interTrialTimer;
                Cue();
                break;

            case "TriggerZone":
                Task();
                break;

            case "Flying":
                PositiveFeedback();
                break;

            case "Slow":
                NegativeFeedback();
                break;

            case "Ground":
                if (gameRunning)
                {
                    kiwi.Fly();
                    kiwi.Run();
                }
                break;

            case "Babies":
                EndGame();
                break;

            default:
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        switch (other.tag)
        {

            case "TriggerZone":
                Signals(signal, false);
                break;

            case "Flying":
                StopSoundEffect(1);
                RestartParadigm();
                kiwi.Descend();
                break;

            case "Slow":
                RestartParadigm();
                break;

            default:
                break;
        }
    }
}