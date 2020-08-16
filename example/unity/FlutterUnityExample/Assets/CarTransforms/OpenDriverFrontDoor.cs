using UnityEngine;
public class OpenDriverFrontDoor : MonoBehaviour
{
    /*********************************************
    * Driver Front Door
    */
    GameObject driver_front_door;
    Animator driver_front_door_ANIMATOR;
    /*********************************************
    * Driver Front Door
    */

    /*********************************************
    * Driver Back Door
    */
    /////TODO: add these vars
    /*********************************************
    * Driver Back Door
    */

    /*********************************************
    * Passenger Front Door
    */
    GameObject passenger_front_door;
    Animator passenger_front_door_ANIMATOR;
    /*********************************************
    * Passenger Back Door
    */

    /*********************************************
    * Trunk
    */
    GameObject trunkdoor;
    Animator trunkdoor_ANIMATOR;
    /*********************************************
    * Trunk
    */

    /*********************************************
    * Frunk
    */
    GameObject frunkdoor;
    Animator frunkdoor_ANIMATOR;
    /*********************************************
    * Frunk
    */

    void initDriverFrontDoor()
    {
        driver_front_door = GameObject.Find("DriverFrontDoor");
        driver_front_door_ANIMATOR = driver_front_door.GetComponent<Animator>();
    }

    void initPassengerFrontDoor()
    {
        passenger_front_door = GameObject.Find("PassengerFrontDoor");
        passenger_front_door_ANIMATOR = passenger_front_door.GetComponent<Animator>();
    }

    void initDriverBackDoor()
    {
        //TODO: Insert Animations for this
    }

    void initPassengerBackDoor()
    {
        //TODO: Insert Animations for this
    }

    void initTrunk()
    {
        trunkdoor = GameObject.Find("TrunkDoor");
        trunkdoor_ANIMATOR = trunkdoor.GetComponent<Animator>();
    }

    void initFrunk()
    {
        frunkdoor = GameObject.Find("FrunkDoor");
        frunkdoor_ANIMATOR = frunkdoor.GetComponent<Animator>();
    }

    // Awake is called before Start
    void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        initDriverFrontDoor();
        initPassengerFrontDoor();
        initTrunk();
        initFrunk();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// Flutter Bridging method to animate openings (open/close)
    public void DoorOpened(string state)
    {
        string[] part = state.Split(':');

        switch (part[0])
        {
            case "driverfrontdoor":
                driverfrontdoor(part[1]);
                break;
            case "passengerfrontdoor":
                passengerfrontdoor(part[1]);
                break;
            case "trunk":
                trunkate_trunkdoor(part[1]);
                break;
            case "frunk":
                trunkate_frunkdoor(part[1]);
                break;
            default: break;
        }

    }

    public void driverfrontdoor(string state)
    {
        driver_front_door_ANIMATOR.SetBool("DoorOpened", bool.Parse(state));
    }

    public void passengerfrontdoor(string state)
    {
        passenger_front_door_ANIMATOR.SetBool("DoorOpened", bool.Parse(state));
    }

    public void trunkate_trunkdoor(string state)
    {
        trunkdoor_ANIMATOR.SetBool("DoorOpened", bool.Parse(state));
    }

    public void trunkate_frunkdoor(string state)
    {
        frunkdoor_ANIMATOR.SetBool("DoorOpened", bool.Parse(state));
    }

}