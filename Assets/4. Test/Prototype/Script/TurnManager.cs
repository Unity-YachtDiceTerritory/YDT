using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance
    {
        get {  return instance; }
    }
    private static TurnManager instance;

    public readonly SyncDictionary<int, NetworkObject> clientNODict = new SyncDictionary<int, NetworkObject>();
    public readonly SyncList<int> clientIDList = new SyncList<int>();
    public readonly SyncVar<int> currentIndex = new SyncVar<int>();
    private int throwChances = 3;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        InstanceFinder.ServerManager.OnRemoteConnectionState += UpdateClientsCount;

        clientIDList.Clear();
        currentIndex.Value = 0;
    }

    public void UpdateClientsCount(NetworkConnection connection, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            clientIDList.Add(connection.ClientId);
        }
        else if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            int currentClientID = clientIDList[currentIndex.Value];

            clientIDList.Remove(connection.ClientId);

            clientNODict.Remove(connection.ClientId);


            for (int i = 0; i < clientIDList.Count; i++)
            {
                if (clientIDList[i] == currentClientID)
                {
                    currentIndex.Value = i;
                    break;
                }
            }
        }
    }

    [Server]
    public void RegisterClientNetworkObject(NetworkObject networkObject)
    {
        int clientId = networkObject.Owner.ClientId;

        if (!clientNODict.ContainsKey(clientId))
        {
            clientNODict.Add(clientId, networkObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void NextPlayerTurnServerRpc()
    {
        currentIndex.Value++;
        DiceController.Instance.DiceTurnState = DiceTurnState.First_ReadyForRoll;
        if (currentIndex.Value >= clientIDList.Count)
        {
            currentIndex.Value = 0;
        }

        Debug.Log($"NextPlayerTurn: {currentIndex.Value}");
    }
}
