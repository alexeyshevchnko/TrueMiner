using UnityEngine;
using System.Collections;

namespace Managers {
    public class BackgroundManager : IBackgroundManager {
        private BackgroundView view;

        [IoC.Inject]
        public IMapGenerator MapGenerator { set; protected get; }

        public void SetView(BackgroundView view) {
            this.view = view;
        }

        public void FixedUpdate() {
            UpdatePos();
        }

        public void Init() {
            UpdatePos();
        }

        private void UpdatePos() {
           
            //2250

            var main = view.main.transform;
            var main2 = view.main2.transform;
            var main3 = view.main3.transform;
            var cam = view.camera;

            
            var perfectVal = ((float) (Screen.height))/2f;
            var currVal = cam.orthographicSize;
            var deltaVal = perfectVal/currVal;
           // Debug.LogError(deltaVal);
            
           // var scale = 1;//(Screen.width / (1024f));

            if (view.main.size.x != Screen.width)
                view.main.size = new Vector2(Screen.width , view.main.size.y);

            if (view.main2.size.x != Screen.width )
                view.main2.size = new Vector2(Screen.width , view.main2.size.y);

            if (view.main3.size.x != Screen.width || view.main3.size.y != Screen.height)
                view.main3.size = new Vector2(Screen.width, Screen.height);


            

            var world = cam.ViewportToWorldPoint(new Vector3(0.5f, 0));


            main.position = view.camera.transform.position;
            main2.position = view.camera.transform.position;
            view.main3.transform.position = world;



           // var deltaY = (MapGenerator.surfaceMinY * 16 - 100) - cam.transform.position.y;
            var deltaY = (MapGenerator.GetSurfaceMinY * 16 - 10) - view.camera.transform.position.y;
         //   Debug.LogError(deltaY);



            //Debug.LogError(main.localPosition.y + deltaY / (2));

            main.localPosition = new Vector3(main.localPosition.x,
                main.localPosition.y + deltaY  / (2 ), 5);

            main2.localPosition = new Vector3(main2.localPosition.x,
                main2.localPosition.y + deltaY /(1.5f), 1);

            main3.localPosition = new Vector3(main3.localPosition.x,
                main3.localPosition.y, 10);



            var x = cam.transform.position.x;
            view.main.SetOffset(new Vector2(-x / 10000, 0));
            view.main2.SetOffset(new Vector2(-x / 5000, 0));
          
        }


        public void SetTime(float v) {
            //Debug.LogError(v);
            Color newColor = Color.Lerp(view.min, view.max, 1 - v);
            view.main.color = newColor;
            view.main2.color = newColor;
            view.main3.color = newColor;

        }
    }
}