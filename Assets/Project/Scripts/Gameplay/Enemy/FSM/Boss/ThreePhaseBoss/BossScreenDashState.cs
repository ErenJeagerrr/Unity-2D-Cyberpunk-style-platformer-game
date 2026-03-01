using UnityEngine;
using System.Collections.Generic;

public class BossScreenDashState : EnemyStateBase
{
    private ThreePhaseBoss Boss;

    private struct DashTask
    {
        public Vector3 StartPos;
        public Vector3 Dir;
        public float Distance;
        public float WarningTime;
        public float Width;
        public float DelayNext;
    }

    private Queue<DashTask> dashQueue = new Queue<DashTask>();
    private class ActiveWarning
    {
        public DashTask Task;
        public GameObject WarningObj;
        public float Timer;
    }
    private List<ActiveWarning> activeWarnings = new List<ActiveWarning>();
    private int totalProjectilesAlive = 0;
    private float nextTaskTimer = 0;
    private float screenLeft, screenRight, screenTop, screenBottom;

    public BossScreenDashState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
        Boss = enemy as ThreePhaseBoss;
    }

    public override void Enter()
    {
        if (Boss == null || Boss.IsDie) return;

        base.Enter();
        Boss.SetVelocityX(0);

        dashQueue.Clear();
        activeWarnings.Clear();
        totalProjectilesAlive = 0;
        nextTaskTimer = 0;

        ToggleBossVisible(false);
        UpdateScreenBounds();
        GenerateCombo();
    }

    private void GenerateCombo()
    {
        float baseWidth = 4.0f;
        float baseWarning = Boss.ScreenDashWarningTime;
        float fastInterval = 0.2f;
        float superFastInterval = 0.15f;

        if (Boss.Phase == 1)
        {
            dashQueue.Enqueue(CreatePlayerThroughTask(baseWidth, baseWarning, 1.0f));
        }
        else if (Boss.Phase == 2)
        {
            dashQueue.Enqueue(CreateScreenCutTask(new Vector2(-1, 1), new Vector2(1, -1), baseWidth, baseWarning, fastInterval));
            dashQueue.Enqueue(CreateScreenCutTask(new Vector2(-1, -1), new Vector2(1, 1), baseWidth, baseWarning, fastInterval));
            dashQueue.Enqueue(CreatePlayerThroughTask(baseWidth, baseWarning, 1.0f));
        }
        else
        {
            float p3Warning = Mathf.Max(0.3f, baseWarning * 0.8f);
            dashQueue.Enqueue(CreateHorizontalTask(baseWidth, p3Warning, superFastInterval));
            dashQueue.Enqueue(CreateVerticalTask(baseWidth, p3Warning, superFastInterval));
            dashQueue.Enqueue(CreatePlayerThroughTask(baseWidth, p3Warning, superFastInterval));
            dashQueue.Enqueue(CreateScreenCutTask(new Vector2(-1, 1), new Vector2(1, -1), 6.0f, p3Warning, 1.0f));
        }
    }

    public override void FrameUpdate()
    {
        if (Boss == null || Boss.IsDie || Boss.gameObject == null) return;

        base.FrameUpdate();

        if (dashQueue.Count > 0)
        {
            nextTaskTimer -= Time.deltaTime;
            if (nextTaskTimer <= 0)
            {
                DashTask next = dashQueue.Dequeue();
                StartWarning(next);
                nextTaskTimer = next.DelayNext;
            }
        }

        for (int i = activeWarnings.Count - 1; i >= 0; i--)
        {
            var w = activeWarnings[i];
            w.Timer += Time.deltaTime;

            if (w.Timer >= w.Task.WarningTime)
            {
                FireProjectile(w.Task);
                if (w.WarningObj != null) GameObject.Destroy(w.WarningObj);
                activeWarnings.RemoveAt(i);
            }
        }
        if (dashQueue.Count == 0 && activeWarnings.Count == 0 && totalProjectilesAlive == 0)
        {
            FinishState();
        }
    }

    private void StartWarning(DashTask task)
    {
        if (Boss == null || Boss.IsDie) return;

        ActiveWarning w = new ActiveWarning();
        w.Task = task;
        w.Timer = 0;

        if (Boss.DashWarningPrefab != null)
        {
            Vector3 spawnPos = Boss.player != null ? Boss.player.position : Vector3.zero;
            w.WarningObj = GameObject.Instantiate(Boss.DashWarningPrefab, spawnPos, Quaternion.identity);

            w.WarningObj.transform.position = task.StartPos + task.Dir * (task.Distance / 2);

            float angle = Mathf.Atan2(task.Dir.y, task.Dir.x) * Mathf.Rad2Deg;
            w.WarningObj.transform.rotation = Quaternion.Euler(0, 0, angle);
            w.WarningObj.transform.localScale = new Vector3(task.Distance, task.Width, 1f);
        }

        activeWarnings.Add(w);
    }

    private void FireProjectile(DashTask task)
    {
        if (Boss == null || Boss.IsDie) return;

        if (Boss.DashProjectilePrefab == null) return;

        GameObject projObj = GameObject.Instantiate(Boss.DashProjectilePrefab, task.StartPos, Quaternion.identity);
        BossDashProjectile proj = projObj.GetComponent<BossDashProjectile>();

        totalProjectilesAlive++;

        proj.Init(task.Dir, Boss.ScreenDashSpeed, Boss.Data.Attack * 2, task.Distance, task.Width, OnOneProjectileFinished);

        if (AudioService.Instance != null) AudioService.Instance.PlayEffect("Dash_Screen");
        if (AttackEffect.Instance != null) AttackEffect.Instance.Shake();
    }

    private void OnOneProjectileFinished()
    {
        totalProjectilesAlive--;
        if (totalProjectilesAlive < 0) totalProjectilesAlive = 0;
    }

    private void FinishState()
    {
        if (Boss == null || Boss.gameObject == null || Boss.IsDie) return;

        Vector3 returnPos = CalculateSafeReturnPos();
        Boss.transform.position = returnPos;
        ToggleBossVisible(true);

        if (!Boss.IsDie)
        {
            state.ChangeState(Boss.BossIdleState);
        }
    }

    private DashTask CreatePlayerThroughTask(float width, float warningTime, float delayNext)
    {
        if (Boss.player == null) return new DashTask();

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        if (Mathf.Abs(randomDir.x) < 0.3f) randomDir.x = randomDir.x > 0 ? 0.5f : -0.5f;
        randomDir.Normalize();

        Vector3 center = Boss.player.position;
        float runUp = 25f;

        return new DashTask
        {
            StartPos = center - (Vector3)(randomDir * runUp),
            Dir = randomDir,
            Distance = runUp * 2,
            WarningTime = warningTime,
            Width = width,
            DelayNext = delayNext
        };
    }

    private DashTask CreateScreenCutTask(Vector2 startDir, Vector2 endDir, float width, float warningTime, float delayNext)
    {
        Vector3 p1 = new Vector3(
            startDir.x > 0 ? screenRight + 5 : screenLeft - 5,
            startDir.y > 0 ? screenTop + 5 : screenBottom - 5,
            0
        );
        Vector3 p2 = new Vector3(
            endDir.x > 0 ? screenRight + 5 : screenLeft - 5,
            endDir.y > 0 ? screenTop + 5 : screenBottom - 5,
            0
        );

        return new DashTask
        {
            StartPos = p1,
            Dir = (p2 - p1).normalized,
            Distance = Vector3.Distance(p1, p2),
            WarningTime = warningTime,
            Width = width,
            DelayNext = delayNext
        };
    }

    private DashTask CreateHorizontalTask(float width, float warningTime, float delayNext)
    {
        float playerY = Boss.player != null ? Boss.player.position.y : 0;
        bool fromLeft = Random.value > 0.5f;
        Vector3 start = new Vector3(fromLeft ? screenLeft - 15 : screenRight + 15, playerY, 0);

        return new DashTask
        {
            StartPos = start,
            Dir = fromLeft ? Vector3.right : Vector3.left,
            Distance = 50f,
            WarningTime = warningTime,
            Width = width,
            DelayNext = delayNext
        };
    }

    private DashTask CreateVerticalTask(float width, float warningTime, float delayNext)
    {
        float playerX = Boss.player != null ? Boss.player.position.x : 0;
        Vector3 start = new Vector3(playerX, screenTop + 15, 0);

        return new DashTask
        {
            StartPos = start,
            Dir = Vector3.down,
            Distance = 40f,
            WarningTime = warningTime,
            Width = width,
            DelayNext = delayNext
        };
    }

    private void UpdateScreenBounds()
    {
        if (Camera.main == null) return;
        Camera cam = Camera.main;
        float height = cam.orthographicSize * 2;
        float width = height * cam.aspect;
        Vector3 center = cam.transform.position;

        screenLeft = center.x - width / 2;
        screenRight = center.x + width / 2;
        screenTop = center.y + height / 2;
        screenBottom = center.y - height / 2;
    }

    private Vector3 CalculateSafeReturnPos()
    {
        if (Camera.main == null) return Vector3.zero;
        Camera cam = Camera.main;
        Vector3 returnPos = cam.transform.position;
        returnPos.z = 0;
        returnPos.y += 2.0f;

        RaycastHit2D hit = Physics2D.Raycast(returnPos, Vector2.down, 20f, LayerMask.GetMask("Ground"));
        if (hit.collider != null) returnPos.y = hit.point.y + 1.5f;
        else if (Boss.player != null) returnPos.y = Boss.player.position.y;

        return returnPos;
    }

    private void ToggleBossVisible(bool isVisible)
    {
        if (Boss == null || Boss.gameObject == null) return;

        var col = Boss.GetComponent<Collider2D>();
        if (col != null) col.enabled = isVisible;
        if (Boss.rig != null)
        {
            Boss.rig.simulated = isVisible;
            if (!isVisible) Boss.rig.velocity = Vector2.zero;
        }
        var sr = Boss.EnemySpriteRenderer();
        if (sr != null) sr.enabled = isVisible;
        Boss.IsImmunity = !isVisible;

        if (Boss.BleedSlider != null)
        {
            Boss.BleedSlider.gameObject.SetActive(isVisible);
        }
    }

    public override void Exit()
    {
        base.Exit();
        if (Boss != null && !Boss.IsDie)
        {
            ToggleBossVisible(true);
        }

        foreach (var w in activeWarnings)
        {
            if (w.WarningObj != null) GameObject.Destroy(w.WarningObj);
        }
        activeWarnings.Clear();
    }
}