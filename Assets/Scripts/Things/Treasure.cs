using UnityEngine;

public class Treasure : Things
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Open()
    {
        if (animator != null)
            animator.SetBool("isOpen", true);
    }
}
