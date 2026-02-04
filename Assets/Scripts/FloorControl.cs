using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FloorControl : MonoBehaviour
{
    [System.Serializable]
    public struct LayerInfo {
        public string levelName;
        public int sortingOrder;
        public int layerLevel;
        public float floorY;
        public LayerColliderMap colliderMap;
    }

    public List<LayerInfo> layers;
    public GameObject player;
    public bool flipLayerDirection;

    int currentLayer;
    public int GetCurrentLayer() { return currentLayer; }
    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, layers[currentLayer].floorY, gameObject.transform.position.z);
    }
    public void OnDrawGizmos()
    {
        for (int i = 0; i < layers.Count; i++)
        {
            Vector3 colliderPosition = new Vector3(gameObject.transform.position.x, layers[i].floorY, gameObject.transform.position.z);
            Vector2 colliderDimens = gameObject.GetComponent<BoxCollider2D>().size;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(colliderPosition, colliderDimens);
        }
    }
    IEnumerator SwitchLayer(int direction)
    {
        //wait for clear target in: Current layer if moving up, next layer if moving down.
        int nextLayer = currentLayer + direction;
        bool nextLayerInBounds = nextLayer > 0 && nextLayer < layers.Count;
        if (direction > 0 && nextLayerInBounds && layers[nextLayer].colliderMap != null)
        {
            yield return new WaitUntil(() => layers[nextLayer].colliderMap.clearTarget);
            //print($"Cleared to move down to layer {(nextLayerInBounds ? nextLayer : currentLayer)}");
        }
        if (direction < 0 && layers[currentLayer].colliderMap != null)
        {
            yield return new WaitUntil(() => layers[currentLayer].colliderMap.clearTarget);
            //print($"Cleared to move up to layer {(nextLayerInBounds ? nextLayer : currentLayer)}");
        }
        currentLayer = (nextLayer >= 0 && nextLayer < layers.Count) ? nextLayer : currentLayer;
        //print(currentLayer);
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, layers[currentLayer].floorY, gameObject.transform.position.z);
        
        player.GetComponent<SpriteRenderer>().sortingOrder = layers[currentLayer].sortingOrder;// instead, recurse through player hierarchy when applicable
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKeyUp(KeyCode.S)) //move floor down
        {
            StartCoroutine(SwitchLayer(flipLayerDirection ? -1 : 1));
        }
        if (Input.GetKeyUp(KeyCode.W)) //move floor up
        {
            StartCoroutine(SwitchLayer(flipLayerDirection ? 1 : -1));
        }
        //when ready reassign sorting order
    }
}
