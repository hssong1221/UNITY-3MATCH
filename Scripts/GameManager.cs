using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    #region 변수
    public Board board; // 보드 자체 정보 

    [Header("UI")]
    public Sprite[] jelly;  // 젤리 이미지
    public Sprite hole;     // 구멍 이미지
    public Sprite superNode;// 특수 노드 이미지

    public RectTransform gamePanel; // 게임 패널 위치 기준 

    [Header("프리팹")]
    public GameObject nodeJelly;

    [Header("UI")]
    public Text tryText;
    public Text scoreText;
    public Text goalText;
    public Text endText;
    public GameObject endPanel;
    public GameObject blockPanel;

    //------------------------------------------

    int width = 9;
    int height = 9;
    Node[,] gameBoard;  // 보드에 들어갈 객체 

    int[] refill;

    List<NodeJelly> nodeUpdate; // 위치 업데이트해야 하는 노드 들어가는 곳
    List<FlipNodeJelly> flip;   // 서로 위치 바꿔야하는 노드 2개 들어가는 곳
    List<NodeJelly> straight; // 슈퍼노드 때문에 터지는 노드 들어가는 곳
    List<NodeJelly> dead; // 매칭후에 터지는 노드 들어가는 곳

    List<NodeJelly> finishingUpdate; // 위치 업데이트 끝난 노드들이 들어가는 곳

    bool isSquare;  // 2*2 연결인가?

    [HideInInspector]
    public bool isCount; // 남은 횟수 차감 해야할 때

    //NodeJelly superJelly;

    System.Random rand = new System.Random();

    float timer;
    float wait;

    #endregion

    #region lifeCycle

    void Start()
    {
        nodeUpdate = new List<NodeJelly>(); 
        flip = new List<FlipNodeJelly>();
        straight = new List<NodeJelly>();
        dead = new List<NodeJelly>();
        
        refill = new int[width];


        tryText.text = "21";
        //scoreText.text = "0";
        goalText.text = "3";
        endPanel.SetActive(false);

        InitBoard();
        VerifyBoard();
        InstBoard();

        timer = 0.0f;   // 업데이트 지연 타이머
        wait = 0.9f;

        finishingUpdate = new List<NodeJelly>();

        isSquare = false;
    }


    void Update()
    {
        timer += Time.deltaTime;

        for (int i = 0; i < nodeUpdate.Count; i++)
        {
            // 위치에 딱 고정된 후에
            // false 받으면 finishingUpdate에 넘겨서 최종 판정을 함
            if (!nodeUpdate[i].UpdateNode())
            {
                finishingUpdate.Add(nodeUpdate[i]);
            }
        }
        if (timer > wait)
        {
            //Debug.Log(finishingUpdate.Count);

            finishingUpdate = finishingUpdate.Distinct().ToList();

            // 움직이면 list에서 체크 하고 지움
            for (int i = 0; i < finishingUpdate.Count; i++)
            {
                // node는 바뀌는 노드 flipnode는 누른 노드
                NodeJelly node = finishingUpdate[i];
                FlipNodeJelly flipjelly = GetFlip(node); // flip list에 유효한 값이 들어있다면 값을 가져옴
                NodeJelly flipNode = null;

                int x = (int)node.index.x;
                refill[x] = Mathf.Clamp(refill[x] - 1, 0, width);

                // ------------------------ 매칭된 노드 판정하는 곳 --------------------------
                // 움직인 노드기준 연결된 거 있는지 확인 시작
                List<Point> connected = IsConnected(node.index, true);           // x        x
                bool isFlip = (flipjelly != null);                               // x    ->  x
                                                                                 // oxoo     xooo 검사

                // flip 할수 있다면
                if (isFlip)
                {
                    flipNode = flipjelly.GetOtherNode(node);
                    // flipnode 도 위치 바뀐다음에 연결된 거 있는 지 확인 하기
                    AddPoints(ref connected, IsConnected(flipNode.index, true));

                    timer = 0;
                    //Debug.Log("플립 성공");
                    //Debug.Log(node.index.x + " " + node.index.y);
                    //Debug.Log(flipNode.index.x + " " + flipNode.index.y);
                }

                // straight list에 추가된 블럭이 있다면
                
                if(straight.Count != 0)
                {
                    MoveCount(isCount);
                    foreach (NodeJelly nj in straight)
                    {
                        Point p = nj.index;
                        Node n = GetNodePoint(p);           // 위치로 노드 찾아서
                        NodeJelly nodeJelly = n.GetNode();  // 노드에서 프리팹 찾고
                        if (nodeJelly != null)
                        {
                            //nodeJelly.gameObject.SetActive(false);  // 비활성화

                            dead.Add(nodeJelly);                //매칭후에 터지는 노드들 모아놓기
                        }
                        n.SetNode(null);                    // 초기화
                    }
                    FillBlankNode();                    // 비어 있는 칸 노드가 떨어지면서 채우기
                }
                straight.Clear();    

                // 바꿨는데 하나도 매칭 되는게 없다면
                if (connected.Count == 0)
                {
                    // 다시 원래대로 돌려놓기
                    if (isFlip)
                    {
                        FlipNode(node.index, flipNode.index, false);
                        blockPanel.SetActive(false);                    // 노드가 변경되는 동안 움직임 방지
                    }
                }
                // 매칭 되는게 있다면
                else
                {
                    MoveCount(isCount);

                    // 매칭된 노드 삭제하는 곳
                    foreach (Point p in connected)
                    {
                        //Debug.Log("삭제");
                        //Debug.Log(p.x + " " + p.y);

                        Node n = GetNodePoint(p);           // 위치로 노드 찾아서
                        NodeJelly nodeJelly = n.GetNode();  // 노드에서 프리팹 찾고
                        if (nodeJelly != null)
                        {
                            nodeJelly.gameObject.SetActive(false);  // 비활성화

                            dead.Add(nodeJelly);                //매칭후에 터지는 노드들 모아놓기
                        }
                        n.SetNode(null);                    // 초기화
                    }

                    // node 위치에 특수 블럭 생성
                    if (isSquare)
                    {
                        Point p = new Point(node.index.x, node.index.y);
                        //Debug.Log("특수 생성 준비");

                        // 특수 노드 하나 만들고
                        Node superNode = GetNodePoint(p);
                        //Debug.Log(superNode.index.x + " " + superNode.index.y);

                        // 안에 넣을 정보도 만들고
                        NodeJelly superJelly = null;

                        // 사라진 노드 중에 맞는 위치 찾아서
                        foreach(NodeJelly d in dead)
                        {
                            Point temp = new Point(d.index.x, d.index.y);
                            if (temp.Equals(d.index))
                            {
                                superJelly = d;
                                dead.Remove(d);
                                break;
                            }
                        }
                        // 초기화 해주고
                        superJelly.Initialize(5, p, this.superNode);
                        // 노드에 정보 넣고
                        superNode.SetNode(superJelly);
                        // 보여주고
                        superJelly.gameObject.SetActive(true);

                        ResetNode(superJelly);

                        isSquare = false;
                    }

                    FillBlankNode();                    // 비어 있는 칸 노드가 떨어지면서 채우기
                    
                }
                flip.Remove(flipjelly);                 // 뒤집은거 list에서 삭제
                nodeUpdate.Remove(node);                // 업데이트한거 list에서 삭제

                // 노드가 변경되는 동안 움직임 방지
                if (nodeUpdate.Count == 0)
                    blockPanel.SetActive(false);
            }
            timer = 0;
        }
        finishingUpdate.Clear();
    }

    #endregion

    #region basic logic method (돌아가는 뼈대임)

    void InitBoard()    // 최초 게임 보드 정보 초기화
    {
        gameBoard = new Node[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 상하좌우 테두리는 구멍
                if(y == 0 || x == 0 || y == 8 || x == 8)
                {
                    // 모서리는 안쓰는 칸
                    if (board.rows[x].row[y])
                    {
                        //Debug.Log(x + " " + y);
                        gameBoard[x, y] = new Node(-2, new Point(x, y));
                        continue;
                    }
                    //Debug.Log(x + " " + y);
                    gameBoard[x, y] = new Node(-1, new Point(x, y));
                }
                else
                {
                    // 맨 처음 board는 전부 false 임
                    gameBoard[x, y] = new Node((board.rows[x].row[y]) ? -1 : InitNodeVal(), new Point(x, y));
                }
            }
        }
    }

    int InitNodeVal() // 노드에 젤리 값 부여
    {
        int num;
        num = rand.Next(1, 5); // 젤리 정보 1 - 4까지
        return num;
    }

    void VerifyBoard() // 보드 검증 
    {
        List<int> remove = new List<int>(); // 매칭되서 없애야하는 노드 리스트
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Point p = new Point(x, y);
                int val = GetValuePoint(p);

                if (val <= 0)
                    continue;

                remove.Clear();
                int temp = 10000;
                while (IsConnected(p, true).Count > 0)   // connected list에 추가되있나 확인 (매칭이 안되면 0)
                {
                    val = GetValuePoint(p);
                    if (!remove.Contains(val))          // remove list에 검사한 노드 넣어주고
                        remove.Add(val);

                    SetValuePoint(p, ReValue(ref remove)); // 새 값 넣어주고 다시 검사


                    // 무한 루프 임시 방지
                    temp--;
                    if (temp == 0)
                        break;
                }
            }
        }
    }

    int GetValuePoint(Point p)  // 현재 포인트에서 value값 가져오는데 범위 밖이면 -1
    {
        if (p.x < 0 || p.x >= width || p.y < 0 || p.y >= height)
            return -1;
        return gameBoard[p.x, p.y].value;
    }

    void SetValuePoint(Point p, int val)    // 현재 포인트에 value값 할당
    {
        gameBoard[p.x, p.y].value = val;
    }

    int ReValue(ref List<int> remove) // 안 겹치는 val 값 다시 주는 곳
    {
        List<int> avail = new List<int>();
        for (int i = 0; i < jelly.Length; i++) // 젤리 개수 만큼 주고
            avail.Add(i + 1);

        foreach (int r in remove)           // 겹치는 val값 찾아서 없애주고
            avail.Remove(r);

        if (avail.Count <= 0)
            return 0;

        return avail[rand.Next(0, avail.Count)];    // 그 중에서 랜덤으로 고르기
    }

    List<Point> IsConnected(Point p, bool overlap)  // 검사 노드가 3매치가 되었는지 확인하는 곳
    {
        List<Point> connected = new List<Point>();  // 3매칭 완성된 포인트 리스트를 넣는곳
        int val = GetValuePoint(p);                 // 검사 중인 노드의 젤리모양 값
        Point[] dir = { // 검사 방향 상 우 하 좌 
            Point.Up,
            Point.Right,
            Point.Down,
            Point.Left
        };

        foreach (Point d in dir) // 같은 방향으로 같은 모양 2개 이상 체크 
        {
            List<Point> line = new List<Point>();

            int same = 0;
            for (int i = 1; i < 3; i++)
            {
                Point check = Point.Add(p, Point.Multiple(d, i));

                //Debug.Log(p.x + " " + p.y);
                //Debug.Log(check.x + " " + check.y);

                // 같은 모양이지만 특수노드는 모여도 안터지게 한다
                if (GetValuePoint(check) == val && GetValuePoint(check) != 5)
                {
                    //Debug.Log("same");
                    line.Add(check);
                    same++;
                }
            }

            // 체크하는 방향으로 같은 모양 2개 이상 있으면 3매칭
            if (same > 1)
            {
                AddPoints(ref connected, line); // connected list에 추가
            }
        }

        for (int i = 0; i < 2; i++) // 체크하는 노드 양쪽에 같은 모양이 2개 이상 있는지 확인
        {
            List<Point> line = new List<Point>();
            int same = 0;
            Point next = Point.Add(p, dir[i]);          // 상 | 우
            Point next2 = Point.Add(p, dir[i + 2]);     // 하 | 좌 를 확인
            if (GetValuePoint(next) == val && GetValuePoint(next) != 5)
            {
                line.Add(next);
                same++;
            }
            if (GetValuePoint(next2) == val && GetValuePoint(next2) != 5)
            {
                line.Add(next2);
                same++;
            }

            // 체크하는 방향으로 같은 모양 2개 이상 있으면 3매칭
            if (same > 1)
            {
                AddPoints(ref connected, line); // connected list에 추가
            }
        }

        for (int i = 0; i < 4; i++) // 2*2 모양 체크 - 빙글 돌면서 4군데 확인
        {
            List<Point> square = new List<Point>();
            int same = 0;
            int next = i + 1;
            if (next >= 4)
                next -= 4;

            // 본인 기준 근처 두 칸 그리고 대각선 한 칸 
            Point[] check = { Point.Add(p, dir[i]), Point.Add(p, dir[next]), Point.Add(p, Point.Add(dir[i], dir[next])) };

            foreach (Point c in check)
            {
                if (GetValuePoint(c) == val && GetValuePoint(c) != 5)
                {
                    square.Add(c);
                    same++;
                }
            }

            // 체크해서 같은 모양 3개면 2*2매칭 (1번만)
            if (same > 2)
            {
                AddPoints(ref connected, square);
                Debug.Log("2*2 탐지");
                /*Debug.Log(p.x + " " + p.y);
                foreach (Point c in check)
                    Debug.Log(c.x + " " + c.y);*/
                if(!overlap)
                    isSquare = true;
            }
        }

        // 중복 검사
        // 처음 진입한 isConncedted 라면 지금까지 더한 connected list 와
        //  connected list에 포함된 노드를 검사한  list와 합쳐서 같은 val 인지 체크 함 - ㄱ자나 ㅗ 모양 체크하기 위함
        if (overlap)
        {
            for (int i = 0; i < connected.Count; i++)
            {
                AddPoints(ref connected, IsConnected(connected[i], false));
            }
        }

        return connected;
    }

    void AddPoints(ref List<Point> connect, List<Point> add) // 3매치가 된 노드를 connected list에 넣는 곳
    {
        // connect는  List<Point> connected를 의미함 
        foreach (Point p in add)
        {
            bool flag = true;

            for (int i = 0; i < connect.Count; i++)
            {
                //Debug.Log(points[i]);
                // 같은 노드면 추가 안한다.
                if (connect[i].Equals(p))
                {
                    flag = false;
                    break;
                }
            }

            if (flag)
                connect.Add(p);
        }
    }

    void InstBoard() // 보드 정보에 따라 프리팹 생성
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Node node = GetNodePoint(new Point(x, y));
                int val = node.value;

                if (val == 0 || val == -2)
                    continue;
                
                // 노드 젤리 생성 - 위치와 모양을 정함
                GameObject gameObject = Instantiate(nodeJelly, gamePanel);

                NodeJelly nJelly = gameObject.GetComponent<NodeJelly>();

                if(val == -1)
                {
                    nJelly.Initialize(val, node.index, hole);     // 구멍
                }
                else if (val == 5)
                {
                    nJelly.Initialize(val, node.index, superNode);     // 슈퍼노드
                }
                else
                {
                    nJelly.Initialize(val, node.index, jelly[val - 1]);     // 노드 젤리 모양 위치 정보 초기화
                }
                node.SetNode(nJelly);                                   // 노드에 프리팹 정보 추가

                RectTransform rect = gameObject.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2((64 * x), -(64 * y));
            }
        }
    }

    #endregion

    #region sub methods

    void FillBlankNode() // 비어 있는 칸 노드가 떨어지면서 채우기 하는 곳
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = height-1; 0 <= y;  y--)
            {
                Point p = new Point(x, y);
                Node node = GetNodePoint(p);
                int val = GetValuePoint(p);

                if (val != 0) // 빈칸이 아니라면 아무것도 안함
                    continue;

                // 현재 위치에서 위쪽으로 빈칸이 발견 되면
                // (y값이 커지면 내려가는거라 거꾸로 체크해야함)
                for(int ny = (y-1); -1 <= ny; ny--)
                {
                    Point np = new Point(x, ny);
                    int nval = GetValuePoint(np);

                    // 연속 빈칸이면
                    if (nval == 0)
                        continue;

                    if (nval != -1) // 위에 있는 애들은 빈칸만큼 땡겨서 내려줌
                    {
                        Node nnode = GetNodePoint(np);
                        NodeJelly nj = nnode.GetNode();

                        node.SetNode(nj);
                        nodeUpdate.Add(nj);

                        nnode.SetNode(null);
                    }
                    else // 끝을 쳤을 때
                    {
                        //Debug.Log("새로운 프리팹 생성");
                        int newvalue = InitNodeVal();

                        NodeJelly nj;

                        if(dead.Count > 0)
                        {
                            NodeJelly newJelly = dead[0];   // 이름 바꾸기
                            newJelly.gameObject.SetActive(true);
                            newJelly.rect.anchoredPosition = GetPosFromPoint(new Point(x, -1 - refill[x]));
                            nj = newJelly;

                            dead.RemoveAt(0);
                        }
                        else
                        {
                            GameObject gameObject = Instantiate(nodeJelly, gamePanel);
                            NodeJelly newJelly = gameObject.GetComponent<NodeJelly>();

                            RectTransform rect = gameObject.GetComponent<RectTransform>();
                            rect.anchoredPosition = GetPosFromPoint(new Point(x, -1 - refill[x]));

                            nj = newJelly;
                        }

                        nj.Initialize(newvalue, p, jelly[newvalue - 1]);

                        Node hole = GetNodePoint(p);
                        hole.SetNode(nj);
                        ResetNode(nj);
                        refill[x]++;

                    }
                    break;
                }
            }
        }
    }

    public void FlipNode(Point p1, Point p2, bool flag) // 두 개의 위치를 서로 바꾸는 곳
    {
        if (GetValuePoint(p1) < 0)
            return;

        Node n1 = GetNodePoint(p1);
        NodeJelly nj1 = n1.GetNode();

        // 서로 위치 바꾸기 유효한 위치에 있다면
        if (GetValuePoint(p2) > 0)  
        {
            Node n2 = GetNodePoint(p2); // 위치로 노드 정보 가져오기
            NodeJelly nj2 = n2.GetNode();

            n1.SetNode(nj2);        // 노드1에 프리팹2 정보로 덮어쓰기
            n2.SetNode(nj1);        // 노드2에 프리팹1 정보로 덮어쓰기 
                
            /*nj1.flip = nj2;
            nj2.flip = nj1;*/

            // 처음 진입 시 flip에 노드 추가
            if(flag)
                flip.Add(new FlipNodeJelly(nj1, nj2));

            // 위치 변경 시작
            nodeUpdate.Add(nj1);
            nodeUpdate.Add(nj2);
        }
        // 아님 말고
        else
            ResetNode(nj1);
    }

    FlipNodeJelly GetFlip(NodeJelly nodejelly)
    {
        FlipNodeJelly flipjelly = null;
        for (int i = 0; i < flip.Count; i++)
        {
            // flip 안에 유효한 노드가 2개 정확히 있으면
            if (flip[i].GetOtherNode(nodejelly) != null)
            {
                flipjelly = flip[i]; // 해당 노드들이 들어있는 flipjelly를 리턴
                break;
            }
        }
        return flipjelly;
    }

    public void StraightNode(Point p, Point d)  // 슈퍼노드 동작 하는 곳
    {
        if (GetValuePoint(p) < 0)
            return;

        GoalCount();

        // 슈퍼노드가 굴러 가야하는 방향과 목적지 구멍구하고
        Point direction = new Point(d.x - p.x, d.y - p.y);
        //Debug.Log(direction.x + " " + direction.y);

        Node node = GetNodePoint(p);
        NodeJelly superJelly = node.GetNode();

        Point endPoint = Point.Zero;
        if (direction.x == 1 || direction.x == -1) // 좌우 이동
        {
            direction = Point.Multiple(direction, 8);
            endPoint = Point.Add(node.index, direction);
            endPoint = new Point(Mathf.Clamp(endPoint.x, 0, 8), endPoint.y);

            // 지나가는 경로에 있는 노드 추가
            if (endPoint.x == 0)
            {
                for (int i = 1; i < p.x; i++)
                {
                    Node n = GetNodePoint(new Point(i, endPoint.y));
                    NodeJelly nj = n.GetNode();
                    straight.Add(nj);
                }
                straight.Reverse();
                straight.Add(superJelly);
            }
            else
            {
                for (int i = p.x + 1; i < endPoint.x; i++)
                {
                    Node n = GetNodePoint(new Point(i, endPoint.y));
                    NodeJelly nj = n.GetNode();
                    straight.Add(nj);
                }
                straight.Add(superJelly);
            }
        }
        else if(direction.y == 1 || direction.y == -1) // 상하 이동
        {
            direction = Point.Multiple(direction, 8);
            endPoint = Point.Add(node.index, direction);
            endPoint = new Point(endPoint.x, Mathf.Clamp(endPoint.y, 0, 8));

            // 지나가는 경로에 있는 노드 추가
            if (endPoint.y == 0)
            {
                for (int i = 1; i < p.y; i++)
                {
                    Node n = GetNodePoint(new Point(endPoint.x, i));
                    NodeJelly nj = n.GetNode();
                    straight.Add(nj);
                }
                straight.Reverse();
                straight.Add(superJelly);

            }
            else
            {
                for (int i = p.y + 1; i < endPoint.y; i++)
                {
                    Node n = GetNodePoint(new Point(endPoint.x, i));
                    NodeJelly nj = n.GetNode();
                    straight.Add(nj);
                }
                straight.Add(superJelly);

            }
        }

        // 가야할 곳 입력
        Vector2 pos = GetPosFromPoint(endPoint);
        superJelly.pos = pos;

        nodeUpdate.Add(superJelly);

        // 지나가는 경로에 있는 거 순서대로 끄기
        StartCoroutine(Nodeoff());
    }

    IEnumerator Nodeoff()
    {
        foreach(NodeJelly p in straight)
        {
            p.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.15f);
        }
    }

    public void ResetNode(NodeJelly node)
    {
        //Debug.Log("2 : " + node.index.x + node.index.y);

        node.ResetPos();        // 노드 pos 초기화
        nodeUpdate.Add(node);   // 위치 업데이트 해야 하는 노드 추가
    }

    void MoveCount(bool flag)
    {
        // 남은 횟수 한번만 깎이고 다음 움직임을 기다림 - 연속적으로 안 깎이게
        if (flag)
        {
            // 게임 종료 조건 달성 여부
            tryText.text = (int.Parse(tryText.text) - 1).ToString();
            if (tryText.text.Equals("0"))
            {
                endPanel.SetActive(true);
                // 게임 리플레이 ui 생성
                Debug.Log("게임 종료 (실패)");
            }
            isCount = false;
        }
    }

    void GoalCount()
    {
        goalText.text = (int.Parse(goalText.text) - 1).ToString();
        if (goalText.text.Equals("0"))
        {
            endPanel.SetActive(true);
            endText.text = "게임 클리어";
            // 게임 리플레이 ui 생성
            Debug.Log("게임 종료 (성공)");
        }
    }


    // ----------------- 먼가 값을 불러와야 할 때 쓰는것 들 *--------------------------

    public Vector2 GetPosFromPoint(Point p) // point위치로 실제 postion 값 나오는 곳 
    {
        return new Vector2((64 * p.x), -(64 * p.y));
    }


    Node GetNodePoint(Point p) // 위치로 해당 Node를 부르는 곳
    {
        return gameBoard[p.x, p.y];
    }

    public void RetryBtn()
    {
        //  재시작은 버튼 만들어서 게임 종료시 실행
        SceneManager.LoadScene(0);
    }

    #endregion

}

[System.Serializable]
public class Node   // 노드 젤리 이미지 값 + 위치 정보  
{
    public int value; // -2 : 안쓰는 칸, -1 : 구멍,  0 : 빈칸, 1 : 빨강, 2 : 노랑, 3 : 초록, 4 : 보라, 5 : 굴러가는거
    public Point index;
    NodeJelly nodeJelly;

    public Node(int value, Point index)
    {
        this.value = value;
        this.index = index;
    }

    public NodeJelly GetNode()
    {
        return nodeJelly;
    }
    public void SetNode(NodeJelly n) // 노드와 연결되있는 프리팹 정보 넣는 곳
    {
        //Debug.Log("2");
        nodeJelly = n;
        value = (nodeJelly == null) ? 0 : nodeJelly.value;
        if (nodeJelly == null) 
            return;

        nodeJelly.SetJellyInfo(index);
    }
}

[System.Serializable]
public class FlipNodeJelly  
{
    public NodeJelly nj1;
    public NodeJelly nj2;

    public FlipNodeJelly(NodeJelly nj1, NodeJelly nj2)
    {
        this.nj1 = nj1;
        this.nj2 = nj2;
    }

    public NodeJelly GetOtherNode(NodeJelly nj)
    {
        if (nj == nj1)
            return nj2;
        else if (nj == nj2)
            return nj1;
        else
            return null;
    }
}