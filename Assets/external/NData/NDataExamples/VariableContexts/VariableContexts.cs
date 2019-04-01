using UnityEngine;

public class ShopItem2 : EZData.Context
{
    #region Property Name
    private readonly EZData.Property<string> _privateNameProperty = new EZData.Property<string>();
    public EZData.Property<string> NameProperty { get { return _privateNameProperty; } }
    public string Name
    {
    get    { return NameProperty.GetValue();    }
    set    { NameProperty.SetValue(value); }
    }
    #endregion
    
    #region Property Price
    private readonly EZData.Property<int> _privatePriceProperty = new EZData.Property<int>();
    public EZData.Property<int> PriceProperty { get { return _privatePriceProperty; } }
    public int Price
    {
    get    { return PriceProperty.GetValue();    }
    set    { PriceProperty.SetValue(value); }
    }
    #endregion
}

public class ShopContext1 : EZData.Context
{
	public ShopItem2 StaticFeaturedItem { get; set; }
	
	#region DynamicFeaturedItem
	public readonly EZData.VariableContext<ShopItem2> DynamicFeaturedItemEzVariableContext =
		new EZData.VariableContext<ShopItem2>(null);
	public ShopItem2 DynamicFeaturedItem
	{
	    get { return DynamicFeaturedItemEzVariableContext.Value; }
	    set { DynamicFeaturedItemEzVariableContext.Value = value; }
	}
	#endregion
	
	private ShopItem2 [] _items;
    
    public ShopContext1()
    {
        _items = new []
        {
            new ShopItem2 { Name = "Boots of Speed", Price = 450 },
            new ShopItem2 { Name = "Power Treads", Price = 1400 },
            new ShopItem2 { Name = "Phase Boots", Price = 1350 },
            new ShopItem2 { Name = "Tranquil Boots", Price = 975 },
            new ShopItem2 { Name = "Boots of Travel", Price = 2450 },
            new ShopItem2 { Name = "Arcane Boots", Price = 1450 },
        };
        
		StaticFeaturedItem = _items[0];
		DynamicFeaturedItem = _items[0];
	}
    
    public void FeatureRandomItem()
    {
		ShopItem2 randomItem = null;
		while (randomItem == null || randomItem == StaticFeaturedItem)
		{
			randomItem = _items[Random.Range(0, _items.Length - 1)];
		}
		
		// Both sub-contexts are assigned here,
		// but only dynamic one will trigger the UI update.
        StaticFeaturedItem = randomItem;
		DynamicFeaturedItem = randomItem;
    }
}

public class VariableContexts : MonoBehaviour
{
	public NguiRootContext View;
	public ShopContext1 Context;
	
	void Awake()
	{
		Context = new ShopContext1();
		View.SetContext(Context);
	}
}
