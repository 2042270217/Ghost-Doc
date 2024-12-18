using Services;
using Services.Event;
using System;
using UnityEngine;

[Serializable]
public class PlayerBlackboard
{
    private IEventSystem eventSystem;
    private IEventSystem EventSystem
    {
        get
        {
            eventSystem ??= ServiceLocator.Get<IEventSystem>();
            return eventSystem;
        }
    }

    public float maxHealth = 100f;
    [SerializeField]
    private float health;
    public float Health
    {
        get => health;
        set
        {
            if (value != health)
            {
                EventSystem.Invoke(EEvent.PlayerHPChange, value, maxHealth);
                health = value;
            }
        }
    }

    public float maxEnergy = 300f;
    [SerializeField]
    private float energy;
    public float Energy
    {
        get => energy;
        set
        {
            if (value != energy)
            {
                EventSystem.Invoke(EEvent.PlayerEnergyChange, value, maxEnergy);
                energy = value;
            }
        }
    }

    public bool grounded;
    public bool jumping;
    public bool isWallJump;
    
    public bool doubleJump;

    public Vector3 dirInput;
    public Vector3 moveDir;
    public Vector3 velocity;        //继承速度
    public float speed;             //当前速率
    
    public Vector3 climbXZDir;        //爬墙水平方向
    public float climbSpeed;        //爬墙继承速度
    

    public Type lastState;
    public Type currentState;
    public Type nextState;

    public RaycastHit wallHit;
    public bool isWall;
    public bool sprintingPause;//冲刺打断
    public bool isJumpBuffer;//跳跃预输入

    public bool isMeleeAttacking;//是否近战攻击
    public bool isBlocking;//是否处于格挡状态
    public bool canCamChange=true;
    
    public bool hasClimbOverTime;//爬墙是否超时
    public bool hasClimbOverAngel;//爬墙是否超过攀爬最大角度
    public bool hasClimbEnergyOut;

    public bool isHoldingMelee;//是否持有近战武器
    public bool isCombo;//释放组合技
    public Melee.WeaponState meleeState;//近战状态


    
    public RaycastHit slopeHit; //斜坡检测
    public bool isSlope;//是否在斜坡上
}
