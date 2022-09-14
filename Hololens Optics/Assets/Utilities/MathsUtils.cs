using System;
using UnityEngine;

public static class MathsUtils
{
    public static Vector3 RoundToInt(this Vector3 v)
    {
        return new Vector3(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
    }
    public static Vector3 MRound(this Vector3 v, float multipleOf)
    {
        return new Vector3(v.x.MRound(multipleOf), v.y.MRound(multipleOf), v.z.MRound(multipleOf));
    }
    public static float MRound(this float value, float multipleOf)
    {
        return (float)Math.Round((decimal)value / (decimal)multipleOf, MidpointRounding.AwayFromZero) * multipleOf;
    }
}