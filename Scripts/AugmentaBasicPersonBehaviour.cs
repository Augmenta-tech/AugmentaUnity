using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaBasicPersonBehaviour : AugmentaPersonBehaviour {

    public float AnimatedValue;

    [Header("Appeareance animation")]
    public float AppearAnimDuration;
    public AnimationCurve AppearAnimCurve;

    [Header("Alive animation")]
    public float AliveAnimDuration;
    public AnimationCurve AliveAnimCurve;

    [Header("Disappeareance animation")]
    public float DisappearAnimDuration;
    public bool StartWithActualValue;
    public AnimationCurve DisappearAnimCurve;

    protected override IEnumerator AppearAnimation(System.Action callBack = null)
    {
        var currentTime = 0.0f;
        while (currentTime < AppearAnimDuration)
        {
            currentTime += Time.deltaTime;

            if (AppearAnimCurve != null)
                AnimatedValue = AppearAnimCurve.Evaluate(Mathf.Clamp01(currentTime / AppearAnimDuration));
            else
                AnimatedValue = Mathf.Clamp01(currentTime / AppearAnimDuration);

            yield return new WaitForFixedUpdate();
        }

        if (callBack != null)
            callBack();
    }

    protected override IEnumerator AliveAnimation(System.Action callBack = null)
    {
        var currentTime = 0.0f;
        while (currentTime < AliveAnimDuration)
        {
            currentTime += Time.deltaTime;

            if (AliveAnimCurve != null)
                AnimatedValue = AliveAnimCurve.Evaluate(Mathf.Clamp01(currentTime / AliveAnimDuration));
            else
                AnimatedValue = Mathf.Clamp01(currentTime / AliveAnimDuration);

            yield return new WaitForFixedUpdate();
        }

        if (callBack != null)
            callBack();
    }

    protected override IEnumerator DisappearAnimation(System.Action callBack = null)
    {
        if (StartWithActualValue)
            DisappearAnimCurve.MoveKey(0, new Keyframe(0.0f, AnimatedValue));

        var currentTime = 0.0f;
        while (currentTime < DisappearAnimDuration)
        {
            currentTime += Time.deltaTime;
            AnimatedValue = DisappearAnimCurve.Evaluate(currentTime / DisappearAnimDuration);

            yield return new WaitForFixedUpdate();
        }

        if (callBack != null)
            callBack();
    }
}
