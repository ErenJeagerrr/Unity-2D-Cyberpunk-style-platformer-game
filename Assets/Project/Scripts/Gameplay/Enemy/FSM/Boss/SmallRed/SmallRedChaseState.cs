using UnityEngine;

public class SmallRedChaseState : BossChaseState
{
    private SmallRed smallRed;

    public SmallRedChaseState(Enemy enemy, EnemyStateMachine stateMachine, string AniName) : base(enemy, stateMachine, AniName)
    {
        smallRed = enemy as SmallRed;
    }

    public override void FrameUpdate()
    {
        if (smallRed.player == null) return;

        float dist = Vector2.Distance(smallRed.player.position, smallRed.transform.position);

        if (smallRed.CheckIsCanAttack())
        {
            state.ChangeState(smallRed.BossAttackState);
            return;
        }

        if (smallRed.TryUseSkill("Summon"))
        {
            state.ChangeState(smallRed.SummonState);
            return;
        }

        if (smallRed.TryUseSkill("SPAttack"))
        {
            state.ChangeState(smallRed.SPAttackState);
            return;
        }

        MoveTowardsPlayer();
    }

    private void MoveTowardsPlayer()
    {
        float xDiff = smallRed.player.position.x - smallRed.transform.position.x;
        float chaseSpeed = Mathf.Abs(smallRed.CurrentChaseSpeed);

        if (Mathf.Abs(xDiff) > 0.5f)
        {
            if (xDiff > 0)
            {
                smallRed.Flip(true);
                smallRed.SetVelocityX(chaseSpeed);
            }
            else
            {
                smallRed.Flip(false);
                smallRed.SetVelocityX(-chaseSpeed);
            }
        }
        else
        {
            smallRed.SetVelocityX(0);
        }
    }
}