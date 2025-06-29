using System.Collections;
using UnityEngine;

public class Book : MonoBehaviour
{
    [SerializeField] GameObject panel, bookPanel;
    [SerializeField] BookPage[] pages;
    [Header("Animation Params")]
    [SerializeField] float duration;
    [SerializeField] float shakeAmount, shakeDuration;

    bool isOpen;
    Vector3 displayPosition, hidePosition;
    int currentPage;
    Coroutine openCoroutine, closeCoroutine, shakeCoro;
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
        if(shakeCoro!=null) return;
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
        if(SceneController.Instance==null) return 0;
        return SceneController.Instance.CurrentDay;
    }
    public void NextPage(int offset){
        if(shakeCoro!=null||openCoroutine!=null||closeCoroutine!=null) return;
        int newPage=currentPage+offset;
        if(newPage>=0&&newPage<pages.Length){
            currentPage=newPage;
            DisplayPage(currentPage);
        }
        shakeCoro=StartCoroutine(Shake(-offset));
    }
    IEnumerator MoveTo(Vector3 to, System.Action callback){
        WaitForFixedUpdate wait=new WaitForFixedUpdate();
        float toTime=Time.time+duration;
        float t=0, dt=Time.fixedDeltaTime/duration;
        Vector3 startPos=panel.transform.position;
        while(t<1){
            panel.transform.position=Vector3.Lerp(startPos, to, Easing.OutExpo(t));
            t+=dt;
            yield return wait;
        }
        panel.transform.position=to;
        callback?.Invoke();
    }
    IEnumerator Shake(int dir){
        Vector3 startPos=bookPanel.transform.position, toPos=startPos+new Vector3(dir*shakeAmount,0,0);
        float t=0, dt=Time.fixedDeltaTime/(shakeDuration/2);
        WaitForFixedUpdate wait=new WaitForFixedUpdate();
        while(t<1){
            bookPanel.transform.position=Vector3.Lerp(startPos, toPos, Easing.InOutSine(t));
            t+=dt;
            yield return wait;
        }
        t=0;
        while(t<1){
            bookPanel.transform.position=Vector3.Lerp(toPos, startPos, Easing.InOutSine(t));
            t+=dt;
            yield return wait;
        }
        shakeCoro=null;
    }
}
