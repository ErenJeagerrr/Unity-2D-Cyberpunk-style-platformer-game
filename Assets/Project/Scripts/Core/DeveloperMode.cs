using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Developer Mode System
/// Hotkey: Ctrl + ~ to toggle
/// </summary>
public class DeveloperMode : SingletonBase<DeveloperMode>
{
    [Header("Developer Mode Settings")]
    public bool IsDevMode = false;
    public KeyCode toggleKey = KeyCode.BackQuote; // Changed to ~ key (BackQuote)
    public bool requireCtrl = true;
    public bool requireAlt = false; // Changed: only Ctrl needed now

    [Header("Feature Toggles")]
    public bool godMode = false;
    public bool noClipMode = false;
    public bool oneHitKill = false;

    [Header("Flight Settings")]
    public float flySpeed = 10f;
    public float fastFlyMultiplier = 2f;

    [Header("UI Settings")]
    public GameObject devModeUI;
    private Text statusText;
    private Player player;
    private Rigidbody2D playerRig;
    private float originalGravity;
    private bool isUICollapsed = false; // Track UI collapse state
    private Button collapseButton;
    private Text collapseButtonText;
    private GameObject contentPanel; // Panel containing the status text

    // Dragging variables
    private bool isDragging = false;
    private Vector2 dragOffset;
    private RectTransform panelRect;

    // Transparency control
    private Slider transparencySlider;
    private Image panelBackground;
    private float currentAlpha = 0.85f;

    // UI scale control
    private Slider scaleSlider;
    private Text scaleLabelText;
    private float currentUIScale = 1.0f;
    private Vector2 baseUISize = new Vector2(480, 680); // Base size (increased for scale slider)

    // UI element references for scaling
    private Text titleBarText;
    private Text dragHintText;
    private Text hotkeysSectionTitle;
    private Text playerStatusTitle;

    // UI update timer
    private float uiUpdateTimer = 0f;
    private const float UI_UPDATE_INTERVAL = 0.2f; // Update UI every 0.2 seconds

    // Track if we just disabled flight and need to monitor falling
    private bool isMonitoringFalling = false;
    private float fallingMonitorTimer = 0f;
    private const float FALLING_MONITOR_DURATION = 1f;

    public override void Init()
    {
        base.Init();
        CreateDevModeUI();
    }

    private void Update()
    {
        CheckToggleDevMode();

        // Handle dragging
        HandleDragging();

        if (!IsDevMode) return;

        // Update UI periodically (avoid updating every frame)
        uiUpdateTimer += Time.deltaTime;
        if (uiUpdateTimer >= UI_UPDATE_INTERVAL)
        {
            uiUpdateTimer = 0f;
            UpdateStatusUI();
        }

        CheckDevModeHotkeys();

        // Continuously apply noClipMode effects
        if (noClipMode)
        {
            HandleNoClipMovement();
        }
        // Monitor falling after disabling flight
        else if (isMonitoringFalling)
        {
            MonitorFalling();
        }
    }

    // Monitor player falling after disabling flight mode
    private void MonitorFalling()
    {
        fallingMonitorTimer += Time.deltaTime;

        if (player == null || playerRig == null)
        {
            isMonitoringFalling = false;
            return;
        }

        // Ensure we have valid gravity
        float targetGravity = originalGravity;
        if (targetGravity == 0) targetGravity = 5f;

        // Stop monitoring after duration or when landed
        if (fallingMonitorTimer >= FALLING_MONITOR_DURATION || player.IsOnGround)
        {
            isMonitoringFalling = false;
            fallingMonitorTimer = 0f;

            // Critical: Ensure gravity is restored and jump counts reset when landed
            if (player.IsOnGround)
            {
                playerRig.gravityScale = targetGravity;
                player.CurrentJumpCount = 0;
                player.CurrentDashJumpCount = 0;
            }
            return;
        }

        // Only apply corrections while in air
        if (!player.IsOnGround)
        {
            // Ensure gravity is active
            if (playerRig.gravityScale != targetGravity)
            {
                playerRig.gravityScale = targetGravity;
            }

            // If player is stuck (no vertical movement), give a push
            if (Mathf.Abs(playerRig.velocity.y) < 0.5f)
            {
                playerRig.velocity = new Vector2(playerRig.velocity.x, -5f);
            }
        }
    }

    private void CheckToggleDevMode()
    {
        bool ctrlPressed = !requireCtrl || (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
        bool altPressed = !requireAlt || (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt));

        if (ctrlPressed && altPressed && Input.GetKeyDown(toggleKey))
        {
            ToggleDevMode();
        }
    }

    private void CheckDevModeHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.F1)) ToggleGodMode();
        if (Input.GetKeyDown(KeyCode.F2)) ToggleNoClipMode();
        if (Input.GetKeyDown(KeyCode.F3)) ToggleOneHitKill();
        if (Input.GetKeyDown(KeyCode.F4)) FullHealPlayer();
        if (Input.GetKeyDown(KeyCode.F5)) UnlockAllEquipment();
        if (Input.GetKeyDown(KeyCode.F6)) AddCoins(1000);
        if (Input.GetKeyDown(KeyCode.F7)) AddExp(500);
        if (Input.GetKeyDown(KeyCode.F8)) KillAllEnemies();
        if (Input.GetKeyDown(KeyCode.F9)) TeleportToMouse();

        // Level selection (1-6)
        if (Input.GetKeyDown(KeyCode.Alpha1)) LoadLevel(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) LoadLevel(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) LoadLevel(3);
        if (Input.GetKeyDown(KeyCode.Alpha4)) LoadLevel(4);
        if (Input.GetKeyDown(KeyCode.Alpha5)) LoadLevel(5);
        if (Input.GetKeyDown(KeyCode.Alpha6)) LoadLevel(6);
        if (Input.GetKeyDown(KeyCode.Alpha0)) LoadTown();
    }

    public void ToggleDevMode()
    {
        IsDevMode = !IsDevMode;

        if (devModeUI != null)
        {
            devModeUI.SetActive(IsDevMode);
        }

        if (IsDevMode)
        {
            Debug.Log("<color=cyan>[Developer Mode] Enabled!</color>");
            InitializePlayer();
        }
        else
        {
            Debug.Log("<color=cyan>[Developer Mode] Disabled</color>");
            DisableAllDevFeatures();
        }

        UpdateStatusUI();
    }

    // Improved player initialization: Always re-acquire player instance
    private void InitializePlayer()
    {
        // Always try to re-acquire player instance (fixes issue after level transition)
        player = PlayerSystem.Instance?.player;

        if (player != null)
        {
            playerRig = player.GetComponent<Rigidbody2D>();
            if (playerRig != null)
            {
                // Critical fix: Only set originalGravity if it hasn't been set or if it's 0
                if (originalGravity == 0)
                {
                    originalGravity = playerRig.gravityScale;
                    // If still 0, use default value of 5
                    if (originalGravity == 0)
                    {
                        originalGravity = 5f;
                    }
                }

                // If noClipMode is already enabled, reapply effects
                if (noClipMode)
                {
                    ApplyNoClipMode();
                }
            }
        }
    }

    #region Feature Toggles

    public void ToggleGodMode()
    {
        godMode = !godMode;
        Debug.Log($"<color=yellow>God Mode: {(godMode ? "ON" : "OFF")}</color>");
        UpdateStatusUI();
    }

    public void ToggleNoClipMode()
    {
        InitializePlayer(); // Ensure player instance is up to date
        noClipMode = !noClipMode;

        if (noClipMode)
        {
            ApplyNoClipMode();
            Debug.Log($"<color=yellow>Flight Mode: ON - Use WASD to fly</color>");
        }
        else
        {
            RemoveNoClipMode();
        }

        UpdateStatusUI();
    }

    // Separate function to apply noClip effects
    private void ApplyNoClipMode()
    {
        if (playerRig == null) return;

        // Save original gravity before changing it
        if (playerRig.gravityScale != 0 && originalGravity == 0)
        {
            originalGravity = playerRig.gravityScale;
        }

        playerRig.gravityScale = 0;
        playerRig.velocity = Vector2.zero;

        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            playerCollider.isTrigger = true;
        }
    }

    // Separate function to remove noClip effects
    private void RemoveNoClipMode()
    {
        if (playerRig == null || player == null) return;

        // Start coroutine for proper cleanup
        player.StartCoroutine(DisableNoClipCoroutine());
    }

    // Coroutine to properly disable noClip mode
    private IEnumerator DisableNoClipCoroutine()
    {
        if (player == null || playerRig == null) yield break;

        // Ensure we have a valid gravity value
        float targetGravity = originalGravity;
        if (targetGravity == 0)
        {
            targetGravity = 5f; // Default gravity value
            originalGravity = targetGravity;
        }

        // Step 1: Restore collision
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            playerCollider.isTrigger = false;
        }

        // Step 2: Restore gravity
        playerRig.gravityScale = targetGravity;

        // Step 3: Clear velocity and set initial falling velocity (increased for faster fall)
        playerRig.velocity = new Vector2(0, -5f);

        // Step 4: Disable player controls temporarily
        player.IsDash = false;
        player.CurrentJumpCount = player.MaxJumpCount;
        player.CurrentDashJumpCount = player.MaxDashJumpCount;

        // Step 5: Force into JumpDownState
        if (player.state != null)
        {
            player.state.ChangeState(player.JumpDownState);
        }

        // Step 6: Wait and monitor falling
        float waitTime = 0f;
        bool hasLanded = false;

        while (waitTime < 3f && !hasLanded)
        {
            yield return new WaitForFixedUpdate();
            waitTime += Time.fixedDeltaTime;

            if (player == null || playerRig == null) yield break;

            // Ensure gravity is on
            if (playerRig.gravityScale != targetGravity)
            {
                playerRig.gravityScale = targetGravity;
            }

            // If velocity is too small and not on ground, give a stronger push
            if (Mathf.Abs(playerRig.velocity.y) < 1f && !player.IsOnGround)
            {
                playerRig.velocity = new Vector2(playerRig.velocity.x, -5f);
            }

            // Check if landed
            if (player.IsOnGround)
            {
                hasLanded = true;

                // Critical: Complete reset when landed
                isMonitoringFalling = false;
                player.CurrentJumpCount = 0;
                player.CurrentDashJumpCount = 0;
                playerRig.gravityScale = targetGravity;

                // Force to idle state
                if (player.state != null)
                {
                    player.state.ChangeState(player.IdleState);
                }

                yield break;
            }
        }

        // Step 7: If still in air after timeout, reset jump counts anyway
        if (!hasLanded && player != null)
        {
            player.CurrentJumpCount = 0;
            player.CurrentDashJumpCount = 0;
            playerRig.gravityScale = targetGravity;
        }

        isMonitoringFalling = false;
    }

    public void ToggleOneHitKill()
    {
        oneHitKill = !oneHitKill;

        if (oneHitKill)
        {
            // Enable one-hit kill handler
            EnableOneHitKill();
        }
        else
        {
            // Disable one-hit kill handler
            DisableOneHitKill();
        }

        Debug.Log($"<color=yellow>One Hit Kill: {(oneHitKill ? "ON" : "OFF")}</color>");
        UpdateStatusUI();
    }

    private void EnableOneHitKill()
    {
        // Add handler component if not exists
        if (GetComponent<OneHitKillHandler>() == null)
        {
            gameObject.AddComponent<OneHitKillHandler>();
        }
    }

    private void DisableOneHitKill()
    {
        // Remove handler component
        OneHitKillHandler handler = GetComponent<OneHitKillHandler>();
        if (handler != null)
        {
            Destroy(handler);
        }
    }

    #endregion

    #region Flight Control

    private void HandleNoClipMovement()
    {
        // Re-acquire player reference each time (prevents failure after level transition)
        if (player == null || playerRig == null)
        {
            InitializePlayer();
            if (player == null || playerRig == null) return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? flySpeed * fastFlyMultiplier : flySpeed;

        Vector2 movement = new Vector2(horizontal, vertical).normalized * currentSpeed;
        playerRig.velocity = movement;

        if (horizontal > 0)
            player.Flip(true);
        else if (horizontal < 0)
            player.Flip(false);
    }

    #endregion

    #region Helper Functions

    private void FullHealPlayer()
    {
        if (PlayerSystem.Instance != null)
        {
            PlayerSystem.Instance.CurrentHeathl = PlayerSystem.Instance.MaxHeathl;
            Debug.Log("<color=green>Player health restored!</color>");
            UpdateStatusUI();
        }
    }

    private void UnlockAllEquipment()
    {
        if (DataSystem.Instance != null && DataSystem.Instance.CurrentLoginData != null)
        {
            DataSystem.Instance.CurrentLoginData.EquipWeaponAtk = 50;
            DataSystem.Instance.CurrentLoginData.EquipArmorHP = 100;
            DataSystem.Instance.CurrentLoginData.EquipShoeSpd = 5;

            if (PlayerSystem.Instance != null)
            {
                PlayerSystem.Instance.ReCalculateData();
                PlayerSystem.Instance.CurrentHeathl = PlayerSystem.Instance.MaxHeathl;
            }

            DataSystem.Instance.Save();
            Debug.Log("<color=green>Top equipment unlocked! Attack+50, HP+100, Speed+5</color>");
            UpdateStatusUI();
        }
    }

    private void AddCoins(int amount)
    {
        if (PlayerSystem.Instance != null)
        {
            PlayerSystem.Instance.CoinCount += amount;
            Debug.Log($"<color=green>+{amount} coins! Current: {PlayerSystem.Instance.CoinCount}</color>");
            UpdateStatusUI();
        }
    }

    private void AddExp(int amount)
    {
        if (PlayerSystem.Instance != null)
        {
            PlayerSystem.Instance.CurrentEXP += amount;
            Debug.Log($"<color=green>+{amount} EXP!</color>");
            UpdateStatusUI();
        }
    }

    private void KillAllEnemies()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (var enemy in enemies)
        {
            enemy.CurrentHeathl = 0;
            enemy.Die();
        }
        Debug.Log($"<color=green>Killed {enemies.Length} enemies!</color>");
    }

    private void TeleportToMouse()
    {
        InitializePlayer();
        if (player != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            player.transform.position = mousePos;
        }
    }

    private void LoadLevel(int level)
    {
        if (LevelSystem.Instance != null)
        {
            LevelSystem.Instance.CurrentLevel = level;
            DataSystem.Instance.CurrentLoginData.CurrentLevel = level;
            DataSystem.Instance.Save();

            // Reinitialize player reference after scene load
            LevelSystem.Instance.LoadCurrentLevel();

            // Delay reapplying persistent effects
            StartCoroutine(ReapplyPersistentEffects());
        }
    }

    private void LoadTown()
    {
        if (LevelSystem.Instance != null)
        {
            // Reinitialize player reference after scene load
            LevelSystem.Instance.LoadTown();

            // Delay reapplying persistent effects
            StartCoroutine(ReapplyPersistentEffects());
        }
    }

    // Critical fix: Reapply persistent effects after scene load
    private IEnumerator ReapplyPersistentEffects()
    {
        // Wait for scene load and player creation
        yield return new WaitForSeconds(0.5f);

        // Reinitialize player reference
        InitializePlayer();

        // If noClipMode is enabled, reapply effects
        if (noClipMode && player != null)
        {
            ApplyNoClipMode();
        }

        // If oneHitKill is enabled, reapply handler
        if (oneHitKill)
        {
            EnableOneHitKill();
        }

        // Force UI update
        UpdateStatusUI();
    }

    #endregion

    #region UI Management

    private void CreateDevModeUI()
    {
        GameObject canvas = new GameObject("DevModeCanvas");
        canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.GetComponent<Canvas>().sortingOrder = 999;
        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvas);

        devModeUI = new GameObject("DevModePanel");
        devModeUI.transform.SetParent(canvas.transform, false);

        panelRect = devModeUI.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(10, -10);
        panelRect.sizeDelta = new Vector2(480, 640); // Increased height to accommodate slider

        panelBackground = devModeUI.AddComponent<Image>();
        panelBackground.color = new Color(0, 0, 0, currentAlpha);

        // Create title bar (draggable area + collapse button)
        GameObject titleBar = new GameObject("TitleBar");
        titleBar.transform.SetParent(devModeUI.transform, false);

        RectTransform titleBarRect = titleBar.AddComponent<RectTransform>();
        titleBarRect.anchorMin = new Vector2(0, 1);
        titleBarRect.anchorMax = new Vector2(1, 1);
        titleBarRect.pivot = new Vector2(0.5f, 1);
        titleBarRect.anchoredPosition = new Vector2(0, 0);
        titleBarRect.sizeDelta = new Vector2(0, 50); // Height of title bar

        Image titleBarBg = titleBar.AddComponent<Image>();
        titleBarBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        // Add EventTrigger for dragging on title bar
        UnityEngine.EventSystems.EventTrigger eventTrigger = titleBar.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        // Drag start
        UnityEngine.EventSystems.EventTrigger.Entry dragBegin = new UnityEngine.EventSystems.EventTrigger.Entry();
        dragBegin.eventID = UnityEngine.EventSystems.EventTriggerType.BeginDrag;
        dragBegin.callback.AddListener((data) => { OnBeginDrag((UnityEngine.EventSystems.PointerEventData)data); });
        eventTrigger.triggers.Add(dragBegin);

        // Dragging
        UnityEngine.EventSystems.EventTrigger.Entry dragging = new UnityEngine.EventSystems.EventTrigger.Entry();
        dragging.eventID = UnityEngine.EventSystems.EventTriggerType.Drag;
        dragging.callback.AddListener((data) => { OnDrag((UnityEngine.EventSystems.PointerEventData)data); });
        eventTrigger.triggers.Add(dragging);

        // Drag end
        UnityEngine.EventSystems.EventTrigger.Entry dragEnd = new UnityEngine.EventSystems.EventTrigger.Entry();
        dragEnd.eventID = UnityEngine.EventSystems.EventTriggerType.EndDrag;
        dragEnd.callback.AddListener((data) => { OnEndDrag((UnityEngine.EventSystems.PointerEventData)data); });
        eventTrigger.triggers.Add(dragEnd);

        // Title text with drag hint
        GameObject titleTextObj = new GameObject("TitleText");
        titleTextObj.transform.SetParent(titleBar.transform, false);

        RectTransform titleTextRect = titleTextObj.AddComponent<RectTransform>();
        titleTextRect.anchorMin = Vector2.zero;
        titleTextRect.anchorMax = Vector2.one;
        titleTextRect.offsetMin = new Vector2(10, 0);
        titleTextRect.offsetMax = new Vector2(-60, 0); // Leave space for collapse button

        Text titleText = titleTextObj.AddComponent<Text>();
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 18;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = Color.cyan;
        titleText.alignment = TextAnchor.MiddleLeft;
        titleText.text = "⚡ Developer Mode";
        titleBarText = titleText; // Save reference

        // Drag hint text
        GameObject dragHintObj = new GameObject("DragHint");
        dragHintObj.transform.SetParent(titleBar.transform, false);

        RectTransform dragHintRect = dragHintObj.AddComponent<RectTransform>();
        dragHintRect.anchorMin = new Vector2(0, 0);
        dragHintRect.anchorMax = new Vector2(1, 0);
        dragHintRect.pivot = new Vector2(0.5f, 0);
        dragHintRect.anchoredPosition = new Vector2(0, 2);
        dragHintRect.sizeDelta = new Vector2(-70, 14);

        Text dragHint = dragHintObj.AddComponent<Text>();
        dragHint.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        dragHint.fontSize = 11;
        dragHint.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
        dragHint.alignment = TextAnchor.LowerLeft;
        dragHint.text = "🖱️ Drag to move";
        dragHintText = dragHint; // Save reference

        // Collapse button on the right side of title bar
        GameObject buttonObj = new GameObject("CollapseButton");
        buttonObj.transform.SetParent(titleBar.transform, false);

        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1, 0);
        buttonRect.anchorMax = new Vector2(1, 1);
        buttonRect.pivot = new Vector2(1, 0.5f);
        buttonRect.anchoredPosition = new Vector2(-5, 0);
        buttonRect.sizeDelta = new Vector2(50, 40);

        collapseButton = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.25f, 0.25f, 0.25f, 1f);

        // Button hover effect
        ColorBlock colors = collapseButton.colors;
        colors.normalColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        colors.highlightedColor = new Color(0.35f, 0.35f, 0.35f, 1f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        collapseButton.colors = colors;

        // Button icon (collapse/expand arrow)
        GameObject buttonTextObj = new GameObject("ButtonText");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);

        RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;

        collapseButtonText = buttonTextObj.AddComponent<Text>();
        collapseButtonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        collapseButtonText.fontSize = 24;
        collapseButtonText.fontStyle = FontStyle.Bold;
        collapseButtonText.color = Color.yellow;
        collapseButtonText.alignment = TextAnchor.MiddleCenter;
        collapseButtonText.text = "▼";

        // Collapse hint tooltip
        GameObject tooltipObj = new GameObject("CollapseHint");
        tooltipObj.transform.SetParent(buttonObj.transform, false);

        RectTransform tooltipRect = tooltipObj.AddComponent<RectTransform>();
        tooltipRect.anchorMin = new Vector2(0.5f, 0);
        tooltipRect.anchorMax = new Vector2(0.5f, 0);
        tooltipRect.pivot = new Vector2(0.5f, 1);
        tooltipRect.anchoredPosition = new Vector2(0, -2);
        tooltipRect.sizeDelta = new Vector2(60, 14);

        Text tooltip = tooltipObj.AddComponent<Text>();
        tooltip.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tooltip.fontSize = 10;
        tooltip.color = new Color(1f, 1f, 0.5f, 0.9f);
        tooltip.alignment = TextAnchor.UpperCenter;
        tooltip.text = "Click to fold";

        // Add click listener
        collapseButton.onClick.AddListener(ToggleUICollapse);

        // Create content panel (the part that can collapse)
        contentPanel = new GameObject("ContentPanel");
        contentPanel.transform.SetParent(devModeUI.transform, false);

        RectTransform contentRect = contentPanel.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = new Vector2(0, -50); // Below the title bar
        contentRect.offsetMin = new Vector2(0, 0);
        contentRect.offsetMax = new Vector2(0, -50);

        // Status text inside content panel
        GameObject textObj = new GameObject("StatusText");
        textObj.transform.SetParent(contentPanel.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(15, 90); // Leave space for scale + opacity sliders
        textRect.offsetMax = new Vector2(-15, -15);

        statusText = textObj.AddComponent<Text>();
        statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusText.fontSize = 18; // Increased from 14 to 18
        statusText.color = Color.white;
        statusText.alignment = TextAnchor.UpperLeft;
        statusText.lineSpacing = 1.1f; // Add line spacing for readability

        // Create UI Scale control at the bottom (above opacity)
        GameObject scalePanel = new GameObject("ScalePanel");
        scalePanel.transform.SetParent(contentPanel.transform, false);

        RectTransform scalePanelRect = scalePanel.AddComponent<RectTransform>();
        scalePanelRect.anchorMin = new Vector2(0, 0);
        scalePanelRect.anchorMax = new Vector2(1, 0);
        scalePanelRect.pivot = new Vector2(0.5f, 0);
        scalePanelRect.anchoredPosition = new Vector2(0, 40); // Above opacity slider
        scalePanelRect.sizeDelta = new Vector2(0, 40);

        // Scale label
        GameObject scaleLabelObj = new GameObject("ScaleLabel");
        scaleLabelObj.transform.SetParent(scalePanel.transform, false);

        RectTransform scaleLabelRect = scaleLabelObj.AddComponent<RectTransform>();
        scaleLabelRect.anchorMin = new Vector2(0, 0);
        scaleLabelRect.anchorMax = new Vector2(0, 1);
        scaleLabelRect.pivot = new Vector2(0, 0.5f);
        scaleLabelRect.anchoredPosition = new Vector2(15, 0);
        scaleLabelRect.sizeDelta = new Vector2(100, 0);

        scaleLabelText = scaleLabelObj.AddComponent<Text>();
        scaleLabelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        scaleLabelText.fontSize = 14;
        scaleLabelText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        scaleLabelText.alignment = TextAnchor.MiddleLeft;
        scaleLabelText.text = "📏 UI Scale:";

        // Scale slider
        GameObject scaleSliderObj = new GameObject("ScaleSlider");
        scaleSliderObj.transform.SetParent(scalePanel.transform, false);

        RectTransform scaleSliderRect = scaleSliderObj.AddComponent<RectTransform>();
        scaleSliderRect.anchorMin = new Vector2(0, 0.5f);
        scaleSliderRect.anchorMax = new Vector2(1, 0.5f);
        scaleSliderRect.pivot = new Vector2(0.5f, 0.5f);
        scaleSliderRect.anchoredPosition = new Vector2(0, 0);
        scaleSliderRect.sizeDelta = new Vector2(-130, 20);
        scaleSliderRect.offsetMin = new Vector2(115, -10);
        scaleSliderRect.offsetMax = new Vector2(-15, 10);

        scaleSlider = scaleSliderObj.AddComponent<Slider>();
        scaleSlider.minValue = 0.5f;
        scaleSlider.maxValue = 2.0f;
        scaleSlider.value = currentUIScale;
        scaleSlider.onValueChanged.AddListener(OnScaleChanged);

        // Scale slider background
        GameObject scaleBg = new GameObject("Background");
        scaleBg.transform.SetParent(scaleSliderObj.transform, false);

        RectTransform scaleBgRect = scaleBg.AddComponent<RectTransform>();
        scaleBgRect.anchorMin = Vector2.zero;
        scaleBgRect.anchorMax = Vector2.one;
        scaleBgRect.sizeDelta = Vector2.zero;

        Image scaleBgImage = scaleBg.AddComponent<Image>();
        scaleBgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // Scale slider fill area
        GameObject scaleFillArea = new GameObject("Fill Area");
        scaleFillArea.transform.SetParent(scaleSliderObj.transform, false);

        RectTransform scaleFillAreaRect = scaleFillArea.AddComponent<RectTransform>();
        scaleFillAreaRect.anchorMin = Vector2.zero;
        scaleFillAreaRect.anchorMax = Vector2.one;
        scaleFillAreaRect.sizeDelta = new Vector2(-10, -10);

        GameObject scaleFill = new GameObject("Fill");
        scaleFill.transform.SetParent(scaleFillArea.transform, false);

        RectTransform scaleFillRect = scaleFill.AddComponent<RectTransform>();
        scaleFillRect.anchorMin = Vector2.zero;
        scaleFillRect.anchorMax = Vector2.one;
        scaleFillRect.sizeDelta = Vector2.zero;

        Image scaleFillImage = scaleFill.AddComponent<Image>();
        scaleFillImage.color = new Color(1f, 0.7f, 0.3f, 1f); // Orange

        scaleSlider.fillRect = scaleFillRect;

        // Scale slider handle
        GameObject scaleHandleArea = new GameObject("Handle Slide Area");
        scaleHandleArea.transform.SetParent(scaleSliderObj.transform, false);

        RectTransform scaleHandleAreaRect = scaleHandleArea.AddComponent<RectTransform>();
        scaleHandleAreaRect.anchorMin = Vector2.zero;
        scaleHandleAreaRect.anchorMax = Vector2.one;
        scaleHandleAreaRect.sizeDelta = new Vector2(-10, 0);

        GameObject scaleHandle = new GameObject("Handle");
        scaleHandle.transform.SetParent(scaleHandleArea.transform, false);

        RectTransform scaleHandleRect = scaleHandle.AddComponent<RectTransform>();
        scaleHandleRect.anchorMin = new Vector2(0, 0.5f);
        scaleHandleRect.anchorMax = new Vector2(0, 0.5f);
        scaleHandleRect.pivot = new Vector2(0.5f, 0.5f);
        scaleHandleRect.sizeDelta = new Vector2(16, 16);

        Image scaleHandleImage = scaleHandle.AddComponent<Image>();
        scaleHandleImage.color = Color.white;

        scaleSlider.handleRect = scaleHandleRect;
        scaleSlider.targetGraphic = scaleHandleImage;

        // Create transparency control at the bottom
        GameObject sliderPanel = new GameObject("TransparencyPanel");
        sliderPanel.transform.SetParent(contentPanel.transform, false);

        RectTransform sliderPanelRect = sliderPanel.AddComponent<RectTransform>();
        sliderPanelRect.anchorMin = new Vector2(0, 0);
        sliderPanelRect.anchorMax = new Vector2(1, 0);
        sliderPanelRect.pivot = new Vector2(0.5f, 0);
        sliderPanelRect.anchoredPosition = Vector2.zero;
        sliderPanelRect.sizeDelta = new Vector2(0, 40);

        // Slider label
        GameObject labelObj = new GameObject("SliderLabel");
        labelObj.transform.SetParent(sliderPanel.transform, false);

        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(0, 1);
        labelRect.pivot = new Vector2(0, 0.5f);
        labelRect.anchoredPosition = new Vector2(15, 0);
        labelRect.sizeDelta = new Vector2(100, 0);

        Text label = labelObj.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 14;
        label.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        label.alignment = TextAnchor.MiddleLeft;
        label.text = "🎨 Opacity:";

        // Slider
        GameObject sliderObj = new GameObject("OpacitySlider");
        sliderObj.transform.SetParent(sliderPanel.transform, false);

        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 0.5f);
        sliderRect.anchorMax = new Vector2(1, 0.5f);
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(0, 0);
        sliderRect.sizeDelta = new Vector2(-130, 20);
        sliderRect.offsetMin = new Vector2(115, -10);
        sliderRect.offsetMax = new Vector2(-15, 10);

        transparencySlider = sliderObj.AddComponent<Slider>();
        transparencySlider.minValue = 0.3f;
        transparencySlider.maxValue = 1.0f;
        transparencySlider.value = currentAlpha;
        transparencySlider.onValueChanged.AddListener(OnTransparencyChanged);

        // Slider background
        GameObject sliderBg = new GameObject("Background");
        sliderBg.transform.SetParent(sliderObj.transform, false);

        RectTransform sliderBgRect = sliderBg.AddComponent<RectTransform>();
        sliderBgRect.anchorMin = Vector2.zero;
        sliderBgRect.anchorMax = Vector2.one;
        sliderBgRect.sizeDelta = Vector2.zero;

        Image sliderBgImage = sliderBg.AddComponent<Image>();
        sliderBgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // Slider fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);

        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-10, -10);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);

        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.3f, 0.7f, 1f, 1f); // Light blue

        transparencySlider.fillRect = fillRect;

        // Slider handle
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);

        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.sizeDelta = new Vector2(-10, 0);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);

        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0, 0.5f);
        handleRect.anchorMax = new Vector2(0, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(16, 16);

        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;

        transparencySlider.handleRect = handleRect;
        transparencySlider.targetGraphic = handleImage;

        devModeUI.SetActive(false);
    }

    // Transparency change handler
    private void OnTransparencyChanged(float value)
    {
        currentAlpha = value;
        if (panelBackground != null)
        {
            Color color = panelBackground.color;
            color.a = currentAlpha;
            panelBackground.color = color;
        }
    }

    // Scale change handler
    private void OnScaleChanged(float value)
    {
        currentUIScale = value;
        ApplyUIScale();
    }

    private void ApplyUIScale()
    {
        if (panelRect == null) return;

        Vector2 newSize = baseUISize * currentUIScale;

        if (!isUICollapsed)
        {
            panelRect.sizeDelta = newSize;
        }

        // Scale all text elements
        if (statusText != null)
        {
            statusText.fontSize = Mathf.RoundToInt(18 * currentUIScale);
        }

        if (titleBarText != null)
        {
            titleBarText.fontSize = Mathf.RoundToInt(18 * currentUIScale);
        }

        if (dragHintText != null)
        {
            dragHintText.fontSize = Mathf.RoundToInt(11 * currentUIScale);
        }

        if (collapseButtonText != null)
        {
            collapseButtonText.fontSize = Mathf.RoundToInt(24 * currentUIScale);
        }

        if (scaleLabelText != null)
        {
            scaleLabelText.fontSize = Mathf.RoundToInt(14 * currentUIScale);
        }
    }

    private void ToggleUICollapse()
    {
        isUICollapsed = !isUICollapsed;

        if (isUICollapsed)
        {
            // Collapsed state - only show title bar, arrow points up
            contentPanel.SetActive(false);
            collapseButtonText.text = "▲";
            panelRect.sizeDelta = new Vector2(baseUISize.x * currentUIScale, 50 * currentUIScale);
        }
        else
        {
            // Expanded state - show everything, arrow points down
            contentPanel.SetActive(true);
            collapseButtonText.text = "▼";
            panelRect.sizeDelta = baseUISize * currentUIScale; // Use current scale
        }
    }

    // Dragging functions
    private void OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        isDragging = true;
        // Calculate offset between mouse position and panel position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panelRect.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );
        dragOffset = panelRect.anchoredPosition - localPoint;
    }

    private void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panelRect.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint))
        {
            panelRect.anchoredPosition = localPoint + dragOffset;
        }
    }

    private void OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        isDragging = false;
    }

    private void HandleDragging()
    {
        // This function is called in Update but can be used for additional drag logic if needed
        // Currently, dragging is handled by EventTrigger callbacks
    }

    private void UpdateStatusUI()
    {
        if (statusText == null) return;

        // Re-acquire player reference each UI update
        InitializePlayer();

        string status = "";
        status += "<size=20><color=yellow>━━━ Hotkeys ━━━</color></size>\n";
        status += "<b>F1:</b> God Mode " + (godMode ? "<color=lime>[ON]</color>" : "<color=red>[OFF]</color>") + "\n";
        status += "<b>F2:</b> Fly Mode " + (noClipMode ? "<color=lime>[ON]</color>" : "<color=red>[OFF]</color>") + "\n";
        status += "<b>F3:</b> One Hit Kill " + (oneHitKill ? "<color=lime>[ON]</color>" : "<color=red>[OFF]</color>") + "\n";
        status += "<b>F4:</b> Full Heal  |  <b>F5:</b> Unlock Equip\n";
        status += "<b>F6:</b> +1000 Coins  |  <b>F7:</b> +500 EXP\n";
        status += "<b>F8:</b> Kill All  |  <b>F9:</b> Teleport\n";
        status += "<color=orange><b>1-6:</b> Load Level  |  <b>0:</b> Town</color>\n\n";

        // Read player data in real-time
        if (PlayerSystem.Instance != null)
        {
            status += "<size=20><color=yellow>━━━ Player Status ━━━</color></size>\n";
            status += $"<b>Health:</b> {(int)PlayerSystem.Instance.CurrentHeathl} / {(int)PlayerSystem.Instance.MaxHeathl}\n";
            status += $"<b>Level:</b> {PlayerSystem.Instance.Level} / 10\n";
            status += $"<b>EXP:</b> {PlayerSystem.Instance.CurrentEXP}\n";
            status += $"<b>Coins:</b> {PlayerSystem.Instance.CoinCount}\n";
            status += $"<b>Attack:</b> {PlayerSystem.Instance.Attack}\n";
            status += $"<b>Speed:</b> {PlayerSystem.Instance.MoveSpeed:F1}\n";

            if (LevelSystem.Instance != null)
            {
                status += $"<color=orange><b>Current Stage:</b> {LevelSystem.Instance.CurrentLevel} / 6</color>\n";
            }
        }
        else
        {
            status += "<color=red><b>⚠ Player not loaded</b></color>\n";
        }

        statusText.text = status;
    }

    #endregion


    private void DisableAllDevFeatures()
    {
        if (noClipMode) ToggleNoClipMode();
        godMode = false;
        oneHitKill = false;
    }

    public bool ShouldBlockDamage()
    {
        return IsDevMode && godMode;
    }

    public float GetDamageMultiplier()
    {
        return (IsDevMode && oneHitKill) ? 9999f : 1f;
    }
}

// One-hit kill handler - monitors all enemies and kills them when damaged
public class OneHitKillHandler : MonoBehaviour
{
    private Dictionary<Enemy, float> enemyHealthCache = new Dictionary<Enemy, float>();

    private void Update()
    {
        // Find all enemies in scene
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || enemy.IsDie) continue;

            // Track enemy health
            if (!enemyHealthCache.ContainsKey(enemy))
            {
                enemyHealthCache[enemy] = enemy.CurrentHeathl;
            }
            else
            {
                // If health decreased, enemy was damaged - kill instantly
                if (enemy.CurrentHeathl < enemyHealthCache[enemy])
                {
                    enemy.CurrentHeathl = 0;
                    enemy.Die();
                }

                // Update cached health
                enemyHealthCache[enemy] = enemy.CurrentHeathl;
            }
        }

        // Clean up dead enemies from cache
        List<Enemy> toRemove = new List<Enemy>();
        foreach (var kvp in enemyHealthCache)
        {
            if (kvp.Key == null || kvp.Key.IsDie)
            {
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var enemy in toRemove)
        {
            enemyHealthCache.Remove(enemy);
        }
    }

    private void OnDestroy()
    {
        enemyHealthCache.Clear();
    }
}