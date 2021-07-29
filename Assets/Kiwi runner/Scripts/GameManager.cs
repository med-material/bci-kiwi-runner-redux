using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public struct GameData
{
    public int trials;
    public float interTrialIntervalSeconds;
    public float inputWindowSeconds;
    public GameState gameState;
    public float noInputReceivedFabAlarm;
    public float fabAlarmVariability;
}

public class Mechanism
{
    public string name = "";
    public TrialType trialType;
    public float rate = -1f;
    public int trialsLeft = -1;
    public int trials = -1;
    public UrnEntryBehavior behavior;
}

public class InputData
{
    public InputValidity validity;
    public InputType type;
    public float confidence;
    public int inputNumber;
}

public enum InputValidity
{
    Accepted,
    Rejected
}

public enum InputType
{
    KeySequence,
    MotorImagery,
    BlinkDetection,
    FabInput
}

public class GameDecisionData
{
    public TrialType decision;
    public float currentFabAlarm;
}

public struct GameTimers
{
    public float inputWindowTimer;
    public float interTrialTimer;
}

public enum InputWindowState
{
    Closed,
    Open,
}

public enum GameState
{
    Running,
    Paused,
    Stopped
}

public enum TrialType
{
    AccInput,
    FabInput,
    RejInput
}

public enum TrampolineColor
{
    Blue,
    Red,
    Green,
    Yellow
}

public class GameManager : MonoBehaviour
{
    [Header("Game setup")]
    [Range(0, 10)]
    public float gameSpeed;
    [HideInInspector]
    public float actualSpeed;

    public TrampolineColor trampolineColor = TrampolineColor.Blue;

    [Header("Trial Setup")]
    [Tooltip("The total number of trials is calculated from the trial counts set here.")]
    public int rejTrials = 5;
    public int accTrials = 10;
    public int fabTrials = 5;

    // made public for kiwi runner
    [HideInInspector]
    public int trialsTotal = -1;
    [HideInInspector]
    public int currentTrial = -1;

    private TrialType trialResult = TrialType.RejInput;
    private TrialType trialGoal = TrialType.RejInput;

    private Dictionary<string, Mechanism> mechanisms = new Dictionary<string, Mechanism>();

    [Header("FabInput Settings")]
    [Tooltip("When should the fabrication fire.")]
    [SerializeField]
    private float noInputReceivedFabAlarm = 0.5f; // fixed alarm in seconds relative to input window, at what point should we try and trigger fab input.
    [SerializeField]
    private float fabAlarmVariability = 0.5f; //added delay variability to make the alarm unpredictable.
    private float currentFabAlarm = 0f;
    private bool alarmFired = false;
    private int fabInputNumber = 0;


    [Header("InputWindow Settings")]
    [SerializeField]
    public float prepPhaseSeconds = 1f; 
    public float inputWindowSeconds = 1f; 

    private float inputWindowTimer = 0.0f;
    [HideInInspector]
    public float interTrialTimer = 0.0f;
    private InputWindowState inputWindow = InputWindowState.Closed;
    private int inputIndex = 0;

    private GameState gameState = GameState.Stopped;

    [Serializable]
    public class OnGameStateChanged : UnityEvent<GameData> { }
    public OnGameStateChanged onGameStateChanged;
    [Serializable]
    public class GameDecision : UnityEvent<GameDecisionData> { }
    public GameDecision gameDecision;

    [Serializable]
    public class OnInputWindowChanged : UnityEvent<InputWindowState> { }
    public OnInputWindowChanged onInputWindowChanged;

    private LoggingManager loggingManager;
    private UrnModel urn;


    void Start()
    {
        Application.targetFrameRate = 30;
        actualSpeed = 20 - gameSpeed;
        loggingManager = GameObject.Find("LoggingManager").GetComponent<LoggingManager>();
        urn = GetComponent<UrnModel>();
        SetupMechanisms();
        SetupUrn();
        LogMeta();
    }

    private void SetupMechanisms()
    {
        mechanisms["AccInput"] = new Mechanism
        {
            name = "AccInput",
            trialType = TrialType.AccInput,
            rate = 0f,
            trials = accTrials,
            trialsLeft = accTrials,
            behavior = UrnEntryBehavior.Persist
        };

        mechanisms["FabInput"] = new Mechanism
        {
            name = "FabInput",
            trialType = TrialType.FabInput,
            rate = 0f,
            trials = fabTrials,
            trialsLeft = fabTrials,
            behavior = UrnEntryBehavior.Persist
        };

        mechanisms["RejInput"] = new Mechanism
        {
            name = "RejInput",
            trialType = TrialType.RejInput,
            rate = 0f,
            trials = rejTrials,
            trialsLeft = rejTrials,
            behavior = UrnEntryBehavior.Override
        };
    }

    private void SetupUrn()
    {
        trialsTotal = 0;
        foreach (KeyValuePair<string, Mechanism> pair in mechanisms)
        {
            var m = pair.Value;
            urn.AddUrnEntryType(m.name, m.behavior, m.trials);
            trialsTotal += m.trials;
        }

        urn.NewUrn();
        currentTrial = 0;
    }

    private void LogMeta()
    {
        Dictionary<string, object> metaLog = new Dictionary<string, object>() {
            {"FabInputTrials", fabTrials},
            {"AccInputTrials", accTrials},
            {"RejInputTrials", rejTrials},
            {"Trials", trialsTotal},
            {"InputWindow_sec", inputWindowSeconds},
            {"PrepPhase_sec", prepPhaseSeconds},
            {"noInputReceivedFabAlarm_sec", noInputReceivedFabAlarm},
            {"FabAlarmVariability_sec", fabAlarmVariability},
            {"GameSpeed", gameSpeed},
            {"TrampolineColor", trampolineColor},
        };
        loggingManager.Log("Meta", metaLog);
    }

    private void LogEvent(string eventLabel)
    {
        Dictionary<string, object> gameLog = new Dictionary<string, object>() {
            {"Event", eventLabel},
            {"InputWindow", System.Enum.GetName(typeof(InputWindowState), inputWindow)},
            {"InputWindowOrder", inputIndex},
            {"InterTrialTimer", interTrialTimer},
            {"InputWindowTimer", inputWindowTimer},
            {"GameState", System.Enum.GetName(typeof(GameState), gameState)},
            {"CurrentFabAlarm", currentFabAlarm},
        };

        foreach (KeyValuePair<string, Mechanism> pair in mechanisms)
        {
            var m = pair.Value;
            gameLog[m.name + "TrialsLeft"] = m.trialsLeft;
            gameLog[m.name + "Rate"] = m.rate;
        }

        if (eventLabel == "GameDecision")
        {
            gameLog["TrialGoal"] = trialGoal;
            gameLog["TrialResult"] = trialResult;
        }
        else
        {
            gameLog["TrialGoal"] = "NA";
            gameLog["TrialResult"] = "NA";
        }

        loggingManager.Log("Game", gameLog);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameState == GameState.Running)
        {
            if (inputWindow == InputWindowState.Closed)
            {
                alarmFired = false;
            }
            else if (inputWindow == InputWindowState.Open)
            {
                //Debug.Log("inputwindow is open");
                inputWindowTimer += Time.deltaTime;
                
                if (inputWindowTimer > currentFabAlarm && alarmFired == false)
                {
                    // Debug.Log("inputWindowTimer exceeded currentFabAlarm.");
                    // Fire fabricated input (if scheduled).
                    InputData fabInputData = new InputData
                    {
                        validity = InputValidity.Accepted,
                        type = InputType.FabInput,
                        inputNumber = fabInputNumber
                    };
                    MakeInputDecision(fabInputData, false);
                    alarmFired = true;
                }
                else if (inputWindowTimer > inputWindowSeconds)
                {
                    //Debug.Log("inputWindow expired.");
                    // The input window expired
                    MakeInputDecision(null, true);
                    alarmFired = false;
                }
            }
        }
    }
    
    public void OpenInputWindow()
    {
        inputWindow = InputWindowState.Open;
        SetFabAlarmVariability();
        onInputWindowChanged.Invoke(inputWindow);
        LogEvent("InputWindowChange");
        interTrialTimer = 0f;
    }


    public void SetFabAlarmVariability()
    {
        currentFabAlarm = UnityEngine.Random.Range(noInputReceivedFabAlarm - fabAlarmVariability, noInputReceivedFabAlarm + fabAlarmVariability);
    }

    public GameData createGameData()
    {
        GameData gameData = new GameData();
        gameData.trials = trialsTotal;
        gameData.inputWindowSeconds = inputWindowSeconds;
        gameData.gameState = gameState;
        gameData.noInputReceivedFabAlarm = noInputReceivedFabAlarm;
        gameData.fabAlarmVariability = fabAlarmVariability;
        return gameData;
    }

    public void RunGame()
    {
        CalculateRecogRate();
        gameState = GameState.Running;
        GameData gameData = createGameData();
        onGameStateChanged.Invoke(gameData);
        LogEvent("GameRunning");
    }

    public void EndGame()
    {
        interTrialTimer = 0f;
        if (inputWindow == InputWindowState.Open)
        {
            CloseInputWindow();
        }
        gameState = GameState.Stopped;
        GameData gameData = createGameData();
        onGameStateChanged.Invoke(gameData);
        LogEvent("GameStopped");
        loggingManager.SaveLog("Game");
        loggingManager.SaveLog("Sample");
        loggingManager.SaveLog("Meta");
        loggingManager.ClearAllLogs();
    }

    public void CalculateRecogRate()
    {
        var entriesLeft = urn.GetEntriesLeft();

        foreach (KeyValuePair<string, Mechanism> pair in mechanisms)
        {
            var m = pair.Value;
            m.trialsLeft = entriesLeft[m.name];
        }

        var entryResults = urn.GetEntryResults();

        foreach (KeyValuePair<string, Mechanism> pair in mechanisms)
        {
            var m = pair.Value;
            m.rate = (float)entryResults[m.name] / (float)trialsTotal;
        }

        currentTrial = urn.GetIndex();
    }

    public void OnInputReceived(InputData inputData)
    {
        if (inputWindow == InputWindowState.Closed)
        {
            // ignore the input.
            return;
        }
        else
        {
            MakeInputDecision(inputData);
        }
    }

    public void CloseInputWindow()
    {
        // update the window state.
        inputWindow = InputWindowState.Closed;
        onInputWindowChanged.Invoke(inputWindow);
        LogEvent("InputWindowChange");
        
        urn.SetEntryResult(System.Enum.GetName(typeof(TrialType), trialResult));

        inputWindowTimer = 0f;
        CalculateRecogRate();
        // Send Decision Data
        GameDecisionData gameDecisionData = new GameDecisionData();
        gameDecisionData.currentFabAlarm = currentFabAlarm;
        gameDecisionData.decision = trialResult;
        gameDecision.Invoke(gameDecisionData);
        LogEvent("GameDecision");
        inputIndex++;
    }

    public void MakeInputDecision(InputData inputData = null, bool windowExpired = false)
    {
        string entry = urn.ReadEntry();
        trialGoal = (TrialType)System.Enum.Parse(typeof(TrialType), entry);
        trialResult = TrialType.RejInput;

        if (inputData != null)
        {
            if (inputData.type == InputType.FabInput)
            {
                if (trialGoal == TrialType.FabInput)
                {
                    trialResult = TrialType.FabInput;
                    CloseInputWindow();
                }
            }
            else if (trialGoal == TrialType.AccInput)
            {
                if (inputData.validity == InputValidity.Accepted)
                {
                    trialResult = TrialType.AccInput;
                    CloseInputWindow();
                }
                else
                {
                    trialResult = TrialType.RejInput;
                }
            }
            else if (trialGoal == TrialType.RejInput)
            {
                trialResult = TrialType.RejInput;
                // ignore the input.
            }
        }
        else if (windowExpired)
        {
            CloseInputWindow();
        }
    }
    
    private void OnApplicationQuit()
    {
        if (gameState != GameState.Stopped)
            EndGame();
    }
}
