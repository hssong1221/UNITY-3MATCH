using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveNode : MonoBehaviour
{
    public static MoveNode instance;    // ��𼭵� �ҷ��� ��� ����
    GameManager gameManager;

    NodeJelly node;
    Point point;
    Vector2 startPos;

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        gameManager = GetComponent<GameManager>();
    }

    void Update()
    {
        if(node != null)
        {
            Vector2 dir = ((Vector2)Input.mousePosition - startPos);    // �Ÿ�
            Vector2 ndir = dir.normalized;                              // ���� ����
            Vector2 adir = new Vector2(Mathf.Abs(dir.x), Mathf.Abs(dir.y)); // ����

            // ���� ����� x y ���� ����
            point = Point.Dup(node.index);

            Point p = Point.Zero;

            if (dir.magnitude > 32) { // ��尡 �ٸ� ��� ������ �Ѿ��
                // ��� �������� �Ѿ���� Ȯ��
                // �� or ��
                if(adir.x > adir.y)
                {
                    p = new Point((ndir.x > 0) ? 1 : -1, 0);
                }
                // ��(-1) or ��(1) - y���� Ŀ���� ȭ�鿡�� �Ʒ������� ��
                else if (adir.x < adir.y)
                {
                    p = new Point(0, (ndir.y > 0) ? -1 : 1);
                }
                
                // ������ ���ؼ� ���� ��� ��ġ���� ������ ����
                point.Add(p);

                // ����� ���� ��ġ 
                Vector2 pos = gameManager.GetPosFromPoint(node.index);


                // ���� ���� ��忡 ���� ���Ѱ� ���� �ʴٸ�(�������ٸ�)
                if (!point.Equals(node.index))
                {
                    // pos�� �����ϴ� ��ŭ ���ϱ� - y���� Ŀ���� ȭ�鿡�� �Ʒ������� ��
                    pos += Point.Multiple(new Point(p.x, -p.y), 16).ToVector();
                }

                // ���� ��尡 pos �� ��ŭ �̵�
                node.MovePosTo(pos);
                //Debug.Log("pos : " + pos);
            }
        }
    }

    public void MovingNode(NodeJelly nodeJelly) // ��� ���� �� �۵�, ���� ��� ���� �ʱ�ȭ
    {
        if (node != null)
            return;

        node = nodeJelly;
        startPos = Input.mousePosition;
    }

    public void PopNode()   // ��忡�� �� �� �۵�, ���� ��� �۵� ���� ��� ���� ���ְ�
    {
        if(node == null)    // ��尡 ���� �۵�����
            return;

        //Debug.Log("1");

        // �������� ����ġ�� �Ѿ��� �� ������ �ʰ� ��� ��ü
        if (!point.Equals(node.index))
        {
            if(0 <= node.value && node.value < 5)
            {
                gameManager.FlipNode(node.index, point, true);
            }
            else if(node.value == 5)
            {
                gameManager.StraightNode(node.index, point);
            }

            gameManager.isCount = true;                     // �����̸� ���� Ƚ�� ���̴� ���� �޼�
            gameManager.blockPanel.SetActive(true);         // �����̴� ���߿� �� �ǵ帮��

        }
        // �� �ٽ� �� ���� ���� ��ġ�� ���ƿ�
        else
            gameManager.ResetNode(node);

        node = null; // �ն��� �ʱ�ȭ
    }
}
