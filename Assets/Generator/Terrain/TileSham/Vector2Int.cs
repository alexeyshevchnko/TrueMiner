using UnityEngine;
using System.Collections;


public struct Vector2Int {

    public int x, y;

    public Vector2Int(int x, int y) {
        this.x = x;
        this.y = y;
    }


    public Vector2 GetVector2() {
        return  new Vector2(x,y);
    }

    public override string ToString() {
        return string.Format("x = {0}, y = {1}",x,y);
    }

    public bool IsNull() {
        return x == int.MinValue && y == int.MinValue;
    }

    public static Vector2Int GetNull() {
        return new Vector2Int(int.MinValue, int.MinValue);
    }

    //left, right, top, bottom
    public Vector2Int Left() {
        return new Vector2Int(x - 1, y);
    }

    public Vector2Int Right() {
        return new Vector2Int(x + 1, y);
    }

    public Vector2Int Top() {
        return new Vector2Int(x , y + 1);
    }

    public Vector2Int Bottom() {
        return new Vector2Int(x, y - 1);
    }

    public bool IsEqual(Vector2Int val) {
        return x == val.x && y == val.y;
    }


    public override bool Equals(object obj) {
        if (!(obj is Vector2Int))
            return false;
        return ((Vector2Int) obj).IsEqual(this);

    }


    public static bool operator ==(Vector2Int c1, Vector2Int c2) {
        return c1.Equals(c2);
    }

    public static bool operator !=(Vector2Int c1, Vector2Int c2) {
        return !c1.Equals(c2);
    }

}
