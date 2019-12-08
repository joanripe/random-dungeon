using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class LevelGenerator : MonoBehaviour
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

    private int  numRng;
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

    private bool endRoomSpawned;

    LayerMask roomLayerMask;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetSpawn();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        availableDoors = new List<GameObject>();
        roomsSpawned = new List<GameObject>();
        wallsSpawned = new List<GameObject>();
        trashRooms = new List<GameObject>();
        spawnsPool = new List<GameObject>();
        patrolWaypointsPool = new List<Transform>();
        endRoomSpawned = false;


        for (int i = 0; i < roomsPool.Length; i++){
            roomsPool[i].GetComponent<RoomTemplate>().roomType = i;
        }

        StartSpawn();

        //        Debug.Log("rooms spawned: " + roomsSpawned.Count);
        //CheckMinRooms();

      

        //GetSpawnsPool();
        /*
        //player.transform.position = spawnsPool[0].transform.position;
        spawnsPool.Remove(spawnsPool[0]);

        GameObject newEnemy;
        for (int i = 0; i < numZombies; i++){
            if (spawnsPool.Count == 0) break;

            numRng = Random.Range(0, zombies.Length-1);
            newEnemy = Instantiate(zombies[numRng]);


            numRng = Random.Range(0, spawnsPool.Count-1);
            newEnemy.transform.position = spawnsPool[numRng].transform.position;
            spawnsPool.Remove(spawnsPool[numRng]);

            numRng = Random.Range(0, 360);
            newEnemy.transform.rotation = Quaternion.Euler(Vector3.up * numRng);

            //newEnemy.GetComponent<StateController>().SetupAI(true, patrolWaypointsPool);
            
        }*/

    }

    void CheckMinRooms()
    {
        if (roomsSpawned.Count < minRooms)
        {
            //Debug.Log("Reset: minimo de salas no alcanzado");

            ResetSpawn();

        } else
        {
            Debug.Log("test");
        }
    }

    void StartSpawn(){
        roomLayerMask = LayerMask.GetMask("Room");
        GameObject room;
        numRng = numberGenerator.GetNumber (entryRooms.Length);
        //spawn y colocacion de la sala inicial
        if(startRoom == null){
            room = Instantiate(entryRooms[numRng], Vector3.zero, entryRooms[numRng].transform.rotation);
            startRoom = room;
        } else {
            room = startRoom;
        }
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
            StartCoroutine ("GenerateMainPath");
            //GenerateOffPath();

           
        } catch (System.ArgumentOutOfRangeException e){
            Debug.Log("alcanzado maximo de salas: " + e);
        }
        //ClearTrashRooms();
        //GenerateWalls();
        //ClearComponents();

        //this.GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    // genera un camino pincipal con sala final incluida de la longitud definida en EntryToEndRooms
    IEnumerator GenerateMainPath(){
        
        DoorTemplate doorway;
        int intents = 0;
        int i = 0;
        do
        {
            try
            {
                doorways = roomsSpawned[i].GetComponent<RoomTemplate>().doorways;
                do
                {
                    numRng = numberGenerator.GetNumber(doorways.Length);
                    doorway = doorways[numRng].GetComponent<DoorTemplate>();
                    if (doorway.conected)
                    {
                        intents++;
                    }
                } while (doorway.conected && intents<maxIntents);
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
            
            i += 1;
            yield return null;
        } while (intents < maxIntents && i < entryToEndRooms);


        // crea la sala final
        try
        {
            doorways = roomsSpawned[entryToEndRooms].GetComponent<RoomTemplate>().doorways;
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
        
            
        

        Debug.Log("comprobando spawn sala final");
        if(!endRoomSpawned){
            ResetSpawn();
        } else
        {
            StartCoroutine("GenerateOffPath2");
        }

    }

    // intenta crear salas hasta llegar el numero maximo definido
    IEnumerator GenerateOffPath1(){
        nextRoomNum = 0;
        int intents = 0;
        while (roomsSpawned.Count < numRooms && intents < maxIntents) ;
        {

            doorways = roomsSpawned[nextRoomNum].GetComponent<RoomTemplate>().doorways;
            for (int x = 0; x < doorways.Length; x++)
            {
                DoorTemplate doorway = doorways[x].GetComponent<DoorTemplate>();

                if (!doorway.conected)
                {
                    if (!SpawnRoom(doorways[x], false))
                    {
                        intents++;
                    };
                }
            }
            nextRoomNum += 1;
            yield return null;
        }
    }

    // intenta crear salas hasta llegar el numero maximo definido
    IEnumerator GenerateOffPath2()
    {
        nextRoomNum = 0;
        int intents = 0;

        while (roomsSpawned.Count < numRooms && intents < maxIntents)
        {
            Debug.Log("entrando al buclo de offpath");
            /*do
            {
                numRng = numberGenerator.GetNumber(roomsSpawned.Count);
            } while (
                roomsSpawned[numRng].GetComponent<RoomTemplate>().roomType == 0
            );
            
            doorways = roomsSpawned[numRng].GetComponent<RoomTemplate>().doorways;
            
            
            DoorTemplate doorway = doorways[numRng].GetComponent<DoorTemplate>();
            */

            try
            {
                numRng = numberGenerator.GetNumber(availableDoors.Count);
                DoorTemplate doorway = availableDoors[numRng].GetComponent<DoorTemplate>();

                if (!doorway.conected)
                {
                    Debug.Log("puesta no conectada");
                    if (!SpawnRoom(doorway.gameObject, false))
                    {
                        Debug.Log("offpath: spawn fallido ");
                        intents++;
                    }
                    else
                    {
                        Debug.Log("offpath: spawn correcto ");
                    };
                }
                else
                {
                    Debug.Log("puesta conectada");
                    intents++;
                }
            }
            catch (System.ArgumentOutOfRangeException ex)
            {
                intents++;
                Debug.LogError(ex);
            }
            nextRoomNum += 1;
            yield return null;
        }
        Debug.Log("saliendo del bucle de offpath");
        CheckMinRooms();

        ClearTrashRooms();
        GenerateWalls();
        //ClearComponents();
    }

    //eliminacion de todas las salas generadas para volver a empezar a generar
    private void ResetSpawn()
    {
        //Debug.Break();

        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
        
        
        /*roomsSpawned.Remove(endRoom);
        roomsSpawned.Remove(startRoom);
        GameObject[] doors = startRoom.GetComponent<RoomTemplate>().doorways;
        for (int x = 0; x < doors.Length; x++)
        {
            doors[x].GetComponent<DoorTemplate>().conected = false;
        }

        for (int i = 0; i < roomsSpawned.Count; i++)
        {
            GameObject room = roomsSpawned[i];
            doors = room.GetComponent<RoomTemplate>().doorways;
            for (int x = 0; x < doors.Length;x++){
                doors[x].GetComponent<DoorTemplate>().conected = false;
            }
            room.SetActive(false);
            Destroy(roomsSpawned[i]);
            //roomsSpawned.Remove(room);
            //trashRooms.Add(room);
        }

        for (int i = 0; i < wallsSpawned.Count; i++)
        {
            wallsSpawned[i].SetActive(false);
            //Destroy(wallsSpawned[i]);
        }

        endRoomSpawned = false;
        roomsSpawned.Clear();
        wallsSpawned.Clear();

        StartSpawn();
    */}

    void GenerateWalls(){
        for (int i = 0; i < roomsSpawned.Count; i++){
            doorways = roomsSpawned[i].GetComponent<RoomTemplate>().doorways;
            for (int x = 0; x < doorways.Length; x++) {
                DoorTemplate doorway = doorways[x].GetComponent<DoorTemplate>();
                if (!doorway.conected){
                    SpawnWalls(doorways[x]);
                }
            }

            //roomsSpawned[i].GetComponent<RoomTemplate>().spawnArt();
        }
    }

    void  SpawnWalls(GameObject _doorway){
        DoorTemplate pt = _doorway.GetComponent<DoorTemplate>();
        GameObject wall;
        Vector3 rotateWall = Vector3.zero;

        if (pt.orientation.Equals(DoorTemplate.orientations.left))
        {
            rotateWall = Vector3.up * 90;

        }
        else if (pt.orientation.Equals(DoorTemplate.orientations.right))
        {
            rotateWall = Vector3.up * -90;

        }
        else if (pt.orientation.Equals(DoorTemplate.orientations.top))
        {
            rotateWall = Vector3.up * 180;
        }

        wall = Instantiate(wallPool);
        wall.transform.rotation = Quaternion.Euler(rotateWall);
        wall.transform.position = _doorway.transform.position - (Vector3.up * 1.25f);
        wallsSpawned.Add(wall);
        pt.conected = true;
        wall.transform.parent = _doorway.transform;
    }

    // generacion de sala nueva en la puerta recibida
    //int num2 = 0;
    bool SpawnRoom(GameObject _door, bool _endRoom){
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
                    numRng = numberGenerator.GetNumber(roomsPool.Length-1);
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
                intents<maxIntents
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

            newRoom.GetComponent<RoomTemplate>().parentRoom = _door.transform.parent.gameObject;
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
        } while (newRoom==null && intents<maxIntents);

        return false;
        //Debug.Log("colocada sala: " + nextRoomNum);
    }



    //elimina todas las salas de los intentos fallidos de creacion de sala
    void ClearTrashRooms(){
        for (int i = 0; i < trashRooms.Count; i++){
            Destroy(trashRooms[i].gameObject);
            //trashRooms[i].gameObject.SetActive(false);
        }
        trashRooms.Clear();
    }

    //comprobacion de si una sala se superpone con alguna otra
    bool CheckRoomOverlap(GameObject _room){
        BoxCollider[] roomColliders;

        roomColliders = _room.GetComponents<BoxCollider>(); //se obtienen todos los collides de la sala nueva

        foreach(BoxCollider col in roomColliders){
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

    void ClearComponents(){
        foreach (GameObject g in roomsSpawned){
            Collider[] colliders = g.GetComponents<Collider>();
            foreach(Collider c in colliders){
                Destroy(c);
            }
            Rigidbody rb = g.GetComponent<Rigidbody>();
            Destroy(rb);
        }
    }

    int nRooms = 0;
    int numRoomsSpawned()
    {
        nRooms++;
        return nRooms;
    }

    GameObject CheckRoomInTrashList(GameObject _newRoom){
        RoomTemplate roomTemplate = _newRoom.GetComponent<RoomTemplate>();
        for (int i = 0; i < trashRooms.Count; i++){
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

    void GetSpawnsPool(){
        GameObject[] roomSpawns;
        GameObject[] patrolWaypoints;
        for (int i = 0; i < roomsSpawned.Count; i++)
        {
            Debug.Log("buscando spawns de " + roomsSpawned[i].name + " tiene " + roomsSpawned[i].GetComponent<RoomTemplate>().spawns.Length);
            RoomTemplate roomTemplate = roomsSpawned[i].GetComponent<RoomTemplate>();
            roomSpawns = roomTemplate.spawns;
            patrolWaypoints = roomTemplate.patrolWaypoints;
            if(roomSpawns.Length != 0){
                for (int x = 0; x < roomSpawns.Length; x++)
                {
                    spawnsPool.Add(roomSpawns[x]);
                } 
            }

            if (patrolWaypoints.Length != 0)
            {
                for (int x = 0; x < patrolWaypoints.Length; x++)
                {
                    patrolWaypointsPool.Add(patrolWaypoints[x].transform);
                }
            }
        }
    }
}