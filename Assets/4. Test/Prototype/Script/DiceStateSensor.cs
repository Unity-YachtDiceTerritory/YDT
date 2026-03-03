using UnityEngine;

public enum DiceStateEnum
{
    None = 0,
    Grounded,
    Rolling,
    Selected
}

public class DiceStateSensor : MonoBehaviour
{
    private Rigidbody rb;
    private Collider diceCollider;

    public DiceStateEnum CurrentDiceState
    {
        get { return currentDiceState; }
    }
    private DiceStateEnum currentDiceState;

    private void Update()
    {
        Debug.Log(rb.angularVelocity);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            ChangeState(DiceStateEnum.Grounded);
            Debug.Log("tag: Ground");
        }
    }

    public void ChangeState(DiceStateEnum state)
    {
        currentDiceState = state;
    }
}
