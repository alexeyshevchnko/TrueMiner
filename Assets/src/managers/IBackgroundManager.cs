using UnityEngine;
using System.Collections;

namespace Managers {

    public interface IBackgroundManager {
        void Init();
        void SetView(BackgroundView view);
        void FixedUpdate();
        void SetTime(float v);
    }

}

