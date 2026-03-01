using UnityEngine;

public class BossTeleportState : EnemyStateBase
{
    private ThreePhaseBoss Boss;
    private float teleportTimer;

    private enum TeleportStep { Vanish, Move, Appear, Finish }
    private TeleportStep currentStep;

    private float vanishDuration = 0.4f; 
    private float appearDuration = 0.2f; 
    private float distanceFromPlayer = 2.5f;

    public BossTeleportState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
        Boss = enemy as ThreePhaseBoss;
    }

    public override void Enter()
    {
        base.Enter();
        Boss.SetVelocityX(0);
        Boss.IsImmunity = true;
        teleportTimer = 0;
        currentStep = TeleportStep.Vanish;

        Boss.CreateAfterImage();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        teleportTimer += Time.deltaTime;

        switch (currentStep)
        {
            case TeleportStep.Vanish:
                HandleVanish();
                break;
            case TeleportStep.Move:
                HandleMove();
                break;
            case TeleportStep.Appear:
                HandleAppear();
                break;
            case TeleportStep.Finish:
                if (Boss.Phase > 1 && Random.value < 0.4f)
                {
                    Boss.StartCombo("Wave", 0.2f);
                    state.ChangeState(Boss.BossIdleState);
                }
                else
                {
                    if (Random.value < 0.3f)
                        state.ChangeState(Boss.FanState);
                    else
                        state.ChangeState(Boss.DashState);
                }
                break;
        }
    }

    public override void Exit()
    {
        base.Exit();
        Boss.IsImmunity = false; 
        SetAlpha(1f);
    }

    private void HandleVanish()
    {
        if (AudioService.Instance != null)
        {
            AudioService.Instance.PlayEffect("Boss_Vanish");
        }
        float alpha = Mathf.Lerp(1, 0, teleportTimer / vanishDuration);
        SetAlpha(alpha);

        if (teleportTimer >= vanishDuration)
        {
            currentStep = TeleportStep.Move;
            teleportTimer = 0;
        }
    }

    private void HandleMove()
    {
        if (Boss.player == null)
        {
            currentStep = TeleportStep.Appear;
            return;
        }

        Vector3 idealPos = Boss.player.position;

        // Get to the side opposite to the player
        if (Boss.player.position.x > Boss.transform.position.x)
            idealPos.x += distanceFromPlayer;
        else
            idealPos.x -= 2.5f;

        idealPos.y = Boss.transform.position.y;

        // Detect whether this location is safe
        Vector3 safePos = GetSafeTeleportPos(Boss.player.position, idealPos);

        // 3. 执行移动
        Boss.transform.position = safePos;

        // 4. 转向玩家
        if (Boss.player.position.x > Boss.transform.position.x) Boss.Flip(true);
        else Boss.Flip(false);

        currentStep = TeleportStep.Appear;
        teleportTimer = 0;
    }

    /// <summary>
    /// Acquire a secure teleportation location
    /// </summary>
    /// <param name="originPos">Starting point (palyer pos)</param>
    /// <param name="targetPos">Target point (ideal pos for teleport)</param>
    /// <returns>Corrected Safety Coordinates</returns>
    private Vector3 GetSafeTeleportPos(Vector3 originPos, Vector3 targetPos)
    {
        // Wall Check

        float distance = Vector3.Distance(originPos, targetPos);
        Vector3 direction = (targetPos - originPos).normalized;

        // 发射射线，检测 Ground 层
        RaycastHit2D hit = Physics2D.Raycast(originPos, direction, distance, LayerMask.GetMask("Ground"));

        if (hit.collider != null)
        {
            Debug.Log("Teleport hit wall! Adjusting position...");
            return hit.point - (Vector2)direction * 0.8f;
        }

        // Ground Check
        RaycastHit2D groundHit = Physics2D.Raycast(targetPos, Vector2.down, 2.0f, LayerMask.GetMask("Ground"));

        // If not on the ground, then deliver a close-range attack.
        if (groundHit.collider == null)
        {
            Debug.Log("Teleport target is Air! Fallback to Player pos.");
            return originPos;
        }
        return targetPos;
    }

    private void HandleAppear()
    {
        float alpha = Mathf.Lerp(0, 1, teleportTimer / appearDuration);
        SetAlpha(alpha);

        if (teleportTimer >= appearDuration)
        {
            currentStep = TeleportStep.Finish;
        }
    }

    private void SetAlpha(float alpha)
    {
        var sr = Boss.EnemySpriteRenderer();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}