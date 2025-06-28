using UnityEngine;

public class CGGalleryController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string parameterName = "CGIndex";
    [SerializeField] private int totalCGs = 3;

    private int currentIndex = 0;

    private void Start()
    {
        PlayNextCG();
    }

    private void PlayNextCG()
    {

        
            currentIndex = (currentIndex + 1) % totalCGs;
        

        animator.SetInteger(parameterName, currentIndex);
    }
}