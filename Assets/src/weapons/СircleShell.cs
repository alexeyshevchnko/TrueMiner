using UnityEngine;
using System.Collections;

public class СircleShell : Shell {

    protected override void FixedUpdate() {
        base.FixedUpdate();

        //подкручивает по направлению когда предмет катится
        if (DeltaPosition.y == 0 && DeltaPosition.x != 0) {
            var circleSize = size.x*Mathf.PI;
            var deltaCircleSize = circleSize * Mathf.Abs(DeltaPosition.x);
            var alpha = (deltaCircleSize*360)/circleSize;
            transform.rotation = Quaternion.AngleAxis(Mathf.Sign(DeltaPosition.x)*alpha,  Vector3.forward);
        }
    }
}
