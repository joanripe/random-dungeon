using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTemplate : MonoBehaviour
{
    public enum orientations {top, right, bottom, left};
    public orientations orientation;

    public bool conected = false;
    public bool childDoor = false;
}
