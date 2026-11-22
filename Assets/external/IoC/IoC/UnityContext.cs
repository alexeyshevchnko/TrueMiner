using UnityEngine;
using System;

public class ContextMessage
{
	static public readonly string ADDED = "OnAdded";
}

public class UnityContext:MonoBehaviour
{

}

public class UnityContext<T>: UnityContext where T:IContextRoot, new()
{
	virtual protected void Awake()
	{
		Debug.Log("UnityContext Awaked");

		EnsureApplicationRoot();
	}

	//
	// Defining OnEnable as fix for UnityEngine execution order bug
	//
	void OnEnable()
	{
		Debug.Log("UnityContext Enabled");
	}

	void OnAdded(MonoBehaviour component)
	{
		// Если этот Awake ещё не успел выполниться (порядок Awake между объектами сцены
		// не задан), инициализируем корень прямо здесь вместо падения с NullReferenceException.
		// Инъекция не пропускается — компонент получает полностью рабочее окружение.
		EnsureApplicationRoot();

		_applicationRoot.container.Inject(component);
	}

	private void EnsureApplicationRoot()
	{
		// Повторный вызов (OnAdded раньше Awake) не должен создавать второй контейнер:
		// синглтоны из первого контейнера уже могли быть выданы компонентам.
		if (_applicationRoot == null)
		{
			_applicationRoot = new T();
		}
	}

	protected IoC.IContainer container { get { return _applicationRoot.container;} }

	private T 				_applicationRoot;
}