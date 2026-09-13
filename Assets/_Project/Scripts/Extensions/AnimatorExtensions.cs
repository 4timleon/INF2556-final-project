using System;
using System.Collections;
using UnityEngine;

public static class AnimatorExtensions
{
    public class AnimationHandle
    {
        private Action _callback;
        public AnimationHandle SetOnComplete(Action callback)
        {
            _callback = callback;
            return this;
        }
        internal void Complete() => _callback?.Invoke();
    }

    public static AnimationHandle CrossFadeWithCallback(
    this Animator animator,
    MonoBehaviour runner,
    int stateHash,
    float transitionDuration,
    int layer = 0,
    bool waitForStateEnd = true)
    {
        var handle = new AnimationHandle();
        runner.StartCoroutine(WaitForCrossFade(animator, stateHash, transitionDuration, layer, waitForStateEnd, handle));
        return handle;
    }

    private static IEnumerator WaitForCrossFade(
        Animator animator,
        int stateHash,
        float duration,
        int layer,
        bool waitForStateEnd,
        AnimationHandle handle)
    {
        animator.CrossFadeInFixedTime(stateHash, duration, layer);

        yield return null;

        while (animator.IsInTransition(layer))
        {
            var next = animator.GetNextAnimatorStateInfo(layer);
            if (next.shortNameHash != 0 && next.shortNameHash != stateHash && next.fullPathHash != stateHash)
                yield break;

            yield return null;
        }

        if (!waitForStateEnd)
        {
            handle.Complete();
            yield break;
        }

        while (true)
        {
            var current = animator.GetCurrentAnimatorStateInfo(layer);

            if (current.shortNameHash != stateHash && current.fullPathHash != stateHash)
                yield break;

            if (current.normalizedTime >= 1.0f)
                break;

            yield return null;
        }

        handle.Complete();
    }
}