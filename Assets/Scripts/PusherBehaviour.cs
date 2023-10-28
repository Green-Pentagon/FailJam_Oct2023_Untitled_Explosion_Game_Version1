using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PusherBehaviour : MonoBehaviour
{

    public int levelNumber;
    public int pusherIndex;
    public bool makeStaticRotating;
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetInteger("level", levelNumber);
        animator.SetInteger("index", pusherIndex);
        animator.SetBool("rotate", makeStaticRotating);
    }

}
