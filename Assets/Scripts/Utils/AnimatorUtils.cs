using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorUtils : MonoBehaviour
{
    public static AnimatorUtils animatorUtils;
    private void Awake() {
        if (animatorUtils != null) {
            GameObject.Destroy(animatorUtils);
        } else {
            animatorUtils = this;
        }
        DontDestroyOnLoad(this);
    }

    public class AnimatorWatcher
    {
        public Animator animator;
        public string animationName;
        public Action callback;
    }
    private static Animator animator;
    private static string animationName;
    private static Action callback;

    public static void Watch(Animator anim, string animName, Action cb) {
        animator = anim;
        animationName = animName;
        callback = cb;
    }

    private void Update() {
        if (animator != null) {
            AnimatorStateInfo anim = animator.GetCurrentAnimatorStateInfo(0);
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (!anim.IsName(animationName) || anim.normalizedTime >= 1.0f) {
                animator = null;
                callback();
            }
        }
    }


}
