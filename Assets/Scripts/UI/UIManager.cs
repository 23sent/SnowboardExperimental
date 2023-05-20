using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cinemachine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("MenuPanel")]
    public GameObject MenuPanel;
    public Camera menuCamera;
    public Button pregameButton;
    public Button menuSettingsButton;
    public Button exitButton;
    public GameObject[] playerList;
    public GameObject[] boardList;
    public Transform spawnpointUIPlayer;
    public Transform spawnpointUISnowboard;
    private GameObject currentPlayer;
    private GameObject currentBoard;
    private int currentPlayerIndex = 0;
    private int currentBoardIndex = 0;
    
    [Space (5)]

    [Header("PreGamePanel")]
    public GameObject PreGamePanel;
    public Button playButton;
    public Button backButtonPreGame;
    public Button[] gameModes;
    private Button selectedMode;

    [Space(5)]

    [Header("Settings Panel")]
    public GameObject SettingsPanel;
    public Slider sliderMusic;
    public Slider sliderSound;
    public Button backButtonSettings;

    [Space(5)]

    [Header("Game Panel")]
    public bool isGameStart;
    public GameObject GamePanel;
    public Button pauseButton;
    public int totalScore = 0;
    public TextMeshProUGUI scoreText;
    public Transform spawnPoint;
    public GameObject mainObjectPrefab;
    public GameObject mainObject;
    public Camera gameCamera;
    public CinemachineVirtualCamera cmCamera;  

    [Space(5)]

    [Header("Pause Panel")]
    public GameObject PausePanel;
    public Button continueButton;
    public Button menuButton;
    public Button pauseSettingsButton;


    string lastPanel;
    string currentPanel;

    void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        totalScore = 0;
        SetActivePanel(MenuPanel.name);
        playButton.interactable = false;
        InstantiateCharacter();
        InstantiateSnowboard();
        pregameButton.onClick.AddListener(OnClickPregameButton);
        menuSettingsButton.onClick.AddListener(OnClickSettingsButton);
        pauseSettingsButton.onClick.AddListener(OnClickSettingsButton);
        exitButton.onClick.AddListener(OnClickExitButton);
        playButton.onClick.AddListener(OnClickPlayButton);
        menuButton.onClick.AddListener(OnClickMenuButton);
        pauseButton.onClick.AddListener(OnClickPauseButton);
        continueButton.onClick.AddListener(OnClickContinueButton);
        backButtonPreGame.onClick.AddListener(OnClickBackButton);
        backButtonSettings.onClick.AddListener(OnClickBackButton);
    }

    
    public void SetActivePanel(string activePanel)
    {
        lastPanel = currentPanel;
        MenuPanel.SetActive(activePanel.Equals(MenuPanel.name));
        PreGamePanel.SetActive(activePanel.Equals(PreGamePanel.name));
        SettingsPanel.SetActive(activePanel.Equals(SettingsPanel.name));
        GamePanel.SetActive(activePanel.Equals(GamePanel.name));
        PausePanel.SetActive(activePanel.Equals(PausePanel.name));
        currentPanel = activePanel;
    }
    public void OnClickContinueButton()
    {
        Time.timeScale = 1;
        SetActivePanel(GamePanel.name);
    }
    
    public void OnClickBackButton()
    {
        if (lastPanel == "MenuUI")
        {
            currentPlayer.SetActive(true);
        }
        SetActivePanel(lastPanel);
    }
    public void OnClickPauseButton()
    {
        SetActivePanel(PausePanel.name);
        Time.timeScale = 0;
    }

    public void OnClickPlayButton()
    {
        isGameStart = true;
        SetActivePanel(GamePanel.name);
        StarGameSetups();

    }

    private void StarGameSetups()
    {
        // First activate selected player and board.
        currentPlayer.SetActive(isGameStart);
        currentBoard.SetActive(isGameStart);

        // Then set variables
        DOTween.Kill(currentPlayer.transform);
        DOTween.Kill(currentBoard.transform);
        currentPlayer.transform.rotation = Quaternion.Euler(0, 0, 0);
        currentBoard.transform.rotation = Quaternion.Euler(0, 0, 0);
        mainObject =  Instantiate(mainObjectPrefab);
        currentBoard.transform.SetParent(mainObject.transform);
        currentBoard.transform.position = new Vector3(-0.007f, 0.47f, -0.116f);
        currentPlayer.transform.SetParent(currentBoard.transform);
        currentPlayer.transform.position = new Vector3(0.014f, -0.028f, -0.006f);
        mainObject.GetComponent<PlayerController>().playerModel = currentPlayer.transform;
        mainObject.GetComponent<PlayerController>().boardModel = currentBoard.transform;
        mainObject.GetComponent<PlayerController>().boardNormal = currentBoard.transform.GetChild(0);
        mainObject.GetComponent<PlayerController>().boardFrontHit1 = currentBoard.transform.GetChild(1);
        mainObject.GetComponent<PlayerController>().boardTailHit2 = currentBoard.transform.GetChild(2);
        mainObject.transform.position = spawnPoint.position;

        // Then deactivate menu camera and activate game camera.
        menuCamera.gameObject.SetActive(!isGameStart);
        gameCamera.gameObject.SetActive(isGameStart);
        cmCamera.gameObject.SetActive(isGameStart);
        cmCamera.Follow = currentPlayer.transform;
        cmCamera.LookAt = currentBoard.transform;
    }

    public void OnClickPregameButton()
    {
        currentPlayer.SetActive(false);
        currentBoard.SetActive(false);
        SetActivePanel(PreGamePanel.name);
    }

    public void OnClickSettingsButton()
    {
        currentPlayer.SetActive(false);
        currentBoard.SetActive(false);
        SetActivePanel(SettingsPanel.name);
    }
    public void OnClickMenuButton()
    {
        Time.timeScale = 1;
        currentPlayer.SetActive(true);
        currentBoard.SetActive(true);
        SetActivePanel(MenuPanel.name);
    }

    public void UpdateScore(int score)
    {
        totalScore += score;
        scoreText.text = "Score: " + totalScore.ToString(); ;
    }

    public void SetAllButtonsInteractable()
    {

        foreach (Button button in gameModes)
        {
            button.interactable = true;
        }
    }

    public void OnClickGameModeChanged(Button clickedButton)
    {
        if (selectedMode == clickedButton)
        {
            SetAllButtonsInteractable();
            selectedMode = null;
            playButton.interactable = false;
            return;
        }
        selectedMode = clickedButton;
        playButton.interactable = true;

        foreach (Button button in gameModes)
        {
            if (clickedButton != button) 
                button.interactable = false;
        }
    }

    public void InstantiateCharacter(int whichWay = 0)
    {
        Quaternion lastrotation = transform.rotation;
        if (currentPlayer != null)
        {
            lastrotation = currentPlayer.transform.rotation;
            DOTween.Kill(currentPlayer.transform);
            Destroy(currentPlayer);
        }

        currentPlayerIndex += whichWay;
        if (currentPlayerIndex >= playerList.Length)
        {
            currentPlayerIndex = 0;
        }
        else if (currentPlayerIndex < 0)
        {
            currentPlayerIndex = playerList.Length - 1;
        }

        currentPlayer = Instantiate(playerList[currentPlayerIndex], spawnpointUIPlayer.position, lastrotation);

        RotateObjects();

    }
    public void InstantiateSnowboard(int whichWay = 0)
    {
        Quaternion lastrotation = transform.rotation;
        if (currentBoard != null)
        {
            lastrotation = currentBoard.transform.rotation;
            DOTween.Kill(currentBoard.transform);
            Destroy(currentBoard);
        }

        currentBoardIndex += whichWay;
        if (currentBoardIndex >= playerList.Length)
        {
            currentBoardIndex = 0;
        }
        else if (currentBoardIndex < 0)
        {
            currentBoardIndex = boardList.Length - 1;
        }

        currentBoard = Instantiate(boardList[currentBoardIndex], spawnpointUISnowboard.position, lastrotation);

        RotateObjects();

    }

    void RotateObjects()
    {
        if (currentPlayer != null)
        {
            currentPlayer.transform.DORotate(new Vector3(0f, 360f, 0f), 10f, RotateMode.WorldAxisAdd)
            .SetEase(Ease.Linear)
            .SetLoops(-1);
        }

        if (currentBoard != null)
        {
            currentBoard.transform.DORotate(new Vector3(0f, 360f, 0f), 10f, RotateMode.WorldAxisAdd)
            .SetEase(Ease.Linear)
            .SetLoops(-1);
        }
    }

    public void OnClickExitButton()
    {
        Application.Quit();
    }
}