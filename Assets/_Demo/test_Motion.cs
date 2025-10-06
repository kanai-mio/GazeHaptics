using UnityEngine;

public class test_Motion : MonoBehaviour
{
    public AudioSource audioSource;  // äyäÌâπ
    public Animator animator;        // è„Ç≈ê›íËÇµÇΩAnimator

    void Update()
    {
        animator.SetBool("isPlaying", audioSource.isPlaying);
    }
}
