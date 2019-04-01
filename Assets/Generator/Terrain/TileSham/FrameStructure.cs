using UnityEngine;
using System.Collections;

public struct FrameStructure {
    public string TileCollection;
    public int FrameX, FrameY;
    public int DeltaHeight;
    public FrameStructure(int frameX, int frameY) {
        FrameX = frameX;
        FrameY = frameY;
        TileCollection = "";
        DeltaHeight = 0;
    }

    public FrameStructure(int frameX, int frameY, string tileColl) {
        FrameX = frameX;
        FrameY = frameY;
        TileCollection = tileColl;
        DeltaHeight = 0;
    }


    static FrameStructure empty = new FrameStructure(1, 1, "empty"); 
    public static FrameStructure Empty() {
        return empty;
    }

    public override string ToString() {
        return string.Format("x = {0} y = {1}", FrameX, FrameY);
    }
}

