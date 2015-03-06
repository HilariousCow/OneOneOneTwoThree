using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FloatRangeAttribute : PropertyAttribute
{
    public float MinLimit, MaxLimit;

    public FloatRangeAttribute(float minLimit, float maxLimit)
    {
        this.MinLimit = minLimit;
        this.MaxLimit = maxLimit;
    }
}

[System.Serializable]
public class FloatRange
{
    public float RangeStart, RangeEnd;

    //todo: possibly use a centralized MonoRandom
    public float GetRandomValue()
    {
        return Random.Range(RangeStart, RangeEnd);
    }
}