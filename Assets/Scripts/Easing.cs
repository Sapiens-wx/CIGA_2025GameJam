using UnityEngine;

public class Easing{
    public static float OutExpo(float t){
        return t == 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
    }
    public static float InOutSine(float t){
        return -(Mathf.Cos(Mathf.PI * t) - 1) / 2;
    }
}