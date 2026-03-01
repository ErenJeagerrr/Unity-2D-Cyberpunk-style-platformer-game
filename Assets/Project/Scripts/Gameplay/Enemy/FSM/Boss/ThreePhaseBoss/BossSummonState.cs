using UnityEngine;
using System.Collections;

public class BossSummonState : EnemyStateBase
{
    private ThreePhaseBoss Boss;
    private Coroutine summonCoroutine;

    public BossSummonState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
        Boss = enemy as ThreePhaseBoss;
    }

    public override void Enter()
    {
        if (Boss == null || Boss.IsDie) return;

        base.Enter();
        Boss.SetVelocityX(0);

        summonCoroutine = Boss.StartCoroutine(SummonSequence());
    }

    IEnumerator SummonSequence()
    {
        yield return new WaitForSeconds(1f);

        SummonSpikes();

        yield return new WaitForSeconds(1f);

        state.ChangeState(Boss.BossChaseState);
    }

    private void SummonSpikes()
    {
        if (Boss.SpikePrefab == null) return;

        // 1.Generate one beneath the player's feet.
        if (Boss.player != null)
        {
            GameObject.Instantiate(Boss.SpikePrefab, Boss.player.position, Quaternion.identity);
        }

        // 2. Randomly generate four around the boss.
        if (Boss.transform == null) return;

        for (int i = 0; i < 4; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * 5f;
            Vector3 spawnPos = Boss.transform.position + new Vector3(randomOffset.x, 0, 0);

            if (Boss.GroundCheckPos != null)
            {
                spawnPos.y = Boss.GroundCheckPos.position.y;
            }

            GameObject.Instantiate(Boss.SpikePrefab, spawnPos, Quaternion.identity);
        }
    }

    public override void Exit()
    {
        base.Exit();
        if (Boss != null && summonCoroutine != null)
        {
            Boss.StopCoroutine(summonCoroutine);
            summonCoroutine = null;
        }
    }

    public override void FrameUpdate() { }
}