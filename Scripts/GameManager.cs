using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    #region ����
    public Board board; // ���� ��ü ���� 

    [Header("UI")]
    public Sprite[] jelly;  // ���� �̹���
    public Sprite hole;     // ���� �̹���
    public Sprite superNode;// Ư�� ��� �̹���

    public RectTransform gamePanel; // ���� �г� ��ġ ���� 

    [Header("������")]
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
    Node[,] gameBoard;  // ���忡 �� ��ü 

    int[] refill;

    List<NodeJelly> nodeUpdate; // ��ġ ������Ʈ�ؾ� �ϴ� ��� ���� ��
    List<FlipNodeJelly> flip;   // ���� ��ġ �ٲ���ϴ� ��� 2�� ���� ��
    List<NodeJelly> straight; // ���۳�� ������ ������ ��� ���� ��
    List<NodeJelly> dead; // ��Ī�Ŀ� ������ ��� ���� ��

    List<NodeJelly> finishingUpdate; // ��ġ ������Ʈ ���� ������ ���� ��

    bool isSquare;  // 2*2 �����ΰ�?

    [HideInInspector]
    public bool isCount; // ���� Ƚ�� ���� �ؾ��� ��

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

        timer = 0.0f;   // ������Ʈ ���� Ÿ�̸�
        wait = 0.9f;

        finishingUpdate = new List<NodeJelly>();

        isSquare = false;
    }


    void Update()
    {
        timer += Time.deltaTime;

        for (int i = 0; i < nodeUpdate.Count; i++)
        {
            // ��ġ�� �� ������ �Ŀ�
            // false ������ finishingUpdate�� �Ѱܼ� ���� ������ ��
            if (!nodeUpdate[i].UpdateNode())
            {
                finishingUpdate.Add(nodeUpdate[i]);
            }
        }
        if (timer > wait)
        {
            //Debug.Log(finishingUpdate.Count);

            finishingUpdate = finishingUpdate.Distinct().ToList();

            // �����̸� list���� üũ �ϰ� ����
            for (int i = 0; i < finishingUpdate.Count; i++)
            {
                // node�� �ٲ�� ��� flipnode�� ���� ���
                NodeJelly node = finishingUpdate[i];
                FlipNodeJelly flipjelly = GetFlip(node); // flip list�� ��ȿ�� ���� ����ִٸ� ���� ������
                NodeJelly flipNode = null;

                int x = (int)node.index.x;
                refill[x] = Mathf.Clamp(refill[x] - 1, 0, width);

                // ------------------------ ��Ī�� ��� �����ϴ� �� --------------------------
                // ������ ������ ����� �� �ִ��� Ȯ�� ����
                List<Point> connected = IsConnected(node.index, true);           // x        x
                bool isFlip = (flipjelly != null);                               // x    ->  x
                                                                                 // oxoo     xooo �˻�

                // flip �Ҽ� �ִٸ�
                if (isFlip)
                {
                    flipNode = flipjelly.GetOtherNode(node);
                    // flipnode �� ��ġ �ٲ������ ����� �� �ִ� �� Ȯ�� �ϱ�
                    AddPoints(ref connected, IsConnected(flipNode.index, true));

                    timer = 0;
                    //Debug.Log("�ø� ����");
                    //Debug.Log(node.index.x + " " + node.index.y);
                    //Debug.Log(flipNode.index.x + " " + flipNode.index.y);
                }

                // straight list�� �߰��� ���� �ִٸ�
                
                if(straight.Count != 0)
                {
                    MoveCount(isCount);
                    foreach (NodeJelly nj in straight)
                    {
                        Point p = nj.index;
                        Node n = GetNodePoint(p);           // ��ġ�� ��� ã�Ƽ�
                        NodeJelly nodeJelly = n.GetNode();  // ��忡�� ������ ã��
                        if (nodeJelly != null)
                        {
                            //nodeJelly.gameObject.SetActive(false);  // ��Ȱ��ȭ

                            dead.Add(nodeJelly);                //��Ī�Ŀ� ������ ���� ��Ƴ���
                        }
                        n.SetNode(null);                    // �ʱ�ȭ
                    }
                    FillBlankNode();                    // ��� �ִ� ĭ ��尡 �������鼭 ä���
                }
                straight.Clear();    

                // �ٲ�µ� �ϳ��� ��Ī �Ǵ°� ���ٸ�
                if (connected.Count == 0)
                {
                    // �ٽ� ������� ��������
                    if (isFlip)
                    {
                        FlipNode(node.index, flipNode.index, false);
                        blockPanel.SetActive(false);                    // ��尡 ����Ǵ� ���� ������ ����
                    }
                }
                // ��Ī �Ǵ°� �ִٸ�
                else
                {
                    MoveCount(isCount);

                    // ��Ī�� ��� �����ϴ� ��
                    foreach (Point p in connected)
                    {
                        //Debug.Log("����");
                        //Debug.Log(p.x + " " + p.y);

                        Node n = GetNodePoint(p);           // ��ġ�� ��� ã�Ƽ�
                        NodeJelly nodeJelly = n.GetNode();  // ��忡�� ������ ã��
                        if (nodeJelly != null)
                        {
                            nodeJelly.gameObject.SetActive(false);  // ��Ȱ��ȭ

                            dead.Add(nodeJelly);                //��Ī�Ŀ� ������ ���� ��Ƴ���
                        }
                        n.SetNode(null);                    // �ʱ�ȭ
                    }

                    // node ��ġ�� Ư�� �� ����
                    if (isSquare)
                    {
                        Point p = new Point(node.index.x, node.index.y);
                        //Debug.Log("Ư�� ���� �غ�");

                        // Ư�� ��� �ϳ� �����
                        Node superNode = GetNodePoint(p);
                        //Debug.Log(superNode.index.x + " " + superNode.index.y);

                        // �ȿ� ���� ������ �����
                        NodeJelly superJelly = null;

                        // ����� ��� �߿� �´� ��ġ ã�Ƽ�
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
                        // �ʱ�ȭ ���ְ�
                        superJelly.Initialize(5, p, this.superNode);
                        // ��忡 ���� �ְ�
                        superNode.SetNode(superJelly);
                        // �����ְ�
                        superJelly.gameObject.SetActive(true);

                        ResetNode(superJelly);

                        isSquare = false;
                    }

                    FillBlankNode();                    // ��� �ִ� ĭ ��尡 �������鼭 ä���
                    
                }
                flip.Remove(flipjelly);                 // �������� list���� ����
                nodeUpdate.Remove(node);                // ������Ʈ�Ѱ� list���� ����

                // ��尡 ����Ǵ� ���� ������ ����
                if (nodeUpdate.Count == 0)
                    blockPanel.SetActive(false);
            }
            timer = 0;
        }
        finishingUpdate.Clear();
    }

    #endregion

    #region basic logic method (���ư��� ������)

    void InitBoard()    // ���� ���� ���� ���� �ʱ�ȭ
    {
        gameBoard = new Node[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // �����¿� �׵θ��� ����
                if(y == 0 || x == 0 || y == 8 || x == 8)
                {
                    // �𼭸��� �Ⱦ��� ĭ
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
                    // �� ó�� board�� ���� false ��
                    gameBoard[x, y] = new Node((board.rows[x].row[y]) ? -1 : InitNodeVal(), new Point(x, y));
                }
            }
        }
    }

    int InitNodeVal() // ��忡 ���� �� �ο�
    {
        int num;
        num = rand.Next(1, 5); // ���� ���� 1 - 4����
        return num;
    }

    void VerifyBoard() // ���� ���� 
    {
        List<int> remove = new List<int>(); // ��Ī�Ǽ� ���־��ϴ� ��� ����Ʈ
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
                while (IsConnected(p, true).Count > 0)   // connected list�� �߰����ֳ� Ȯ�� (��Ī�� �ȵǸ� 0)
                {
                    val = GetValuePoint(p);
                    if (!remove.Contains(val))          // remove list�� �˻��� ��� �־��ְ�
                        remove.Add(val);

                    SetValuePoint(p, ReValue(ref remove)); // �� �� �־��ְ� �ٽ� �˻�


                    // ���� ���� �ӽ� ����
                    temp--;
                    if (temp == 0)
                        break;
                }
            }
        }
    }

    int GetValuePoint(Point p)  // ���� ����Ʈ���� value�� �������µ� ���� ���̸� -1
    {
        if (p.x < 0 || p.x >= width || p.y < 0 || p.y >= height)
            return -1;
        return gameBoard[p.x, p.y].value;
    }

    void SetValuePoint(Point p, int val)    // ���� ����Ʈ�� value�� �Ҵ�
    {
        gameBoard[p.x, p.y].value = val;
    }

    int ReValue(ref List<int> remove) // �� ��ġ�� val �� �ٽ� �ִ� ��
    {
        List<int> avail = new List<int>();
        for (int i = 0; i < jelly.Length; i++) // ���� ���� ��ŭ �ְ�
            avail.Add(i + 1);

        foreach (int r in remove)           // ��ġ�� val�� ã�Ƽ� �����ְ�
            avail.Remove(r);

        if (avail.Count <= 0)
            return 0;

        return avail[rand.Next(0, avail.Count)];    // �� �߿��� �������� ����
    }

    List<Point> IsConnected(Point p, bool overlap)  // �˻� ��尡 3��ġ�� �Ǿ����� Ȯ���ϴ� ��
    {
        List<Point> connected = new List<Point>();  // 3��Ī �ϼ��� ����Ʈ ����Ʈ�� �ִ°�
        int val = GetValuePoint(p);                 // �˻� ���� ����� ������� ��
        Point[] dir = { // �˻� ���� �� �� �� �� 
            Point.Up,
            Point.Right,
            Point.Down,
            Point.Left
        };

        foreach (Point d in dir) // ���� �������� ���� ��� 2�� �̻� üũ 
        {
            List<Point> line = new List<Point>();

            int same = 0;
            for (int i = 1; i < 3; i++)
            {
                Point check = Point.Add(p, Point.Multiple(d, i));

                //Debug.Log(p.x + " " + p.y);
                //Debug.Log(check.x + " " + check.y);

                // ���� ��������� Ư������ �𿩵� �������� �Ѵ�
                if (GetValuePoint(check) == val && GetValuePoint(check) != 5)
                {
                    //Debug.Log("same");
                    line.Add(check);
                    same++;
                }
            }

            // üũ�ϴ� �������� ���� ��� 2�� �̻� ������ 3��Ī
            if (same > 1)
            {
                AddPoints(ref connected, line); // connected list�� �߰�
            }
        }

        for (int i = 0; i < 2; i++) // üũ�ϴ� ��� ���ʿ� ���� ����� 2�� �̻� �ִ��� Ȯ��
        {
            List<Point> line = new List<Point>();
            int same = 0;
            Point next = Point.Add(p, dir[i]);          // �� | ��
            Point next2 = Point.Add(p, dir[i + 2]);     // �� | �� �� Ȯ��
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

            // üũ�ϴ� �������� ���� ��� 2�� �̻� ������ 3��Ī
            if (same > 1)
            {
                AddPoints(ref connected, line); // connected list�� �߰�
            }
        }

        for (int i = 0; i < 4; i++) // 2*2 ��� üũ - ���� ���鼭 4���� Ȯ��
        {
            List<Point> square = new List<Point>();
            int same = 0;
            int next = i + 1;
            if (next >= 4)
                next -= 4;

            // ���� ���� ��ó �� ĭ �׸��� �밢�� �� ĭ 
            Point[] check = { Point.Add(p, dir[i]), Point.Add(p, dir[next]), Point.Add(p, Point.Add(dir[i], dir[next])) };

            foreach (Point c in check)
            {
                if (GetValuePoint(c) == val && GetValuePoint(c) != 5)
                {
                    square.Add(c);
                    same++;
                }
            }

            // üũ�ؼ� ���� ��� 3���� 2*2��Ī (1����)
            if (same > 2)
            {
                AddPoints(ref connected, square);
                Debug.Log("2*2 Ž��");
                /*Debug.Log(p.x + " " + p.y);
                foreach (Point c in check)
                    Debug.Log(c.x + " " + c.y);*/
                if(!overlap)
                    isSquare = true;
            }
        }

        // �ߺ� �˻�
        // ó�� ������ isConncedted ��� ���ݱ��� ���� connected list ��
        //  connected list�� ���Ե� ��带 �˻���  list�� ���ļ� ���� val ���� üũ �� - ���ڳ� �� ��� üũ�ϱ� ����
        if (overlap)
        {
            for (int i = 0; i < connected.Count; i++)
            {
                AddPoints(ref connected, IsConnected(connected[i], false));
            }
        }

        return connected;
    }

    void AddPoints(ref List<Point> connect, List<Point> add) // 3��ġ�� �� ��带 connected list�� �ִ� ��
    {
        // connect��  List<Point> connected�� �ǹ��� 
        foreach (Point p in add)
        {
            bool flag = true;

            for (int i = 0; i < connect.Count; i++)
            {
                //Debug.Log(points[i]);
                // ���� ���� �߰� ���Ѵ�.
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

    void InstBoard() // ���� ������ ���� ������ ����
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Node node = GetNodePoint(new Point(x, y));
                int val = node.value;

                if (val == 0 || val == -2)
                    continue;
                
                // ��� ���� ���� - ��ġ�� ����� ����
                GameObject gameObject = Instantiate(nodeJelly, gamePanel);

                NodeJelly nJelly = gameObject.GetComponent<NodeJelly>();

                if(val == -1)
                {
                    nJelly.Initialize(val, node.index, hole);     // ����
                }
                else if (val == 5)
                {
                    nJelly.Initialize(val, node.index, superNode);     // ���۳��
                }
                else
                {
                    nJelly.Initialize(val, node.index, jelly[val - 1]);     // ��� ���� ��� ��ġ ���� �ʱ�ȭ
                }
                node.SetNode(nJelly);                                   // ��忡 ������ ���� �߰�

                RectTransform rect = gameObject.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2((64 * x), -(64 * y));
            }
        }
    }

    #endregion

    #region sub methods

    void FillBlankNode() // ��� �ִ� ĭ ��尡 �������鼭 ä��� �ϴ� ��
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = height-1; 0 <= y;  y--)
            {
                Point p = new Point(x, y);
                Node node = GetNodePoint(p);
                int val = GetValuePoint(p);

                if (val != 0) // ��ĭ�� �ƴ϶�� �ƹ��͵� ����
                    continue;

                // ���� ��ġ���� �������� ��ĭ�� �߰� �Ǹ�
                // (y���� Ŀ���� �������°Ŷ� �Ųٷ� üũ�ؾ���)
                for(int ny = (y-1); -1 <= ny; ny--)
                {
                    Point np = new Point(x, ny);
                    int nval = GetValuePoint(np);

                    // ���� ��ĭ�̸�
                    if (nval == 0)
                        continue;

                    if (nval != -1) // ���� �ִ� �ֵ��� ��ĭ��ŭ ���ܼ� ������
                    {
                        Node nnode = GetNodePoint(np);
                        NodeJelly nj = nnode.GetNode();

                        node.SetNode(nj);
                        nodeUpdate.Add(nj);

                        nnode.SetNode(null);
                    }
                    else // ���� ���� ��
                    {
                        //Debug.Log("���ο� ������ ����");
                        int newvalue = InitNodeVal();

                        NodeJelly nj;

                        if(dead.Count > 0)
                        {
                            NodeJelly newJelly = dead[0];   // �̸� �ٲٱ�
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

    public void FlipNode(Point p1, Point p2, bool flag) // �� ���� ��ġ�� ���� �ٲٴ� ��
    {
        if (GetValuePoint(p1) < 0)
            return;

        Node n1 = GetNodePoint(p1);
        NodeJelly nj1 = n1.GetNode();

        // ���� ��ġ �ٲٱ� ��ȿ�� ��ġ�� �ִٸ�
        if (GetValuePoint(p2) > 0)  
        {
            Node n2 = GetNodePoint(p2); // ��ġ�� ��� ���� ��������
            NodeJelly nj2 = n2.GetNode();

            n1.SetNode(nj2);        // ���1�� ������2 ������ �����
            n2.SetNode(nj1);        // ���2�� ������1 ������ ����� 
                
            /*nj1.flip = nj2;
            nj2.flip = nj1;*/

            // ó�� ���� �� flip�� ��� �߰�
            if(flag)
                flip.Add(new FlipNodeJelly(nj1, nj2));

            // ��ġ ���� ����
            nodeUpdate.Add(nj1);
            nodeUpdate.Add(nj2);
        }
        // �ƴ� ����
        else
            ResetNode(nj1);
    }

    FlipNodeJelly GetFlip(NodeJelly nodejelly)
    {
        FlipNodeJelly flipjelly = null;
        for (int i = 0; i < flip.Count; i++)
        {
            // flip �ȿ� ��ȿ�� ��尡 2�� ��Ȯ�� ������
            if (flip[i].GetOtherNode(nodejelly) != null)
            {
                flipjelly = flip[i]; // �ش� ������ ����ִ� flipjelly�� ����
                break;
            }
        }
        return flipjelly;
    }

    public void StraightNode(Point p, Point d)  // ���۳�� ���� �ϴ� ��
    {
        if (GetValuePoint(p) < 0)
            return;

        GoalCount();

        // ���۳�尡 ���� �����ϴ� ����� ������ ���۱��ϰ�
        Point direction = new Point(d.x - p.x, d.y - p.y);
        //Debug.Log(direction.x + " " + direction.y);

        Node node = GetNodePoint(p);
        NodeJelly superJelly = node.GetNode();

        Point endPoint = Point.Zero;
        if (direction.x == 1 || direction.x == -1) // �¿� �̵�
        {
            direction = Point.Multiple(direction, 8);
            endPoint = Point.Add(node.index, direction);
            endPoint = new Point(Mathf.Clamp(endPoint.x, 0, 8), endPoint.y);

            // �������� ��ο� �ִ� ��� �߰�
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
        else if(direction.y == 1 || direction.y == -1) // ���� �̵�
        {
            direction = Point.Multiple(direction, 8);
            endPoint = Point.Add(node.index, direction);
            endPoint = new Point(endPoint.x, Mathf.Clamp(endPoint.y, 0, 8));

            // �������� ��ο� �ִ� ��� �߰�
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

        // ������ �� �Է�
        Vector2 pos = GetPosFromPoint(endPoint);
        superJelly.pos = pos;

        nodeUpdate.Add(superJelly);

        // �������� ��ο� �ִ� �� ������� ����
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

        node.ResetPos();        // ��� pos �ʱ�ȭ
        nodeUpdate.Add(node);   // ��ġ ������Ʈ �ؾ� �ϴ� ��� �߰�
    }

    void MoveCount(bool flag)
    {
        // ���� Ƚ�� �ѹ��� ���̰� ���� �������� ��ٸ� - ���������� �� ���̰�
        if (flag)
        {
            // ���� ���� ���� �޼� ����
            tryText.text = (int.Parse(tryText.text) - 1).ToString();
            if (tryText.text.Equals("0"))
            {
                endPanel.SetActive(true);
                // ���� ���÷��� ui ����
                Debug.Log("���� ���� (����)");
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
            endText.text = "���� Ŭ����";
            // ���� ���÷��� ui ����
            Debug.Log("���� ���� (����)");
        }
    }


    // ----------------- �հ� ���� �ҷ��;� �� �� ���°� �� *--------------------------

    public Vector2 GetPosFromPoint(Point p) // point��ġ�� ���� postion �� ������ �� 
    {
        return new Vector2((64 * p.x), -(64 * p.y));
    }


    Node GetNodePoint(Point p) // ��ġ�� �ش� Node�� �θ��� ��
    {
        return gameBoard[p.x, p.y];
    }

    public void RetryBtn()
    {
        //  ������� ��ư ���� ���� ����� ����
        SceneManager.LoadScene(0);
    }

    #endregion

}

[System.Serializable]
public class Node   // ��� ���� �̹��� �� + ��ġ ����  
{
    public int value; // -2 : �Ⱦ��� ĭ, -1 : ����,  0 : ��ĭ, 1 : ����, 2 : ���, 3 : �ʷ�, 4 : ����, 5 : �������°�
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
    public void SetNode(NodeJelly n) // ���� ������ִ� ������ ���� �ִ� ��
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