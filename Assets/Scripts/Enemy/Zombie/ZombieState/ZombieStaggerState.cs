﻿public class ZombieStaggerState:ZombieStateBase
{
    public ZombieStaggerState(Enemy enemy) : base(enemy)
    {
    }

    public override void OnInit()
    {
    }

    public override void OnEnter()
    {
        enemy.canGrab = true;
    }

    public override void OnExit()
    {
        enemy.canGrab = false;
    }

    public override void OnShutdown()
    {
    }

    public override void OnUpdate()
    {
        if(blackboard.currentHealth<=0)
        {
            CurrentFsm.ChangeState<ZombieDeadState>();
        }
    }

    public override void OnCheck()
    {
    }

    public override void OnFixedUpdate()
    {
    }
}