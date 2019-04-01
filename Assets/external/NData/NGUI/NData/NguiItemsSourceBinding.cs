using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
[AddComponentMenu("NGUI/NData/ItemsSource Binding")]
public class NguiItemsSourceBinding : NguiBinding
{
    protected NguiItemsSourceBinding _parent;
	protected NguiListItemTemplate _itemTemplate;
	protected EZData.Collection _collection;
	protected bool _isCollectionSelecting = false;
	
	private UITable _uiTable = null;
	private UIGrid _uiGrid = null;
	
	protected bool _dirty = false;

    protected void BaseBind() {
        base.Bind();
    }

    protected void BaseUnbind() {
        base.Unbind();
    }

	public override void Awake()
	{
		base.Awake();
		_uiTable = GetComponent<UITable>();
		_uiGrid = GetComponent<UIGrid>();
		_itemTemplate = gameObject.GetComponent<NguiListItemTemplate>();
	}
	
	public override void Start ()
	{
		base.Start ();
		_parent = NguiUtils.GetComponentInParentsExcluding<NguiItemsSourceBinding>(gameObject);
	}
	
	void LateUpdate()
	{
		if (_dirty)
		{
			var topDirty = this;
			var p = _parent;
			while (p != null)
			{
				if (p._dirty)
					topDirty = p;
				p = p._parent;
			}
			topDirty.RepositionHierarchy(this);
		}
	}
	
	private void RepositionHierarchy(NguiItemsSourceBinding node)
	{
		var childSources = node.GetComponentsInChildren<NguiItemsSourceBinding>(true);
		
		foreach (var c in childSources)
		{
			if (c != node && c._dirty)
			{
				RepositionHierarchy(c);
			}
		}
		node.RepositionContent();
	}
	
	private void RepositionContent()
	{
		_dirty = false;
		
		if (_uiTable != null)
		{
			_uiTable.Reposition();
		}
		
		if (_uiGrid != null)
		{
			_uiGrid.Reposition();
		}
	}

    public override void Unbind()
	{
		base.Unbind();
		
		if (_collection != null)
		{
			_collection.OnItemInsert -= OnItemInsert;
			_collection.OnItemRemove -= OnItemRemove;
			_collection.OnItemsClear -= OnItemsClear;
			_collection.OnSelectionChange -= OnCollectionSelectionChange;
			_collection = null;
			OnItemsClear();
		}
	}

    public override void Bind()
	{
		base.Bind();
		
		var context = GetContext(Path);
		if (context == null)
			return;
		
		_collection = context.FindCollection(Path, this);
		if (_collection == null)
			return;
	
		_collection.OnItemInsert += OnItemInsert;
		_collection.OnItemRemove += OnItemRemove;
		_collection.OnItemsClear += OnItemsClear;
		_collection.OnSelectionChange += OnCollectionSelectionChange;
		
		for (var i = 0; i < _collection.ItemsCount; ++i)
		{
			OnItemInsert(i, _collection.GetBaseItem(i));
		}
		OnCollectionSelectionChange();
	}
	
	protected virtual void OnItemInsert(int position, EZData.Context item)
	{
		GameObject itemObject = null;
		if (_itemTemplate != null)
		{
			itemObject = _itemTemplate.Instantiate(item, position);
			
			itemObject.name = string.Format("{0}", position);
			for (var i = 0; i < transform.childCount; ++i)
			{
				var child = transform.GetChild(i).gameObject;
				int childNumber;
				if (int.TryParse(child.name, out childNumber) && childNumber >= position)
				{
					child.name = string.Format("{0}", childNumber + 1);
				}
			}
			itemObject.transform.parent = gameObject.transform;
			itemObject.transform.localScale = Vector3.one;
			itemObject.transform.localPosition = Vector3.back;
		}
		else
		{
			if (position < transform.childCount)
			{
				itemObject = transform.GetChild(position).gameObject;	
				var itemData = itemObject.GetComponent<NguiItemDataContext>();
				if (itemData != null)
				{
					itemData.SetContext(item);
					itemData.SetIndex(position);
				}
			}
		}
		if (itemObject != null)
		{
			foreach(var dragObject in itemObject.GetComponentsInChildren<UIDragObject>())
			{
				if (dragObject.target == null)
					dragObject.target = gameObject.transform;
			}
			foreach(var dragObject in itemObject.GetComponents<UIDragObject>())
			{
				if (dragObject.target == null)
					dragObject.target = gameObject.transform;
			}
			
			var parentVisibility = NguiUtils.GetComponentInParentsAs<IVisibilityBinding>(gameObject);
			foreach(var visibility in NguiUtils.GetComponentsInChildrenAs<IVisibilityBinding>(itemObject))
			{
				visibility.InvalidateParent();
			}
			var visible = parentVisibility == null ? true : parentVisibility.Visible;
			NguiUtils.SetVisible(itemObject, visible);
			
			_dirty = true;
		}
	}
	
	protected virtual void OnItemRemove(int position)
	{
		if (_itemTemplate == null)
			return;
		
		for (var i = 0; i < transform.childCount; ++i)
		{
			var child = transform.GetChild(i).gameObject;
			int childNumber;
			if (int.TryParse(child.name, out childNumber))
			{
				if (childNumber == position)
				{
					GameObject.DestroyImmediate(child);
					break;
				}
			}
		}
		for (var i = 0; i < transform.childCount; ++i)
		{
			var child = transform.GetChild(i).gameObject;
			int childNumber;
			if (int.TryParse(child.name, out childNumber))
			{
				if (childNumber > position)
				{
					child.name = string.Format("{0}", childNumber - 1);
				}
			}
		}
	
		_dirty = true;
	}
	
	protected virtual void OnItemsClear()
	{
		if (_itemTemplate == null)
			return;
		
		while(transform.childCount > 0)
		{
			GameObject.DestroyImmediate(transform.GetChild(0).gameObject);
		}
		
		_dirty = true;
	}
	
	public void OnSelectionChange(GameObject selectedObject)
	{
		if (_collection != null && !_isCollectionSelecting)
		{
			_isCollectionSelecting = true;
			for (var i = 0; i < transform.childCount; ++i)
			{
				var child = transform.GetChild(i).gameObject;
				if (selectedObject != child)
					continue;
				int childNumber;
				if (int.TryParse(child.name, out childNumber))
				{
					_collection.SelectItem(childNumber);
					break;
				}
			}
			_isCollectionSelecting = false;
		}
	}
	
	protected virtual void OnCollectionSelectionChange()
	{
		for (var i = 0; i < transform.childCount; ++i)
		{
			var child = transform.GetChild(i).gameObject;
			int childNumber;
			if (int.TryParse(child.name, out childNumber))
			{
				var itemData = child.GetComponent<NguiItemDataContext>();
				if (itemData != null)
					itemData.SetSelected(childNumber == _collection.SelectedIndex);
			}
		}
	}
}
