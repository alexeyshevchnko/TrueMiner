using UnityEngine;
using System.Collections;

// Moved here unchanged from LightRendererOld.cs, which was removed as dead code.
// LightPoint is still constructed by PlayerController and ExplosionShell.
public class LightPoint {
    private static int Id;
    private int id;
    public LightPoint() {
        id = Id++;
    }

    public int GetId() {
        return id;
    }

    public Vector2 Pos = Vector2.zero;
    public float Size = -1;

    public Vector2 LastPos;
    public float LastSize;
    public bool isUpdate = true;
    public void SetPosition(Vector2 pos) {
        if (Pos != pos) {
            isUpdate = true;

            if (Pos != Vector2.zero) {
                LastPos = Pos;
            } else {
                LastPos = pos;
            }

            
            Pos = pos;
        }
    }

    public void SetSize(float size) {
        if (Size != size) {
            isUpdate = true;
            if (Size != -1) {
                LastSize = Size;
            } else {
                LastSize = size;
            }
            Size = size;
        }
    }


}
