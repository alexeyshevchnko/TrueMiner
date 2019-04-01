using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LightInfoStructure {
    public byte R;
    public byte G;
    public byte B;
    public byte A;
    private Dictionary<int, byte> sources = new Dictionary<int, byte>();


    public LightInfoStructure() { }

    public LightInfoStructure(byte r, byte g, byte b, byte a) {
        sources = new Dictionary<int, byte>();
        R = r;
        G = g;
        B = b;
        A = a;
    }

    byte GetMinAlpha() {
        byte rez =  255;     
        
        for (int index = 0; index < sources.Count; index++) {
            var item = sources.ElementAt(index);
            var itemValue = item.Value;

            if (itemValue < rez) {
                rez = itemValue;
            }
        }

        return rez;
    }

    public void SetAlpha(int sourceId, byte alpha) {
        var val = (byte)Mathf.Min(A, alpha);
        sources[sourceId] = val;
        A = val;
    }

    public void IncAlpha(int sourceId, byte alpha) {
        sources[sourceId] = alpha;
        A = GetMinAlpha();
    }

    public void DecAlpha(int sourceId) {
        sources.Remove(sourceId);
        A = GetMinAlpha();
    }

    public void print() {
        Debug.LogError(string.Format("r = {0}, g = {1}, b ={2} a= {3}", R, G, B, A));
    }
}
