using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // <-- Add this line

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Progression")]
    public int coinsCollected = 0;       // persistent coin count across scenes
    public int coinsToAdvance = 3;       // coins required to go to next level
    private bool isTransitioning = false; // prevent multiple loads

    [Header("Player Stats")]
    public int playerMaxHealth = 100;
    [HideInInspector] public int playerHealth;

    [Header("Gameplay")]
    public int score = 0;

    [Header("Scene / Prefabs")]
    public GameObject playerPrefab;          // assign in inspector
    public string spawnTag = "Respawn";      // tag for spawn point in each scene

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            playerHealth = playerMaxHealth;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // --- Score ---
    public void AddScore(int value)
    {
        score += value;
        Debug.Log("Score = " + score);
    }

    // call this from collectible (or call from AddScore if coinValue==1)
    public void AddCoin(int amount = 1)
    {
        coinsCollected += amount;
        Debug.Log($"Coins collected: {coinsCollected}/{coinsToAdvance}");

        // optional: also increase score if you want
        // AddScore(amount * coinScoreValue);

        // check threshold and change level once
        if (!isTransitioning && coinsCollected >= coinsToAdvance)
        {
            StartCoroutine(AdvanceToNextLevelRoutine());
        }
    }

    private IEnumerator AdvanceToNextLevelRoutine()
    {
        isTransitioning = true;
        Debug.Log("Reached required coins. Loading next level...");
        // small delay so player sees animation/sound or coin disappears before load
        yield return new WaitForSeconds(0.5f);

        // Load next scene without resetting score/health
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(next);
        }
        else
        {
            Debug.Log("No more levels in build settings. Final score: " + score);
        }
        // do NOT reset coinsCollected or health here unless you want to
    }

    // --- Health ---
    public void TakeDamage(int dmg)
    {
        playerHealth -= dmg;
        playerHealth = Mathf.Clamp(playerHealth, 0, playerMaxHealth);
        Debug.Log($"Player health = {playerHealth}");
        if (playerHealth <= 0) OnPlayerDead();
    }


    // --- Player Death ---
    private void OnPlayerDead()
    {
        // Example: reload current level on death
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // --- Level Transition ---
    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
        {
            // ✅ DO NOT reset health or score here
            SceneManager.LoadScene(next);
        }
        else
        {
            Debug.Log("No more levels. Final score: " + score + " | Health: " + playerHealth);
            // Option: SceneManager.LoadScene("WinScene");
        }
    }

    // Called by Unity after a scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;

        // Find spawn
        GameObject spawn = GameObject.FindGameObjectWithTag(spawnTag);

        // Find existing Player in the scene
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");

        if (existingPlayer == null)
        {
            if (playerPrefab != null && spawn != null)
            {
                Instantiate(playerPrefab, spawn.transform.position, spawn.transform.rotation);
            }
            else
            {
                Debug.LogWarning("GameManager: Missing playerPrefab or spawn point in scene: " + scene.name);
            }
        }
        else
        {
            // Move existing player to spawn
            if (spawn != null)
            {
                var cc = existingPlayer.GetComponent<CharacterController>();
                var rb = existingPlayer.GetComponent<Rigidbody>();
                if (cc != null)
                {
                    cc.enabled = false;
                    existingPlayer.transform.position = spawn.transform.position;
                    existingPlayer.transform.rotation = spawn.transform.rotation;
                    cc.enabled = true;
                }
                else if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    existingPlayer.transform.position = spawn.transform.position;
                    existingPlayer.transform.rotation = spawn.transform.rotation;
                }
                else
                {
                    existingPlayer.transform.position = spawn.transform.position;
                    existingPlayer.transform.rotation = spawn.transform.rotation;
                }
            }
        }

    }

    // --- Full Restart ---
    public void RestartGameFull()
    {
        Time.timeScale = 1f;
        score = 0;
        playerHealth = playerMaxHealth;
        SceneManager.LoadScene(0);
    }
}