using UnityEngine;  
using System.Collections;  
using UnityEngine.UI;  
using UnityEngine.EventSystems;

public class MapScrollRect : ScrollRect   
{
    public float scrollRate = 0.25f;
        private int touchNum = 0;  
        public override void OnBeginDrag (PointerEventData eventData)  
        {  
            if(Input.touchCount > 1)   
            {  
                return;  
            }  
  
            base.OnBeginDrag(eventData);  
        }  
  
        public override void OnDrag (PointerEventData eventData)  
        {  
            if (Input.touchCount > 1)  
            {  
                touchNum = Input.touchCount;  
                return;  
            }  
            else if(Input.touchCount == 1 && touchNum > 1)  
            {  
                touchNum = Input.touchCount;  
                base.OnBeginDrag(eventData);  
                return;  
            }  
  
            base.OnDrag(eventData);  
        }  
  
  
  
    private float preX;  
    private float preY;  
  
    private void Update()  
    {  
    if (Input.touchCount == 2)  
        {  
            Touch   t1   = Input.GetTouch(0);  
            Touch   t2   = Input.GetTouch(1);  
    
            Vector3 p1   = t1.position;  
            Vector3 p2   = t2.position;  
    
            float   newX = Mathf.Abs(p1.x - p2.x);  
            float   newY = Mathf.Abs(p1.y - p2.y);  
    
            if (t1.phase == TouchPhase.Began || t2.phase == TouchPhase.Began)  
            {  
                preX = newX;  
                preY = newY;  
            }  
            else if (t1.phase == TouchPhase.Moved && t2.phase == TouchPhase.Moved)  
            {     
                RectTransform rt    = base.content;  
                float         scale = (newX + newY - preX - preY) / (rt.rect.width * scrollRate) + rt.localScale.x;  
                
                if (0.25f < scale && scale < 1.0f)  
                {         
                    float ratio   = scale / rt.localScale.x;  
    
                    rt.localScale = new Vector3(scale, scale, 0);  
    
                    float maxX    = base.content.rect.width  * scale / 2 - this.viewRect.rect.width  / 2;  
                    float minX    = -maxX;  
    
                    float maxY    = base.content.rect.height * scale / 2 - this.viewRect.rect.height / 2;  
                    float minY    = -maxY;  
    
                    Vector3 pos   = rt.anchoredPosition * ratio;  
    
                    if (pos.x > maxX)  
                    {  
                        pos.x = maxX;  
                    }  
                    else if (pos.x < minX)  
                    {  
                        pos.x = minX;  
                    }  
    
                    if (pos.y > maxY)  
                    {  
                        pos.y = maxY;  
                    }  
                    else if (pos.y < minY)  
                    {  
                        pos.y = minY;  
                    }  
    
                    rt.anchoredPosition = pos;
                }  
            }  
            preX = newX;  
            preY = newY;  
        }  
    }  
}