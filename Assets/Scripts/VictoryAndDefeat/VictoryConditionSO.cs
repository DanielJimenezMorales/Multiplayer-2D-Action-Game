using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is an abstract class from which every Victory condition will inherit. It is a base class
/// </summary>
public abstract class VictoryConditionSO : ScriptableObject
{
    public abstract bool CheckCondition(int matchSecondsLeft);
}