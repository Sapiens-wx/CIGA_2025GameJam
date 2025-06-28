using System.Collections;
using UnityEngine;

public class Book : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] BookPage[] pages;
    [Header("Animation Params")]
    [SerializeField] float duration;

    bool isOpen;
    Vector3 displayPosition, hidePosition;
    int currentPage;
    Coroutine openCoroutine, closeCoroutine;
    void Awake(){
        DisplayPage(-1);
        displayPosition=panel.transform.position;
        hidePosition=displayPosition;
        hidePosition.y-=Screen.height;
        panel.transform.position=hidePosition;
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
            ToggleBook();
    }
    void ToggleBook(){
        if(isOpen){ //close book
            if(openCoroutine!=null){
                StopCoroutine(openCoroutine);
                openCoroutine=null;
            }
            closeCoroutine=StartCoroutine(MoveTo(hidePosition,()=>{DisplayPage(-1); closeCoroutine=null;}));
        }
        else{ //open book
            if(closeCoroutine!=null){
                StopCoroutine(closeCoroutine);
                closeCoroutine=null;
            }
            DisplayPage(GetCurrentPageIndex());
            openCoroutine=StartCoroutine(MoveTo(displayPosition,()=>{ openCoroutine=null;}));
        }
        isOpen=!isOpen;
    }
    void DisplayPage(int idx){
        currentPage=idx;
        for(int i=0;i<pages.Length;++i){
            pages[i].gameObject.SetActive(false);
        }
        if(idx>=0&&idx<pages.Length){
            panel.SetActive(true);
            pages[idx].gameObject.SetActive(true);
        } else
            panel.SetActive(false);
    }
    int GetCurrentPageIndex(){
        return 0;
    }
    public void NextPage(int offset){
        int newPage=currentPage+offset;
        if(newPage>=0&&newPage<pages.Length){
            currentPage=newPage;
            DisplayPage(currentPage);
        }
    }
    IEnumerator MoveTo(Vector3 to, System.Action callback){
        WaitForFixedUpdate wait=new WaitForFixedUpdate();
        float toTime=Time.time+duration;
        float t=0, dt=Time.fixedDeltaTime/duration;
        Vector3 startPos=panel.transform.position;
        while(t<1){
            panel.transform.position=Vector3.Lerp(startPos, to, t);
            t+=dt;
            yield return wait;
        }
        panel.transform.position=to;
        callback?.Invoke();
    }
}
