using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BufferList  {
    List<BufferItem>  Items = new List<BufferItem>();
    private int indexBuffer;
    public BufferList(int size = 100)
    {
        for (int i = 0; i < size; i++)
        {
            Items.Add(new BufferItem(size));
        }
        indexBuffer = 0;
    }

    public void NextBuffer()
    {
        indexBuffer ++;
        if (indexBuffer > Items.Count - 1)
        {
            indexBuffer = 0;
        }
        Items[indexBuffer].Reset();
    }


    public void Add(Vector2Int val)
    {
        Items[indexBuffer].Add(val);
    }

    public Vector2Int this[int key]
    {
        get
        {
            return Items[indexBuffer][key];
        }
        set
        {
            Items[indexBuffer][key] = value;
        }
    }

    public int Count
    {
        get { return Items[indexBuffer].Count; }
    }

    public BufferItem GetCurrent
    {
        get { return Items[indexBuffer]; }
    }
}


public class BufferItem
{
    public List<Vector2Int> Items = new List<Vector2Int>();
    public int Count = 0;
    public BufferItem(int size = 100)
    {
        for (int i = 0; i < 100; i++)
        {
            Items.Add(new Vector2Int());
        }
        Count = 0;
    }

    public void Reset()
    {
        Count = 0;
    }


    public void Add(Vector2Int val)
    {
        Items[Count] = val;
        Count++;
    }


    public Vector2Int this[int key]
    {
        get { return Items[key]; }
        set { Items[key] = value; }
    }
}
