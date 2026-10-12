using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 玩家状态类型的枚举
/// </summary>
public enum Player_StateType
{
    Idle,
    Move,
    Jump,
    Attack,
    Hurt,
    Dead,
    Crouch,
    CrouchWalk,
    CrouchToStand
}
