using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveNode : MonoBehaviour
{
    public static MoveNode instance;    // 어디서든 불러서 사용 가능
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
            Vector2 dir = ((Vector2)Input.mousePosition - startPos);    // 거리
            Vector2 ndir = dir.normalized;                              // 방향 벡터
            Vector2 adir = new Vector2(Mathf.Abs(dir.x), Mathf.Abs(dir.y)); // 절댓값

            // 현재 노드의 x y 값을 복사
            point = Point.Dup(node.index);

            Point p = Point.Zero;

            if (dir.magnitude > 32) { // 노드가 다른 노드 절반을 넘어가면
                // 어느 방향으로 넘어가는지 확인
                // 좌 or 우
                if(adir.x > adir.y)
                {
                    p = new Point((ndir.x > 0) ? 1 : -1, 0);
                }
                // 상(-1) or 하(1) - y값이 커지면 화면에선 아래쪽으로 감
                else if (adir.x < adir.y)
                {
                    p = new Point(0, (ndir.y > 0) ? -1 : 1);
                }
                
                // 방향을 구해서 현재 노드 위치에서 방향을 더함
                point.Add(p);

                // 노드의 현재 위치 
                Vector2 pos = gameManager.GetPosFromPoint(node.index);


                // 현재 노드랑 노드에 방향 더한게 같지 않다면(움직였다면)
                if (!point.Equals(node.index))
                {
                    // pos에 가야하는 만큼 더하기 - y값이 커지면 화면에선 아래쪽으로 감
                    pos += Point.Multiple(new Point(p.x, -p.y), 16).ToVector();
                }

                // 현재 노드가 pos 값 만큼 이동
                node.MovePosTo(pos);
                //Debug.Log("pos : " + pos);
            }
        }
    }

    public void MovingNode(NodeJelly nodeJelly) // 노드 누를 때 작동, 누른 노드 정보 초기화
    {
        if (node != null)
            return;

        node = nodeJelly;
        startPos = Input.mousePosition;
    }

    public void PopNode()   // 노드에서 뗄 때 작동, 리셋 노드 작동 누른 노드 정보 없애고
    {
        if(node == null)    // 노드가 없다 작동안함
            return;

        //Debug.Log("1");

        // 움직였고 기준치를 넘었을 때 움직인 쪽과 노드 교체
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

            gameManager.isCount = true;                     // 움직이면 남은 횟수 깎이는 조건 달성
            gameManager.blockPanel.SetActive(true);         // 움직이는 도중에 못 건드리게

        }
        // 걍 다시 손 떼면 원래 위치로 돌아옴
        else
            gameManager.ResetNode(node);

        node = null; // 손떼면 초기화
    }
}
