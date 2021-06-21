using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator2 : MonoBehaviour
{

    [SerializeField] private PseudoRandomGenerator numberGenerator;

    [SerializeField] private GameObject player;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject[] zombies;
    [SerializeField] private int numZombies;


    [SerializeField] private int entryToEndRooms;
    [SerializeField] private int numRooms;
    [SerializeField] private int minRooms;
    [SerializeField] private int maxIntents;


    [SerializeField] private GameObject[] entryRooms;
    [SerializeField] private GameObject[] endRooms;

    [SerializeField] private GameObject[] roomsPool;
    [SerializeField] private GameObject wallPool;

    private int numRng;
    private int nextRoomNum;
    private List<GameObject> roomsSpawned;
    private List<GameObject> wallsSpawned;
    private List<GameObject> trashRooms;
    private List<GameObject> availableRooms;
    private List<GameObject> availableDoors;
    private List<GameObject> spawnsPool;
    private List<Transform> patrolWaypointsPool;
    private GameObject[] doorways;

    private GameObject startRoom;
    private GameObject endRoom;
    private GameObject lastRoomSpawned;

    private bool endRoomSpawned;

    LayerMask roomLayerMask;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetSpawn();
        }
    }
    
    void Start()
    {
        availableDoors = new List<GameObject>();
        roomsSpawned = new List<GameObject>();
        wallsSpawned = new List<GameObject>();
        trashRooms = new List<GameObject>();
        spawnsPool = new List<GameObject>();
        patrolWaypointsPool = new List<Transform>();
        endRoomSpawned = false;


        for (int i = 0; i < roomsPool.Length; i++)
        {
            roomsPool[i].GetComponent<RoomTemplate>().roomType = i;
        }

        StartSpawn();

   

    }

    void StartSpawn()
    {
        roomLayerMask = LayerMask.GetMask("Room");
        GameObject room;
        numRng = numberGenerator.GetNumber(entryRooms.Length);
        //spawn y colocacion de la sala inicial
        
        room = Instantiate(entryRooms[numRng], Vector3.zero, entryRooms[numRng].transform.rotation);
        startRoom = room;
        lastRoomSpawned = room;
        roomsSpawned.Add(room);
        room.transform.parent = this.transform;
        room.name = "initialRoom";
        RoomTemplate rt = room.GetComponent<RoomTemplate>();

        foreach (GameObject g in rt.doorways)
        {
            availableDoors.Add(g);
        }

        try
        {
            StartCoroutine("GenerateMainPath");
            //GenerateOffPath();


        }
        catch (System.ArgumentOutOfRangeException e)
        {
            Debug.Log("alcanzado maximo de salas: " + e);
        }
        
    }

    // genera un camino pincipal con sala final incluida de la longitud definida en EntryToEndRooms
    IEnumerator GenerateMainPath()
    {

        DoorTemplate doorway;
        int intents = 0;
        int numRng = 0;
        do
        {
            //intents = 0;
            try
            {
                doorways = lastRoomSpawned.GetComponent<RoomTemplate>().doorways;
                do
                {
                    numRng = numberGenerator.GetNumber(doorways.Length);
                    doorway = doorways[numRng].GetComponent<DoorTemplate>();
                    if (doorway.conected)
                    {
                        intents++;
                    }
                } while (doorway.conected && intents < maxIntents);

                //intenta spawnear una sala y si el intento ha resultado fallido añade un intento fallido al contador
                if (!SpawnRoom(doorways[numRng], false))
                {
                    intents++;
                };
            }
            catch (System.ArgumentOutOfRangeException ex)
            {
                Debug.Log("numero: " + numRng);
                Debug.LogError(ex);
                intents++;
            }

            if (intents >= maxIntents)
            {
                
                numRng = numberGenerator.GetNumber(availableDoors.Count - 1);
                lastRoomSpawned = availableDoors[numRng].transform.parent.transform.parent.gameObject;
                intents = 0;
            }

            //Debug.Break();  
        } while (RoomsUntilStart(lastRoomSpawned) < entryToEndRooms);


        // crea la sala final

        intents = 0;
        try
        {
            doorways = lastRoomSpawned.GetComponent<RoomTemplate>().doorways;
            do
            {
                numRng = numberGenerator.GetNumber(doorways.Length);
                doorway = doorways[numRng].GetComponent<DoorTemplate>();
            } while (doorway.conected);
            SpawnRoom(doorways[numRng], true);
        }
        catch (System.ArgumentOutOfRangeException ex)
        {
            Debug.LogError(ex);
        }
        yield return null;



        Debug.Log("comprobando spawn sala final");
        if (!endRoomSpawned)
        {
            StartCoroutine("GenerateMainpath");
            //ResetSpawn();
        }
        else
        {
            StartCoroutine("GenerateOffPath2");
        }

    }

    // generacion de sala nueva en la puerta recibida
    //int num2 = 0;
    bool SpawnRoom(GameObject _door, bool _endRoom)
    {
        DoorTemplate pt = _door.GetComponent<DoorTemplate>();
        GameObject newRoom;
        int intents = 0;

        // el siguiente bucle se repite hasta que se genera una sala de forma correcta o se queda sin intentos
        do
        {
            if (!_endRoom)
            {
                do
                {
                    numRng = numberGenerator.GetNumber(roomsPool.Length - 1);
                } while (roomsPool[numRng].GetComponent<RoomTemplate>().doorways.Length < 2);

                newRoom = CheckRoomInTrashList(roomsPool[numRng]);

            }
            else
            {
                if (endRoom == null)
                {
                    numRng = numberGenerator.GetNumber(endRooms.Length);
                    newRoom = Instantiate(endRooms[numRng]);
                    newRoom.name = "endRoom";
                    endRoom = newRoom;
                }
                else
                {
                    newRoom = endRoom;
                    newRoom.SetActive(true);
                }

            }

            //newRoom.name = newRoom.name + num2;
            //num2++;

            GameObject[] doorsNewRoom = newRoom.GetComponent<RoomTemplate>().doorways;


            GameObject door = null;
            Vector3 offset = Vector3.zero;
            Vector3 rotateRoom = Vector3.zero;
            do
            {
                numRng = numberGenerator.GetNumber(doorsNewRoom.Length);
                if (doorsNewRoom[numRng].GetComponent<DoorTemplate>().conected)
                {
                    intents++;
                }
            } while (
                doorsNewRoom[numRng].GetComponent<DoorTemplate>().conected &&
                intents < maxIntents
            );
            door = doorsNewRoom[numRng];
            RoomTemplate roomTemplate = newRoom.GetComponent<RoomTemplate>();


            // definicion del offset y del giro de la sala nueva para conectarla a la sala anterior
            if (pt.orientation.Equals(DoorTemplate.orientations.left))
            {
                offset = new Vector3(-0.01f, 0, 0);
                if (doorsNewRoom[numRng].GetComponent<DoorTemplate>().orientation.Equals(DoorTemplate.orientations.top))
                {
                    rotateRoom = Vector3.up * 90;
                    roomTemplate.ReAssingDoorsOrientation(1);
                }
                else if (doorsNewRoom[numRng].GetComponent<DoorTemplate>().orientation.Equals(DoorTemplate.orientations.bottom))
                {
                    rotateRoom = Vector3.up * -90;
                    roomTemplate.ReAssingDoorsOrientation(3);
                }
                else if (doorsNewRoom[numRng].GetComponent<DoorTemplate>().orientation.Equals(DoorTemplate.orientations.left))
                {
                    rotateRoom = Vector3.up * 180;
                    roomTemplate.ReAssingDoorsOrientation(2);
                }
            }
            else if (pt.orientation.Equals(DoorTemplate.orientations.right))
            {
                offset = new Vector3(0.01f, 0, 0);
                if (doorsNewRoom[numRng].GetComponent<DoorTemplate>().orientation.Equals(DoorTemplate.orientations.top))
                {
                    rotateRoom = Vector3.up * -90;
                    roomTemplate.ReAssingDoorsOrientation(3);
                }
                else if (doorsNewRoom[numRng].GetComponent<DoorTemplate>().orientation.Equals(DoorTemplate.orientations.bottom))
                {
                    rotateRoom = Vector3.up * 90;
                    roomTemplate.ReAssingDoorsOrientation(1);
                }
                else if (doorsNewRoom[numRng].GetComponent<DoorTemplate>().orientation.Equals(DoorTemplate.orientations.right))
                {
                    rotateRoom = Vector3.up * 180;
                    roomTemplate.ReAssingDoorsOrientation(2);
                }
            }
            else if (pt.orientation.Equals(DoorTemplate.orientations.top))
            {
                offset = new Vector3(0, 0, 0.01f);
                if (doorsNewRoom[numRng].GetComponent<DoorTemplate>().orientation.Equals(DoorTemplate.orientations.top))
                {
                    rotateRoom = Vector3.up * 180;
                    roomTemplate.ReAssingDoorsOrientation(2);
                }
                else if (doorsNewRoom[numRng].GetComponent<DoorTemplate>().orientation.Equals(DoorTemplate.orientations.left))
                {
                    rotateRoom = Vector3.up * -90;
                    roomTemplate.ReAssingDoorsOrientation(3);
                }
                else if (doorsNewRoom[numRng].GetComponent<DoorTemplate>().orientation.Equals(DoorTemplate.orientations.right))
                {
                    rotateRoom = Vector3.up * 90;
                    roomTemplate.ReAssingDoorsOrientation(1);
                }
            }
            else if (pt.orientation.Equals(DoorTemplate.orientations.bottom))
            {
                offset = new Vector3(0, 0, -0.01f);
                if (doorsNewRoom[numRng].GetComponent<DoorTemplate>().orientation.Equals(DoorTemplate.orientations.bottom))
                {
                    rotateRoom = Vector3.up * 180;
                    roomTemplate.ReAssingDoorsOrientation(2);
                }
                else if (doorsNewRoom[numRng].GetComponent<DoorTemplate>().orientation.Equals(DoorTemplate.orientations.left))
                {
                    rotateRoom = Vector3.up * 90;
                    roomTemplate.ReAssingDoorsOrientation(1);
                }
                else if (doorsNewRoom[numRng].GetComponent<DoorTemplate>().orientation.Equals(DoorTemplate.orientations.right))
                {
                    rotateRoom = Vector3.up * -90;
                    roomTemplate.ReAssingDoorsOrientation(3);
                }
            }

            // operaciones de giro y colocacion de la sala nueva

            newRoom.GetComponent<RoomTemplate>().parentRoom = _door.transform.parent.gameObject.transform.parent.gameObject;
            newRoom.transform.Rotate(rotateRoom);
            newRoom.transform.position = _door.transform.position + offset;
            Vector3 displacement = door.transform.position - newRoom.transform.position;
            newRoom.transform.position -= displacement;

            //comprobacion de si la sala nueva se superpone con una sala ya existente
            //si la sala se superpone se añade a la lista de desechos para eliminarla mas tarde y aumenta el contador de intentos
            // si la sala no se superpone se configuran sus puertas como conectadas, se añade a la lista de salas colocadas y se emparenta 
            if (!CheckRoomOverlap(newRoom))
            {
                foreach (GameObject d in newRoom.GetComponent<RoomTemplate>().doorways)
                {
                    if (!d.Equals(door))
                    {
                        availableDoors.Add(d);
                    }
                }
                availableDoors.Remove(_door);

                door.GetComponent<DoorTemplate>().conected = true;
                door.GetComponent<DoorTemplate>().childDoor = true;
                _door.GetComponent<DoorTemplate>().conected = true;
                roomsSpawned.Add(newRoom);
                newRoom.transform.parent = this.transform;
                if (_endRoom)
                {
                    endRoomSpawned = true;
                }
                this.lastRoomSpawned = newRoom;
                return true;
            }
            else
            {
                Debug.Log("colocar sala fallido");
                newRoom.SetActive(false);
                if (!_endRoom)
                {
                    trashRooms.Add(newRoom.gameObject);
                }
                newRoom.transform.parent = null;
                newRoom = null;
                intents += 1;
                //Debug.Log("intent: " + intents);
            }
        } while (newRoom == null && intents < maxIntents);

        return false;
        //Debug.Log("colocada sala: " + nextRoomNum);
    }

    //comprobacion de si una sala se superpone con alguna otra
    bool CheckRoomOverlap(GameObject _room)
    {
        BoxCollider[] roomColliders;

        roomColliders = _room.GetComponents<BoxCollider>(); //se obtienen todos los collides de la sala nueva

        foreach (BoxCollider col in roomColliders)
        {
            Bounds bounds = col.bounds; // se obtienen los tamaños del collider
            Collider[] colliders = Physics.OverlapBox(bounds.center, bounds.size / 2, Quaternion.identity, roomLayerMask); //se comprueba si hay colliders en la zona definida
            //Debug.Log(_room.name + ": " + colliders.Length);
            if (colliders.Length > 0) //si se detecta alguno se comprueba si es del propio objeto
            {

                foreach (Collider c in colliders)
                {

                    //ignora la colision con la sala actual
                    if (c.gameObject.Equals(_room.gameObject))
                    {
                        continue;
                    }
                    else // si el gameobject al que pertenece el collider es de otro objeto distinto a la sala devuelve true
                    {
                        //Debug.Log(c.gameObject.name + " || " + _room.gameObject.name);
                        //Debug.LogError("overlap detected");
                        return true;
                    }
                }
            }
        }
        return false;
    }

    GameObject CheckRoomInTrashList(GameObject _newRoom)
    {
        RoomTemplate roomTemplate = _newRoom.GetComponent<RoomTemplate>();
        for (int i = 0; i < trashRooms.Count; i++)
        {
            if (trashRooms[i].GetComponent<RoomTemplate>().roomType == roomTemplate.roomType)
            {
                GameObject room = trashRooms[i];
                trashRooms.Remove(trashRooms[i]);
                room.SetActive(true);
                //                Debug.Log("sala creada anteriormente");
                //room.name = room.name + "||" + numRoomsSpawned();
                return room;
            }
        }

        //        Debug.Log("sala no creada anteriormente");
        //_newRoom.name = _newRoom.name + "||" + numRoomsSpawned();
        return Instantiate(_newRoom);
    }


    private void ResetSpawn()
    {
        //UnityEngine.SceneManagement.SceneManager.LoadScene("Main");

        Debug.Log("error, intento reseteo de escena");
    }

    //contador de cuantas salas hay desde la sala recibida hasta la primera sala
    //accediendo al componente ParentRoom guardado en cada sala
    int RoomsUntilStart(GameObject g)
    {
        bool gIsFisrtRoom = false;
        int roomsUntilStart = 0;
        do
        {
            try
            {
                g = g.GetComponent<RoomTemplate>().parentRoom;
                roomsUntilStart++;
            }
            catch (System.Exception)
            {
                gIsFisrtRoom = true;
            }
        } while (!gIsFisrtRoom);
        Debug.Log("******salas hasta la primera: " + roomsUntilStart);
        return roomsUntilStart;
    }
}

    


