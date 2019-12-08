using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTemplate : MonoBehaviour
{
    public int roomType;

    Transform[] items;
    public GameObject wallsParent;
    public GameObject floorParent;
    public GameObject wall;
    public bool test;

    public GameObject[] doorways;
    public GameObject[] spawns;
    public GameObject[] patrolWaypoints;
    bool overlaped =  false;

    public GameObject parentRoom;

    public void spawnArt()
    {
        if (test)
        {
            items = wallsParent.GetComponentsInChildren<Transform>();
            

            for(int i = 1; i<items.Length; i++)
            {
                if (items[i].tag.Equals("Wall"))
                {
                    items[i].GetComponentInChildren<MeshRenderer>().enabled = false;
                    GameObject newItem = Instantiate(wall);
                    newItem.transform.SetParent(items[i].transform);
                    //newItem.transform.SetParent(wallsParent.transform);
                    newItem.transform.localPosition = Vector3.zero;
                    newItem.transform.localRotation = Quaternion.Euler(Vector3.zero);
                    //items[i].GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
	{
        if(!other.gameObject.Equals(this.gameObject)){
            overlaped = true;
        }
	}

    private void OnTriggerExit (Collider other)
    {
        if (!other.gameObject.Equals(this.gameObject))
        {
            overlaped = false;
        }
    }

    public void ReAssingDoorsOrientation(int index){
        for (int i = 0; i < doorways.Length; i++){
            DoorTemplate doorTemplate = doorways[i].GetComponent<DoorTemplate>();
            int num = (int)doorTemplate.orientation;
            num += index;
            if(num>=4){
                num -= 4;
            }
            doorTemplate.orientation = (DoorTemplate.orientations)num;

        }
    }

    private void OnDrawGizmosSelected()
    {/*
        BoxCollider[] c = this.GetComponents<BoxCollider>();


        Gizmos.color = Color.red;
        foreach (BoxCollider bc in colliders)
        {
            Gizmos.DrawCube(bc.bounds.center , bc.bounds.size);
        }
    */}
}
