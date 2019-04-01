using System;
using UnityEngine;

namespace IoC
{
	public interface IMonoBehaviourFactory
	{
		M Build<M>(Func<M> constructor) where M:MonoBehaviour;
	}
	
	public class MonoBehaviourFactory: IMonoBehaviourFactory
	{
		[Inject] public IContainer container { set; private get; }
		
		public M Build<M>(Func<M> constructor) where M:MonoBehaviour
		{
			M mb = (M)constructor();
            MonoBehaviour [] components = mb.GetComponents<MonoBehaviour>();
            //инжектим не только текущий объект но и все скрипты что на нем висят
            foreach (MonoBehaviour component in components)
            {
                container.Inject(component);
            }
			return mb;
		}
	}
}

