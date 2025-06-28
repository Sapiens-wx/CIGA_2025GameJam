using UnityEngine;

public class Book : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] BookPage[] pages;

    bool isOpen;
    int currentPage;
    void Awake(){
        DisplayPage(-1);
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
            ToggleBook();
    }
    void ToggleBook(){
        if(isOpen)
            DisplayPage(-1);
        else
            DisplayPage(GetCurrentPageIndex());
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
}
