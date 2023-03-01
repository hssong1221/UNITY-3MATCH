using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Point  //노드 위치 정보 클래스
{
    public int x;
    public int y;

    // 생성자
    public Point(int nx, int ny)
    {
        x = nx;
        y = ny;
    }

    /*public void Multiple(int m)
    {
        x *= m;
        y *= m;
    }*/

    public void Add(Point p)
    {
        x += p.x;
        y += p.y;
    }

    public Vector2 ToVector()
    {
        return new Vector2(x, y);
    }


    public bool Equals(Point p)
    {
        return (x == p.x && y == p.y);
    }

    public static Point Multiple(Point p, int m)
    {
        return new Point(p.x * m, p.y * m);
    }

    public static Point Add(Point p1, Point p2)
    {
        return new Point(p1.x + p2.x, p1.y + p2.y);
    }

    public static Point Dup(Point p)
    {
        return new Point(p.x, p.y);
    }

    public static Point Zero
    {
        get { return new Point(0, 0); }
    }
    public static Point Up
    {
        get { return new Point(0, 1); }
    }
    public static Point Down
    {
        get { return new Point(0, -1); }
    }
    public static Point Right
    {
        get { return new Point(1, 0); }
    }
    public static Point Left
    {
        get { return new Point(-1, 0); }
    }
}
