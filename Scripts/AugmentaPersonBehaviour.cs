using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaPersonBehaviour : MonoBehaviour {

    public int pid;
    public float AnimatedValue;

    [Header("Appeareance animation")]
    public float AppearAnimDuration;
    public AnimationCurve AppearAnimCurve;

    [Header("Alive animation")]
    public float AliveAnimDuration;
    public AnimationCurve AliveAnimCurve;
    public bool LoopAliveAnimation;

    [Header("Disappeareance animation")]
    public float DisappearAnimDuration;
    public bool StartWithActualValue;
    public AnimationCurve DisappearAnimCurve;

    public delegate void DisappearAnimationCompleted(int pid);
    public event DisappearAnimationCompleted disappearAnimationCompleted;

    public virtual IEnumerator ValueAnimation(float duration, AnimationCurve animCurve = null, System.Action callBack = null)
    {
        var currentTime = 0.0f;
        while(currentTime < duration)
        {
            if (animCurve != null)
                AnimatedValue = animCurve.Evaluate(currentTime / duration);// Mathf.Lerp(startValue, endValue, animCurve.Evaluate(currentTime / duration));
            else
                AnimatedValue = currentTime / duration;// Mathf.Lerp(startValue, endValue, currentTime / duration);

            currentTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        AnimatedValue = animCurve.Evaluate(1.0f);

        if (callBack != null)
            callBack();

    }

    public virtual void AliveCallBack()
    {
        if(LoopAliveAnimation)
            StartCoroutine(ValueAnimation(AliveAnimDuration, AliveAnimCurve, AliveCallBack));
    }

    public virtual void AppearCallBack()
    {
        StartCoroutine(ValueAnimation(AliveAnimDuration, AliveAnimCurve, AliveCallBack));
    }

    public virtual void DisappearCallBack()
    {
        if (disappearAnimationCompleted != null)
            disappearAnimationCompleted(pid);
    }

    public void Disappear()
    {
        if (StartWithActualValue)
        {
            DisappearAnimCurve.MoveKey(0, new Keyframe(0.0f, AnimatedValue));
        }
        
        StartCoroutine(ValueAnimation(DisappearAnimDuration, DisappearAnimCurve, DisappearCallBack));
    }

    public void Appear()
    {
        StartCoroutine(ValueAnimation(AppearAnimDuration, AppearAnimCurve, AppearCallBack));
    }
}
