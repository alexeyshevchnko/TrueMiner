using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BufferList<T>  {
    private int count = 0;
    private int addCount = 0;

    private List<T> buffer;

    public BufferList(int bufferChange) {
        buffer = new List<T>();
        addCount = bufferChange;
        AddBuffer();
    }

    private void AddBuffer() {
        for (int i = 0; i < addCount; i++) {
            buffer.Add(default(T));
        }
    }

    public void Add(T val) {
        if ((float)count > (float)buffer.Count / 2.0f) {
            AddBuffer();
        }

        buffer[count] = val;  

        count++;
        
    }

    public void Remove(T val) {
        count--;
        for (int i = 0; i < buffer.Count; i++) {
            
            if (Equals(buffer[i], val)) {
                
                for (int j = i + 1; j < buffer.Count; j++) {
                    buffer[j - 1] = buffer[j];
                }

                break;
                
            }
        }
    }

    public T Get(int index) {
        return buffer[index];
    }

    public int Count {
        get { return count; }
    }

}
