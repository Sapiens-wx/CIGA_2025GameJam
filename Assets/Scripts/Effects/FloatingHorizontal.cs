using UnityEngine;

public class FloatingHorizontal : MonoBehaviour
{
    [SerializeField] float amplitude, frequency;

    Vector3 startPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        startPos=transform.position;
    }
    void FixedUpdate()
    {
        transform.position=startPos+new Vector3(amplitude*Mathf.Sin(2*Mathf.PI*frequency*Time.time),0,0);
    }
}
