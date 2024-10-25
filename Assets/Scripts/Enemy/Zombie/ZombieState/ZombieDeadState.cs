﻿using UnityEngine;
public class ZombieDeadState:EnemyStateBase
{
    public ZombieDeadState(Enemy enemy) : base(enemy)
    {
    }

    public override void OnEnter()
    {
        enemy.rb.constraints = RigidbodyConstraints.FreezeRotationX
                               | RigidbodyConstraints.FreezeRotationZ;
        enemy.selfMyObject.Recycle();
    }

    public override void OnExit()
    {
    }

    public override void OnUpdate()
    {
    }

    public override void OnCheck()
    {
        
    }

    public override void OnFixUpdate()
    {
    }
}
