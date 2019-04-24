using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaBasicPersonBehaviour : AugmentaPersonBehaviour {

    public float animatedValue;

    [Header("Appeareance animation")]
    public float appearAnimDuration;
    public AnimationCurve appearAnimCurve;

    [Header("Alive animation")]
    public float aliveAnimDuration;
    public AnimationCurve aliveAnimCurve;

    [Header("Disappeareance animation")]
    public float disappearAnimDuration;
    public bool startWithActualValue;
    public AnimationCurve disappearAnimCurve;

    protected override IEnumerator AppearAnimation(System.Action callBack = null)
    {
        var currentTime = 0.0f;
        while (currentTime < appearAnimDuration)
        {
            currentTime += Time.deltaTime;

            if (appearAnimCurve != null)
                animatedValue = appearAnimCurve.Evaluate(Mathf.Clamp01(currentTime / appearAnimDuration));
            else
                animatedValue = Mathf.Clamp01(currentTime / appearAnimDuration);

            yield return new WaitForFixedUpdate();
        }

        if (callBack != null)
            callBack();
    }

    protected override IEnumerator AliveAnimation(System.Action callBack = null)
    {
        var currentTime = 0.0f;
        while (currentTime < aliveAnimDuration)
        {
            currentTime += Time.deltaTime;

            if (aliveAnimCurve != null)
                animatedValue = aliveAnimCurve.Evaluate(Mathf.Clamp01(currentTime / aliveAnimDuration));
            else
                animatedValue = Mathf.Clamp01(currentTime / aliveAnimDuration);

            yield return new WaitForFixedUpdate();
        }

        if (callBack != null)
            callBack();
    }

    protected override IEnumerator DisappearAnimation(System.Action callBack = null)
    {
        if (startWithActualValue)
            disappearAnimCurve.MoveKey(0, new Keyframe(0.0f, animatedValue));

        var currentTime = 0.0f;
        while (currentTime < disappearAnimDuration)
        {
            currentTime += Time.deltaTime;
            animatedValue = disappearAnimCurve.Evaluate(currentTime / disappearAnimDuration);

            yield return new WaitForFixedUpdate();
        }

        if (callBack != null)
            callBack();
    }
}
