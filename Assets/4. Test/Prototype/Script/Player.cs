using FishNet.Object;
using UnityEngine;
using UnityEngine.Rendering;

public class Player : NetworkBehaviour
{
    public override void OnStartServer()
    {
        base.OnStartServer();

        TurnManager.Instance.RegisterClientNetworkObject(this.NetworkObject);
    }


    private void Update()
    {
        if (!base.IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Space) &&
            (TurnManager.Instance.clientIDList[TurnManager.Instance.currentIndex.Value] == base.LocalConnection.ClientId))
        {
            DiceController.Instance.ThrowDiceServerRpc();
        }
    }
}
