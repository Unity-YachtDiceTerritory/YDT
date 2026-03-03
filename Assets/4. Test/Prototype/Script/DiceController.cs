using FishNet;
using FishNet.Object;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public enum DiceTurnState
{
    SpawnDice,
    First_ReadyForRoll, 
    First_Rolling,           
    First_ResultShowing,     
    First_SelectingDice,
    Second_ReadyForRoll,
    Second_Rolling,
    Second_ResultShowing,
    Second_SelectingDice,
    Third_ReadyForRoll,
    Third_Rolling,
    Third_ResultShowing,
    Third_SelectingDice,
    TurnWait
}

public class DiceController : NetworkBehaviour
{
    public static DiceController Instance
    {
        get { return instance; }
    }
    private static DiceController instance;

    [SerializeField] private GameObject[] dices;
    private Rigidbody[] diceRb;
    private DiceStateSensor[] diceStates;
    private Vector3[] dicePosition = new Vector3[]
    {
        new Vector3(0,0.5f,0),
        new Vector3(0.3f,1.0f,0),
        new Vector3(0,1.5f,0.3f),
        new Vector3(-0.3f,2.0f,0),
        new Vector3(0,2.5f,-0.3f)
    };

    #region 조정 가능한 수치값
    [SerializeField] private float minForceXValue;
    [SerializeField] private float maxForceXValue;
    [SerializeField] private float minForceYValue;
    [SerializeField] private float maxForceYValue;
    [SerializeField] private float minForceZValue;
    [SerializeField] private float maxForceZValue;

    [SerializeField] private float minTorqueXValue;
    [SerializeField] private float maxTorqueXValue;
    [SerializeField] private float minTorqueYValue;
    [SerializeField] private float maxTorqueYValue;
    [SerializeField] private float minTorqueZValue;
    [SerializeField] private float maxTorqueZValue;
    #endregion

    private bool canThrowDice = false;
    private int remainingRolls = 1;
    public DiceTurnState DiceTurnState
    {
        get { return diceTurnState; }
        set { diceTurnState = value; }
    }
    private DiceTurnState diceTurnState;

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

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    int clientID = TurnManager.Instance.clientIDList[TurnManager.Instance.currentIndex.Value];
        //    NetworkObject networkObject = TurnManager.Instance.clientNODict[clientID];

        //    SpawnDice(networkObject.transform);
        //}
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        Initialized();
    }
    private void Initialized()
    {
        diceRb = new Rigidbody[dices.Length];
        diceStates = new DiceStateSensor[dices.Length];

        for (var i = 0; i < dices.Length; i++)
        {
            diceRb[i] = dices[i].GetComponent<Rigidbody>();
            diceStates[i] = dices[i].GetComponent<DiceStateSensor>();
        }

        if (!base.IsServerInitialized)
        {
            foreach (var itemRb in diceRb)
            {
                itemRb.isKinematic = true;
            }
        }


        canThrowDice = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnDice(Transform playerPosition)
    {
        for (int i = 0; i < dices.Length; i++)
        {
            dices[i].transform.position = playerPosition.position + dicePosition[i];
        }
    }
    
    
    [ServerRpc(RequireOwnership = false)]
    public void ThrowDiceServerRpc()
    {
        if (!canThrowDice || remainingRolls <= 0)
        {
            return;
        }

        remainingRolls--;

        for (var i = 0; i < diceStates.Length; i++)
        {
            if (diceStates[i].CurrentDiceState != DiceStateEnum.Grounded &&
                diceStates[i].CurrentDiceState != DiceStateEnum.Selected)
            {
                return;
            }
        }

        for (var i = 0; i < dices.Length; i++)
        {
            diceRb[i].angularVelocity = Vector3.zero;
            diceRb[i].linearVelocity = Vector3.zero;

            Vector3 randomForce = new Vector3(Random.Range(minForceXValue, maxForceXValue), Random.Range(minForceYValue, maxForceYValue), Random.Range(minForceZValue, maxForceZValue));
            Vector3 randomTorque = new Vector3(Random.Range(minTorqueXValue, maxTorqueXValue), Random.Range(minTorqueYValue, maxTorqueYValue), Random.Range(minTorqueZValue, maxTorqueZValue));

            diceRb[i].AddForce(randomForce);
            diceRb[i].AddTorque(randomTorque);

            diceStates[i].ChangeState(DiceStateEnum.Rolling);
        }
    }


    //[ServerRpc(RequireOwnership = false)]
    
    //IEnumerator SortResult()
    //{
    //    while (true)
    //    {
    //        for (var i = 0; i < diceStates.Length; i++)
    //        {
    //            if (diceStates[i].CurrentDiceState != DiceStateEnum.Grounded &&
    //                diceStates[i].CurrentDiceState != DiceStateEnum.Selected)
    //            {
    //                break;
    //            }
    //        }
    //    }

    //    yield return null;  
    //}
}
