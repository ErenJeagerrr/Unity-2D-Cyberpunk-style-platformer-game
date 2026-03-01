using UnityEngine;
using System.Collections.Generic;

public class BossMissileState : EnemyStateBase
{
    private ThreePhaseBoss Boss;
    private int missileCount = 3;
    private float interval = 0.2f;

    public BossMissileState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
        Boss = enemy as ThreePhaseBoss;
    }

    public override void Enter()
    {
        base.Enter();
        Boss.SetVelocityX(0);

        SpawnMissileRecursive(missileCount);
    }

    private void SpawnMissileRecursive(int countLeft)
    {
        if (Boss == null || Boss.IsDie || state.CurrentState != this) return;

        if (countLeft <= 0)
        {
            TimerSystem.Instance.AddTask(0.5f, () => {
                if (state.CurrentState == this)
                    state.ChangeState(Boss.BossChaseState);
            });
            return;
        }

        CreateMissile();

        // [TODO] ²¥·ÅÒôÐ§)
        if (AudioService.Instance != null)
            AudioService.Instance.PlayEffect("Attack2");

        TimerSystem.Instance.AddTask(interval, () => {
            SpawnMissileRecursive(countLeft - 1);
        });
    }

    private void CreateMissile()
    {
        if (Boss.MissilePrefab == null)
        {
            Debug.LogError("[Boss ERROR] MissilePrefab is empty! Please draw a prefab within the Inspector!");
            return;
        }

        if (Boss.player == null) return;


        Vector3 spawnPos = Boss.transform.position + Vector3.up * 1.5f;

        GameObject go = GameObject.Instantiate(Boss.MissilePrefab, spawnPos, Quaternion.identity);


        HomingMissile missile = go.GetComponent<HomingMissile>();

        if (missile != null)
        {
            if (!Boss.IsFaceRight)
            {
                go.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            missile.Init(Boss.player, Boss.CurrentAtk);
        }
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
    }
}