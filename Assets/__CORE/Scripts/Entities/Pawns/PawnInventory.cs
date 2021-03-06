using System.Collections;
using System.Collections.Generic;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;

public class PawnInventory : PawnComponent
{
	public readonly SyncList<uint> ItemIds = new SyncList<uint>();

	[ReadOnly]
	public uint[] ReadonlyIds;
	
	public CORE_Delegates.VoidDelegate Evnt_OnItemArrayChanged;
	
	private ItemBase[] _items;

	protected override void Initialize()
	{
		base.Initialize();
		ItemIds.Callback += OnItemIdsChanged;
		RegenerateInternalItemArray();
	}

	public override void Tick()
	{
		ReadonlyIds = new uint[ItemIds.Count];
		for (int i = 0; i < ItemIds.Count; i++)
		{
			ReadonlyIds[i] = ItemIds[i];
		}
	}

	public ItemBase[] GetItems()
	{
		return _items;
	}
	
	private void OnItemIdsChanged(SyncList<uint>.Operation op, int itemindex, uint olditem, uint newitem)
	{
		RegenerateInternalItemArray();
	}

	[ContextMenu("aeiou")]
	private void RegenerateInternalItemArray()
	{
		_items = new ItemBase[ItemIds.Count];

		for (int i = 0; i < ItemIds.Count; i++)
		{
			NetworkIdentity nid = FetchNetworkIdentity(ItemIds[i]);
			if (nid != null)
			{
				_items[i] = nid.GetComponent<ItemBase>();
			}
			else
			{
				Debug.Log("NID IS NULL!!");
				_items[i] = null;
			}
		}

		Evnt_OnItemArrayChanged?.Invoke();
	}

	public void TryAddItemToInventory(ItemBase i)
	{
		AddItemToInventory(i.netId);
	}

	[Command(requiresAuthority = false)]
	private void AddItemToInventory(uint itemNetId)
	{
		NetworkIdentity nid = FetchNetworkIdentity(itemNetId);
		if (!nid) return;
		ItemBase i = nid.GetComponent<ItemBase>();
		if (i.IsBeingHeld()) return;

		ItemIds.Add(i.netId);
		i.CurrentHolder = netId;
	}
}