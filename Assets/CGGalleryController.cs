using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CGSceneController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string animationStateName = "CGState";
    [SerializeField] private AudioClip cgAudio;
    [SerializeField] private AudioSource audioSource;

    private void Start()
    {
        animator.Play(animationStateName);

        StartCoroutine(WaitForAnimationEnd());
    }

    private IEnumerator WaitForAnimationEnd()
    {

        yield return null;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        while (!state.IsName(animationStateName) || state.normalizedTime < 1f)
        {
            yield return null;
            state = animator.GetCurrentAnimatorStateInfo(0);
        }

        SceneController.Instance.LoadScene(SceneType.MainMenu);
    }
}