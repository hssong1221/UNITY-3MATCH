using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NodeJelly : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int value;       // 젤리 이미지 번호
    public Point index;     // 노드 순서

    //[HideInInspector]
    public Vector2 pos;     // 위치
    //[HideInInspector]
    public RectTransform rect; //좌표기준


    Image img;
    bool isTouch;

    public void Initialize(int v, Point p, Sprite sprite)
    {
        //Debug.Log("1");
        img = GetComponent<Image>();
        rect = GetComponent<RectTransform>();

        value = v;
        img.sprite = sprite;
        SetJellyInfo(p);
    }

    public void SetJellyInfo(Point p)
    {
        //Debug.Log("3");
        index = p;
        ResetPos();
        transform.name = "Node [" + index.x + "," + index.y + "]";
    }

    public void ResetPos() // 노드가 x y 값에 따라 바뀌는 위치 정하는 곳
    {
        //Debug.Log("3");
        pos = new Vector2((64 * index.x), -(64 * index.y));
    }


    public void MovePosTo(Vector2 pos)  // pos 방향으로 이동
    {
        //Debug.Log("pos : " + pos);
        //Debug.Log(Mathf.Round(rect.anchoredPosition.x) + " " + Mathf.Round(rect.anchoredPosition.y));
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, pos, Time.deltaTime * 5f);
    }


    public bool UpdateNode()
    {
        //Debug.Log("updatenode , pos : " + pos); ;
        //Debug.Log(Vector3.Distance(rect.anchoredPosition, pos));

        // 현재 노드 원래 위치에서 벗어났을 때 다시 원위치로 복귀
        if (Vector2.Distance(rect.anchoredPosition, pos) > 1) 
        {
            //Debug.Log("pos(움직) : " + pos);
            MovePosTo(pos);
            isTouch = true;
            return true;
        }
        // 마지막으로 움직이고 원위치로 돌아옴
        else
        {
            //Debug.Log("pos(원위치) : " + pos);                 
            rect.anchoredPosition = pos;
            isTouch = false;
            return false;
        }
    }

    public void OnPointerDown(PointerEventData eventData) // 노드 누르고 있을 때
    {
        //Debug.Log("touch down: " + transform.name);
        if (isTouch) 
            return;

        if(value != -1 && value != -2)
            MoveNode.instance.MovingNode(this);             // MoveNode로 정보 전달
    }

    public void OnPointerUp(PointerEventData eventData) // 노드에서 떨어졌을 때
    {
        // Debug.Log("touch up: " + transform.name);
        MoveNode.instance.PopNode();
    }
}
