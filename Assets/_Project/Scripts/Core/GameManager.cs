using ButchersGames;
using UnityEngine;
using TMPro;

public enum GameState
{
    WaitingToStart,
    Playing,
    Won,
    Lost
}

public class GameManager : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField] private PlayerMovement player;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private PlayerWealth playerWealth;

    [Header("UI")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private TMP_Text winResultLabel;

    public GameState State { get; private set; }
    public int FinishMultiplier { get; private set; } = 1;

    private void Awake()
    {
        player.SetRunning(false);
        State = GameState.WaitingToStart;

        startPanel.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    private void OnEnable()
    {
        levelManager.OnLevelLoaded += HandleLevelLoaded;
        playerWealth.Depleted += Lose;
    }

    private void Start()
    {
        levelManager.Init();
    }

    private void OnDisable()
    {
        levelManager.OnLevelLoaded -= HandleLevelLoaded;
        playerWealth.Depleted -= Lose;
    }

    private void HandleLevelLoaded(Level level)
    {
        player.SetRunning(false);

        playerWealth.ResetWealth();

        FinishMultiplier = 1;

        CharacterController controller = player.GetComponent<CharacterController>();

        controller.enabled = false;

        player.transform.SetPositionAndRotation(level.PlayerSpawnPoint.position, level.PlayerSpawnPoint.rotation);

        controller.enabled = true;

        foreach (GameOutcomeZone zone in level.GetComponentsInChildren<GameOutcomeZone>(true))
        {
            zone.Initialize(this);
        }

        foreach (FinishGate gate in level.GetComponentsInChildren<FinishGate>(true))
        {
            gate.Initialize(this);
        }

        State = GameState.WaitingToStart;

        startPanel.SetActive(true);
        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    public void StartGame()
    {
        if (State != GameState.WaitingToStart)
        {
            return;
        }

        State = GameState.Playing;
        startPanel.SetActive(false);
        player.SetRunning(true);
    }

    public void Win()
    {
        if (State != GameState.Playing)
        {
            return;
        }

        State = GameState.Won;
        player.SetRunning(false);

        if (winResultLabel != null)
        {
            winResultLabel.text = $"ЗАБЕГ ЗАВЕРШЁН\nМножитель ×{FinishMultiplier}";
        }

        winPanel.SetActive(true);
    }

    public void Lose()
    {
        if (State != GameState.Playing)
        {
            return;
        }

        State = GameState.Lost;
        player.SetRunning(false);
        losePanel.SetActive(true);
    }

    public void RestartLevel()
    {
        if (State != GameState.Won && State != GameState.Lost)
        {
            return;
        }

        levelManager.RestartLevel();
    }

    public void NextLevel()
    {
        if (State != GameState.Won)
        {
            return;
        }

        levelManager.NextLevel();
    }

    public void IncreaseFinishMultiplier(int multiplier)
    {
        if (State != GameState.Playing)
        {
            return;
        }

        FinishMultiplier = Mathf.Max(FinishMultiplier, multiplier);
    }
}