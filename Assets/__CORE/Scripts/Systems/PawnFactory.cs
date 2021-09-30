using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PawnFactory : NetworkBehaviour
{
	private List<Pawn> _pawns;
	public static PawnFactory Instance { get; private set; }

	private void Awake()
	{
		Instance = this;

		_pawns = new List<Pawn>();
		foreach (NetworkIdentity nid in GameHandler.Instance.GameDefinition.Prefabs)
		{
			Pawn p = nid.GetComponent<Pawn>();
			if (p != null)
			{
				_pawns.Add(p);
			}
		}
	}

	public void RequestPawn(PawnControllerBase c, int pawnIndex)
	{
		SpawnPawnForController(c, pawnIndex);
	}

	[Command(requiresAuthority = false)]
	private void SpawnPawnForController(PawnControllerBase controller, int pawnIndex)
	{
		if (controller.CurrentPawnId > 0) return;
		
		Pawn o = Instantiate(_pawns[pawnIndex]);
		NetworkServer.Spawn(o.gameObject, controller.connectionToClient);
		controller.CurrentPawnId = o.netId;
	}
}