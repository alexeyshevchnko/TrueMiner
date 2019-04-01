using UnityEngine;
using System.Collections;

public struct ColorByte {
    public byte R;// { get; set; }
    public byte G;// { get; set; }
    public byte B;// { get; set; }
    public byte A;//{ get; set; }


    public ColorByte(byte r, byte g, byte b) {
        R = r;
        G = g;
        B = b;
        A = 255;
    }

    public ColorByte(byte r, byte g, byte b,byte a) {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public ColorByte(int r, int g, int b, int a) {
        R = (byte)r;
        G = (byte)g;
        B = (byte)b;
        A = (byte)a;
    }

    
    public static ColorByte white = new ColorByte(255, 255, 255, 255);

    
    public static ColorByte whiteTransparent = new ColorByte(255, 255, 255, 0);

    /*
    public static ColorByte white {
        get {
            return new ColorByte(255, 255, 255);
        }
    }*/
}
