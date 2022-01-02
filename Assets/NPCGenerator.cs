using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCGenerator : MonoBehaviour
{
    // the mode number to change the path finding algorithm
    public int modeNumber;
    public static int mode;
    // Simulation, number of NPCs
    public int numberOfNPCs;
    public GameObject NPC;
    private List<Vector3> cannotUsed = new List<Vector3>();
    public static List<GameObject> allNPCs = new List<GameObject>();

    // Switch between two algorithm
    // public static int algorithm;

    private float timer;

    private float time = 120f;

    private int npcOrder;

    private bool flag1 = true;
    private int numberOfRepathing = 0;
    private int numberOfAbandoned = 0;

    // Start is called before the first frame update
    void Start()
    {
        mode = modeNumber;
        timer = 0.1f / (float) numberOfNPCs;
        Invoke("GenerateNPCs", 0.1f);
    }

    void Update(){
        if(time > 0){
            time -= Time.deltaTime;
            if(allNPCs.Count != 0){
                if(timer > 0){
                    timer -= Time.deltaTime;
                }
                else{
                    WalkByOrder();
                    npcOrder ++;
                    if(npcOrder == allNPCs.Count){
                        npcOrder = 0;
                    }
                    timer = 0.1f / (float) numberOfNPCs;
                }
            }
        }
        else{
            if(flag1){
                flag1 = false;
                Debug.Log(getTotalPathingNumber());
                Debug.Log(getTotalTime());
                Debug.Log(numberOfRepathing);
                Debug.Log(numberOfAbandoned);
            }
        }
    }

    public static int getTotalPathingNumber(){
        int num = 0;
        foreach(GameObject o in NPCGenerator.allNPCs){
            num += o.GetComponent<PathFinding>().numberOfPathing;
        }
        return num;
    }

    // Get the total time
    public static float getTotalTime(){
        float time = 0;
        foreach(GameObject o in NPCGenerator.allNPCs){
            time += o.GetComponent<PathFinding>().totalTime;
        }
        return time;
    }

    // Check if collide with other npcs
    public static bool ifOtherNPCs(Vector3 position){
        foreach(GameObject o in NPCGenerator.allNPCs){
            if((o.transform.position + new Vector3(0, -0.25f, 0))  == position){
                return true;
            }
        }
        return false;
    }

    // let all npcs walk one by one
    void WalkByOrder(){
        PathFinding npc = allNPCs[npcOrder].GetComponent<PathFinding>();
        Vector3 next = npc.NextStep();
        // Debug.Log(ifOtherNPCs(next));
        if(ifOtherNPCs(next)){
            npc.avoid = new Vector3(next.x, next.y, next.z);
            npc.start = allNPCs[npcOrder].transform.position + new Vector3(0, -0.25f, 0);
            numberOfRepathing ++;
            npc.GeneratePath();
        }
        npc.Walk();
        if((allNPCs[npcOrder].transform.position + new Vector3(0, -0.25f, 0)) == npc.end){
            npc.start = allNPCs[npcOrder].transform.position + new Vector3(0, -0.25f, 0);
            npc.end = npc.SelectDestination();
            // numberOfRepathing ++;
            numberOfAbandoned ++;
            npc.GeneratePath();
        }
    }

    // Generate npcs based on the given number
    void GenerateNPCs(){
        for(int i = 0; i < numberOfNPCs; i ++){
            GenerateNPC();
        }
    }

    // Randomly generate npc on three different planes
    void GenerateNPC(){
        // Choose a plane
        int mode = Random.Range(0, 3);
        if(mode == 0){
            Vector3 position = new Vector3(Random.Range(-4, 5), 0.25f, Random.Range(-2, 3));
            while(true){
                if(checkOthers(position) && ObstaclesGenerator.checkObstacle(position - new Vector3(0, 0.25f, 0))){
                    break;
                }
                position = new Vector3(Random.Range(-4, 5), 0.25f, Random.Range(-2, 3));
            }
            cannotUsed.Add(position);
            GameObject npc = Instantiate(NPC, position, NPC.transform.rotation);
            npc.GetComponent<MeshRenderer>().material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            allNPCs.Add(npc);
        }
        else if(mode == 1){
            Vector3 position = new Vector3(Random.Range(-4, 5), 0.25f, Random.Range(13, 19));
            while(true){
                if(checkOthers(position) && ObstaclesGenerator.checkObstacle(position - new Vector3(0, 0.25f, 0))){
                    break;
                }
                position = new Vector3(Random.Range(-4, 5), 0.25f, Random.Range(13, 19));
            }
            cannotUsed.Add(position);
            GameObject npc = Instantiate(NPC, position, NPC.transform.rotation);
            npc.GetComponent<MeshRenderer>().material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            allNPCs.Add(npc);
        }
        else if(mode == 2){
            Vector3 position = new Vector3(Random.Range(-4, 5), 8.25f, Random.Range(13, 19));
            while(true){
                if(checkOthers(position) && ObstaclesGenerator.checkObstacle(position - new Vector3(0, 0.25f, 0))){
                    break;
                }
                position = new Vector3(Random.Range(-4, 5), 8.25f, Random.Range(13, 19));
            }
            cannotUsed.Add(position);
            GameObject npc = Instantiate(NPC, position, NPC.transform.rotation);
            npc.GetComponent<MeshRenderer>().material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            allNPCs.Add(npc);
        }
    }
    bool checkOthers(Vector3 npc){
        foreach(Vector3 position in cannotUsed){
            if(position == npc){
                return false;
            }
        }
        return true;
    }
}
