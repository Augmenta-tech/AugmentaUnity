using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentaPersonBehaviour : MonoBehaviour {

    public AugmentaBasicManager augmentaBasicManager;

    public int pid;

    public bool LoopAliveAnimation;

    public delegate void DisappearAnimationCompleted(int pid);
    public event DisappearAnimationCompleted disappearAnimationCompleted;

    public void Appear()
    {
        StartCoroutine(AppearAnimation(AppearCallBack));
    }

    public void Disappear()
    {
        StartCoroutine(DisappearAnimation(DisappearCallBack));
    }

    #region Callback Functions

    protected virtual void AppearCallBack()
    {
        StartCoroutine(AliveAnimation(AliveCallBack));
    }

    protected virtual void AliveCallBack()
    {
        if (LoopAliveAnimation)
            StartCoroutine(AliveAnimation(AliveCallBack));
    }

    protected virtual void DisappearCallBack()
    {
        if (disappearAnimationCompleted != null)
            disappearAnimationCompleted(pid);
    }

    #endregion

    #region Animation Coroutines

    protected virtual IEnumerator AppearAnimation(System.Action callBack = null)
    {
        yield return 0;

        if (callBack != null)
            callBack();
    }

    protected virtual IEnumerator AliveAnimation(System.Action callBack = null)
    {
        yield return 0;

        if (callBack != null)
            callBack();
    }

    protected virtual IEnumerator DisappearAnimation(System.Action callBack = null)
    {
        yield return 0;

        if (callBack != null)
            callBack();
    }

    #endregion
}
