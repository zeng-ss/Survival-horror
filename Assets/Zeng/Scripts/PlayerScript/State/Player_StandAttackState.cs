using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 玩家站立普攻状态
/// </summary>
public class Player_StandAttackState : Player_State
{
    public override void Enter()
    {
        player.PlayerAnimation("StandAttack");
    }
}
