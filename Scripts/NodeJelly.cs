using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NodeJelly : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int value;       // ���� �̹��� ��ȣ
    public Point index;     // ��� ����

    //[HideInInspector]
    public Vector2 pos;     // ��ġ
    //[HideInInspector]
    public RectTransform rect; //��ǥ����


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

    public void ResetPos() // ��尡 x y ���� ���� �ٲ�� ��ġ ���ϴ� ��
    {
        //Debug.Log("3");
        pos = new Vector2((64 * index.x), -(64 * index.y));
    }


    public void MovePosTo(Vector2 pos)  // pos �������� �̵�
    {
        //Debug.Log("pos : " + pos);
        //Debug.Log(Mathf.Round(rect.anchoredPosition.x) + " " + Mathf.Round(rect.anchoredPosition.y));
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, pos, Time.deltaTime * 5f);
    }


    public bool UpdateNode()
    {
        //Debug.Log("updatenode , pos : " + pos); ;
        //Debug.Log(Vector3.Distance(rect.anchoredPosition, pos));

        // ���� ��� ���� ��ġ���� ����� �� �ٽ� ����ġ�� ����
        if (Vector2.Distance(rect.anchoredPosition, pos) > 1) 
        {
            //Debug.Log("pos(����) : " + pos);
            MovePosTo(pos);
            isTouch = true;
            return true;
        }
        // ���������� �����̰� ����ġ�� ���ƿ�
        else
        {
            //Debug.Log("pos(����ġ) : " + pos);                 
            rect.anchoredPosition = pos;
            isTouch = false;
            return false;
        }
    }

    public void OnPointerDown(PointerEventData eventData) // ��� ������ ���� ��
    {
        //Debug.Log("touch down: " + transform.name);
        if (isTouch) 
            return;

        if(value != -1 && value != -2)
            MoveNode.instance.MovingNode(this);             // MoveNode�� ���� ����
    }

    public void OnPointerUp(PointerEventData eventData) // ��忡�� �������� ��
    {
        // Debug.Log("touch up: " + transform.name);
        MoveNode.instance.PopNode();
    }
}
