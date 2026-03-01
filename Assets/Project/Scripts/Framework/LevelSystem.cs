using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSystem : SingletonBase<LevelSystem>
{
    public int CurrentLevel { get; set; }
    private Door door; 
    public bool IsSpawning = false;
    private int _EnemyCount;

    public int EnemyCount
    {
        get => _EnemyCount;
        set
        {
            _EnemyCount = value;
            CheckIsHaveEnemy();
            UIService.Instance.UpdatePanel<BattlePanel>();
        }
    }

    public override void Init()
    {
        base.Init();
        CurrentLevel = 1; 
    }

    public void Load()
    {
        if (DataSystem.Instance.CurrentLoginData != null)
        {
            this.CurrentLevel = DataSystem.Instance.CurrentLoginData.CurrentLevel;
        }
    }

   private string GetLevelMusic(int level)
    {
        switch (level)
        {
            case 1:
                return "Level1";  
            case 2:
                return "Level2";  
            case 3:
                return "Boss1";  
            case 4:
                return "Level4";  
            case 5:
                return "Level5";  
            case 6:
                return "Boss2";   
            default:
                return "Level1";  
        }
    }

    public void LoadTown()
    {
        ResService.Instance.LoadScene("Town", () =>
        {
            InitLevel();
            AudioService.Instance.PlayBK("Town1");
        });
    }

    public void LoadCurrentLevel()
    {
        ResService.Instance.LoadScene("Level" + CurrentLevel, () =>
        {
            InitLevel();
            string musicName = GetLevelMusic(CurrentLevel);
            AudioService.Instance.PlayBK(musicName);
        });
    }

    private void InitLevel()
    {
        IsSpawning = false;

        // 1. Create Player
        Transform PlayerPos = GameObject.FindGameObjectWithTag("PlayerPos")?.transform;
        if (PlayerPos != null)
        {
            PlayerSystem.Instance.CreatPlayer(PlayerPos.position);
        }
        else
        {
            Debug.LogError("PlayerPos not found!");
        }

        // 2. Find Door
        door = GameObject.FindObjectOfType<Door>();

        string sceneName = ResService.Instance.CurrentSceneName;

        // --- FIX 1: TOWN LOGIC ---
        // If in Town, force hide immediately and STOP.
        if (sceneName.Contains("Town"))
        {
            if (door != null)
            {
                door.Hide();
            }
            else 
            {
                Debug.LogWarning("Warning: No Door found in Town scene.");
            }
            
            UIService.Instance.ShowPanel<BattlePanel>();

            // IMPORTANT: Return here so no other logic runs
            return;
        }

        // --- FIX 2: FORCE SYNC LEVEL ---
        // If we are in "Level4" scene, force logic to treat it as Level 4
        // preventing the "Level 1 Open Door" bug.
        if (sceneName.Contains("Level4")) CurrentLevel = 4;
        else if (sceneName.Contains("Level3")) CurrentLevel = 3;
        // You can add others if needed

        // --- FIX 3: LEVEL 4 LOCK ---
        // Explicitly lock door if we are in Level 4
        if (CurrentLevel == 4 || sceneName.Contains("Level4"))
        {
            IsSpawning = true; // Force Lock
            Debug.Log("Level 4 detected: Door Locked.");
        }
        // Also check for the manager as a backup
        else if (GameObject.FindObjectOfType<Level4WaveManager>() != null)
        {
            IsSpawning = true;
        }

        // 3. Handle Door Logic
        if (CurrentLevel == 1 && sceneName.Contains("Level1"))
        {
            // Only open immediately if it is ACTUALLY Level 1
            if(door != null) 
            {
                door.Show();
                door.Init(DoorType.Win); 
            }
        }
        else
        {
            // For Level 2, 3, 4, 5, 6 -> Hide door first
            door?.Hide();
            StartCoroutine(LateCheckForOpenDoor());
        }

        SortEnemyLay();
        UIService.Instance.ShowPanel<BattlePanel>();
    }

    IEnumerator LateCheckForOpenDoor()
    {
        // Wait longer to ensure Spawners have started
        yield return new WaitForSeconds(0.5f); 
        
        Enemy[] enemies = GameObject.FindObjectsOfType<Enemy>();
        _EnemyCount = enemies.Length;

        string sceneName = ResService.Instance.CurrentSceneName;

        // Only open if: Not Town, No Enemies, Not Spawning, Not Level 1
        if (!sceneName.Contains("Town") && 
            _EnemyCount == 0 && 
            IsSpawning == false && 
            CurrentLevel != 1)
        {
            if (CurrentLevel < 6)
            {
                OpenDoor(DoorType.Win);
            }
        }
    }

    public void CheckIsHaveEnemy()
    {
        if (ResService.Instance.CurrentSceneName.Contains("Town")) return;
        if (CurrentLevel == 1) return;

        Enemy[] enemies = GameObject.FindObjectsOfType<Enemy>();
        _EnemyCount = enemies.Length;

        if (_EnemyCount == 0 && IsSpawning == false)
        {
            WinLevel();
        }
    }

    public void OpenDoor(DoorType doorType)
    {
        if(door != null)
        {
            door.Show();
            door.Init(doorType);
            AudioService.Instance.PlayEffect("DoorOpen");
        }
    }

    public void StartSpawning()
    {
        IsSpawning = true;
    }

    public void FinishSpawning()
    {
        IsSpawning = false;
        CheckIsHaveEnemy(); 
    }

    public void WinLevel()
    {
        if (ResService.Instance.CurrentSceneName.Contains("Town")) return;
        
        if (door != null && door.gameObject.activeSelf && CurrentLevel != 6) return;

        PlayerSystem.Instance.CoinCount += 150;
        DataSystem.Instance.Save();

        if (CurrentLevel == 6)
        {
            UIService.Instance.ShowPanel<WinPanel>(3); 
        }
        else
        {
            OpenDoor(DoorType.Win);
        }
    }

    private void SortEnemyLay()
    {
        Enemy[] enemies = GameObject.FindObjectsOfType<Enemy>();
        int j = 0;
        foreach (var i in enemies)
        {
            if (i.EnemySpriteRenderer() != null)
            {
                i.EnemySpriteRenderer().sortingOrder = j;
                j++;
            }
        }
    }
}