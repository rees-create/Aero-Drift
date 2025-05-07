using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class LayerCatalog : MonoBehaviour
{

    [System.Serializable]
    public class SceneObjectLayer 
    {
        public int layerNum;
        public List<GameObject> gameObjects;
        public int playerSpaceSize;
        public int playerSpaceStart;
        public int playerSpot = 1;

        int layerSpaceFactor; //redundantly included as private for easier utility

        public SceneObjectLayer() 
        {
            gameObjects = new List<GameObject>(layerSpaceFactor - playerSpaceSize);
        }

        public void SetLayerSpaceFactor(int lsf) 
        {
            layerSpaceFactor = lsf;
        }

        public int WrapAroundPlayerSpace(int layerDepth)
        {
            if (layerDepth < playerSpaceStart && layerDepth > playerSpaceStart + playerSpaceSize) 
            {
                layerDepth = (layerDepth + playerSpaceSize) % layerSpaceFactor;
            }
            return layerDepth;
        }
        public int Layer(int index) 
        {
            //find the layer depth
            int layerDepth = index < gameObjects.Count ? index : -1;
            int sceneObjectLayerDepth = layerNum * layerSpaceFactor;
            //correct layer depth for player space
            if (layerDepth == -1)
            {
                return -1; //not found, either not in layer array or layer array is too big and object is out of allowed layer space.
            }
            else
            {
                sceneObjectLayerDepth += WrapAroundPlayerSpace(layerDepth);
                return sceneObjectLayerDepth;
            }
            
        }
    }
    
    public List<SceneObjectLayer> sceneObjectLayers;
    [SerializeField] FloorControl floorControl;
    public int layerSpaceFactor;

    public void SetSortingOrder(GameObject g, int desiredSortingOrder, int playerSpaceSize, int playerSpaceStart, int playerSpot)
    {
        if (g.GetComponent<SpriteRenderer>())
        {
            g.transform.GetComponent<SpriteRenderer>().sortingOrder = desiredSortingOrder;
            
            Debug.Log(g.name + " sorting order: " + g.GetComponent<SpriteRenderer>().sortingOrder);
        }
        else if(g.transform.childCount > 0)
        {
            //Debug.Log("Descending hierarchy: " + g.name + " desired sorting order: " + desiredSortingOrder);
            for (int i = 0; i < g.transform.childCount; i++)
            {
                SetSortingOrder(g.transform.GetChild(i).gameObject, desiredSortingOrder, playerSpaceSize, playerSpaceStart, playerSpot);
            }
        }
    }
    bool CatalogEquality(List<SceneObjectLayer> layerA, List<SceneObjectLayer> layerB) 
    {//Just use Equals() on each element.
        for (int i = 0; i < layerA.Count; i++) 
        {
            if (layerA[i].Equals(layerB[i]))
            {
                return false;
            }
        }
        return true;
    }
    IEnumerator UpdateCatalog()
    {
        sceneObjectLayers[1].gameObjects[12].GetComponentInChildren<SpriteRenderer>().sortingOrder = 0;
        while (true) { 
            foreach (SceneObjectLayer sol in sceneObjectLayers)
            {
                sol.SetLayerSpaceFactor(layerSpaceFactor);
                for (int i = 0; i < sol.gameObjects.Count; i++)
                {
                    if (sol.gameObjects[i] != null)
                    {
                        //recursively traverse children to find sprite renderer using SetSortingOrder
                        SetSortingOrder(sol.gameObjects[i], sol.Layer(i), sol.playerSpaceSize, sol.playerSpaceStart, sol.playerSpot);
                        //Debug.Log(sol.gameObjects[i].name + " layer: " + sol.Layer(i));
                    }
                }
                print($"Scene Object Layer Minimum: {sol.Layer(0)}");
            }
            List<SceneObjectLayer> oldSceneObjectLayers = sceneObjectLayers;
            yield return new WaitUntil(() => CatalogEquality(sceneObjectLayers, oldSceneObjectLayers));
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(UpdateCatalog());
    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}
}
