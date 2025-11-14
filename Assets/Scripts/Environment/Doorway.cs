using UnityEngine;

public class Doorway : MonoBehaviour
{
    public GameObject hallDoor;
    public GameObject roomDoor;
    public GameObject wall;
    public Doorway connectedTo;
    public bool isFilled;
    public float hallChance;

    public void ConnectTo(Doorway door)
    {
        connectedTo = door;
        door.connectedTo = this;

    }
    public void Disconnect()
    {
        connectedTo.ForceFill(true, true);
        connectedTo.connectedTo = null;
        ForceFill(true, true);
        connectedTo = null;
    }

    public void ForceFillBoth(bool isHall, bool filled)
    {
        hallDoor.SetActive(false);
        connectedTo.hallDoor.SetActive(false);
        roomDoor.SetActive(false);
        connectedTo.roomDoor.SetActive(false);
        wall.SetActive(false);
        connectedTo.wall.SetActive(false);

        if (isHall)
        {
            roomDoor.SetActive(false);
            connectedTo.roomDoor.SetActive(false);
            hallDoor.SetActive(!filled);
            connectedTo.hallDoor.SetActive(!filled);
            wall.SetActive(filled);
            connectedTo.wall.SetActive(filled);
        }
        else
        {
            hallDoor.SetActive(false);
            connectedTo.hallDoor.SetActive(false);
            roomDoor.SetActive(!filled);
            connectedTo.roomDoor.SetActive(!filled);
            wall.SetActive(filled);
            connectedTo.wall.SetActive(filled);
        }
    }
    public void ForceFill(bool isHall, bool filled)
    {
        hallDoor.SetActive(false);
        roomDoor.SetActive(false);
        wall.SetActive(false);

        if (isHall)
        {
            roomDoor.SetActive(false);
            hallDoor.SetActive(!filled);
            wall.SetActive(filled);
        }
        else
        {
            hallDoor.SetActive(false);
            roomDoor.SetActive(!filled);
            wall.SetActive(filled);
        }
    }

    public void Fill(bool isHall)
    {
        hallDoor.SetActive(false);
        roomDoor.SetActive(false);
        wall.SetActive(false);

        isFilled = connectedTo==null;
        if (isHall)
        {
            roomDoor.SetActive(false);
            hallDoor.SetActive(!isFilled);
            wall.SetActive(isFilled);
        }
        else
        {
            hallDoor.SetActive(false);
            roomDoor.SetActive(!isFilled);
            wall.SetActive(isFilled);
        }
    }
}