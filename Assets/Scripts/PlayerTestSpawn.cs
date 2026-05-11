using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerTestSpawn : MonoBehaviour
{
    public GameObject prefab;
    public bool assignPlayerGlobally;
    public string popBackSpawnSystemName;
    public string planeLayerColliderMapName;

    void AssignPlayerParam(Transform t, GameObject player) 
    {
        if (t.gameObject.GetComponent<FlightJoystick>()) 
        {
            t.gameObject.GetComponent<FlightJoystick>().flightControl = player.GetComponent<FlightControl>();
        }
        if (t.gameObject.GetComponent<GameOver>()) 
        {
            t.gameObject.GetComponent<GameOver>().plane = player;
        }
        if (t.gameObject.GetComponent<FuelBar>()) 
        {
            t.gameObject.GetComponent<FuelBar>().plane = player;
        }
        if (t.gameObject.GetComponent<CinemachineVirtualCamera>()) 
        {
            t.gameObject.GetComponent<CinemachineVirtualCamera>().Follow = player.transform;
        }
        if (t.gameObject.GetComponent<PopBackSync>()) 
        {
            t.gameObject.GetComponent<PopBackSync>().player = player;
        }
        if (t.gameObject.GetComponent<ObjectSpawnSystem>()) 
        {
            if(t.gameObject.name == popBackSpawnSystemName)
                t.gameObject.GetComponent<ObjectSpawnSystem>().player = player;
        }
        if (t.gameObject.GetComponent<DepthIllusion>()) 
        {
            t.gameObject.GetComponent<DepthIllusion>().player = player;
            print($"{t.name}: DepthIllusion player assigned: {player.name}");
        }
        if (t.gameObject.GetComponent<SyncedMovingObjectParams>()) 
        {
            t.gameObject.GetComponent<SyncedMovingObjectParams>().plane = player;
        }
        if (t.gameObject.GetComponent<NPCThrower>()) 
        {
            t.gameObject.GetComponent<NPCThrower>().plane = player;
        }
        if (t.gameObject.GetComponent<Thrower>())
        {
            t.gameObject.GetComponent<Thrower>().plane = player;
        }
        if (t.gameObject.GetComponent<CatchPlane>()) 
        {
            t.gameObject.GetComponent<CatchPlane>().plane = player;
        }
        if (t.gameObject.GetComponent<CollectibleHandler>())
        {
            t.gameObject.GetComponent<CollectibleHandler>().plane = player;
        }
        if (t.gameObject.GetComponent<DestroyPlane>()) 
        {
            t.gameObject.GetComponent<DestroyPlane>().plane = player;
        }
        if (t.gameObject.GetComponent<FloorControl>()) 
        {
            t.gameObject.GetComponent<FloorControl>().player = player;
        }
        if (t.gameObject.GetComponent<NPCMarkovBrain>()) 
        {
            t.gameObject.GetComponent<NPCMarkovBrain>().plane = player;
        }
        if (t.gameObject.GetComponent<LayerColliderMap>()) 
        {
            if(gameObject.name == planeLayerColliderMapName)
                t.gameObject.GetComponent<LayerColliderMap>().SetTarget(player);
        }

    }
    void GlobalAssign(Transform root, GameObject player) 
    {
        //recursively assign player
        int sceneSiblingCount = root.parent.childCount;
        for (int i = 0; i < sceneSiblingCount; i++) 
        {
            //if contains player param assign player copy to param
            
            Transform current = root.parent.GetChild(i);
            //print($"hello? tree at {current.name}");
            AssignPlayerParam(current, player);
            //recurse to check children for 
            if (current.childCount > 0)
            {
                //print("dig to" + current.GetChild(0));
                GlobalAssign(current.GetChild(0), player);
            }
        }
    }

    void Start()
    {
        
        GameObject player = Instantiate(prefab);
        if (assignPlayerGlobally)
        {
            //print("hello? wtf");
            GlobalAssign(transform, player);
        }
    }

}
