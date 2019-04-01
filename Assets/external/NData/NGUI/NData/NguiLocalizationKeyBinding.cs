using UnityEngine;

[System.Serializable]
public class NguiLocalizationKeyBinding : NguiTextBinding
{
	private UILocalize _localize;
	
	public override void Awake()
	{
		base.Awake();
		
		_localize = GetComponent<UILocalize>();
        if(_localize == null)
            _localize = gameObject.AddComponent<UILocalize>();
	}
	
	protected override void ApplyNewValue (string newValue)
	{
		_localize.key = newValue;
#if NGUI_2
		_localize.Localize();
#endif
	}
}
