using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclesGenerator : MonoBehaviour
{
    // All occupied coords
    public static List<Vector3> occupiedCoords = new List<Vector3>();
    public GameObject obstacle1;
    public GameObject obstacle2;
    public GameObject obstacle3;
    public GameObject obstacle4;
    public GameObject obstacle5;



    // Start is called before the first frame update
    void Start()
    {
        // Obstacles do not overlap the waiting area
        occupiedCoords.Add(new Vector3(-2, 0, 15));
        occupiedCoords.Add(new Vector3(2, 0, 15));
        occupiedCoords.Add(new Vector3(-2, 8, 15));
        occupiedCoords.Add(new Vector3(2, 8, 15));

        // Generate the obstacle
        for(int i = 0; i < 4; i ++){
            Vector3 position = new Vector3(Random.Range(-4, 5), 0, Random.Range(-2, 2));
            while(true){
                if(checkObstacle(position)){
                    break;
                }
                position = new Vector3(Random.Range(-4, 5), 0, Random.Range(-2, 2));
            }
            occupiedCoords.Add(position);
            int mode = Random.Range(1, 6);
            ObstacleGenerator(position, mode);
        }
        // Generate the obstacles in the low level
        for(int i = 0; i < 3; i ++){
            Vector3 position = new Vector3(Random.Range(-4, 5), 0, Random.Range(13, 19));
            while(true){
                if(checkObstacle(position)){
                    break;
                }
                position = new Vector3(Random.Range(-4, 5), 0, Random.Range(13, 19));
            }
            occupiedCoords.Add(position);
            int mode2 = Random.Range(1, 6);
            ObstacleGenerator(position, mode2);
        }
        // Generate the obstacles in the high level
        for(int i = 0; i < 3; i ++){
            Vector3 position = new Vector3(Random.Range(-4, 5), 8, Random.Range(13, 19));
            while(true){
                if(checkObstacle(position)){
                    break;
                }
                position = new Vector3(Random.Range(-4, 5), 8, Random.Range(13, 19));
            }
            occupiedCoords.Add(position);
            int mode3 = Random.Range(1, 6);
            ObstacleGenerator(position, mode3);
        }
    }

    public static bool checkObstacle(Vector3 obstacle){
        foreach(Vector3 position in occupiedCoords){
            if(position == obstacle){
                return false;
            }
        }
        return true;
    }

    // Help generate the obstacles
    void ObstacleGenerator(Vector3 center, int whichObstacle){
        if(whichObstacle > 5 || whichObstacle < 1){
            return;
        }
        if(whichObstacle == 1){
            Instantiate(obstacle1, center, obstacle1.transform.rotation);
        }
        else if(whichObstacle == 2){
            Instantiate(obstacle2, center, obstacle2.transform.rotation);
        }
        else if(whichObstacle == 3){
            Instantiate(obstacle3, center, obstacle3.transform.rotation);
        }
        else if(whichObstacle == 4){
            Instantiate(obstacle4, center, obstacle4.transform.rotation);
        }
        else if(whichObstacle == 5){
            Instantiate(obstacle5, center, obstacle5.transform.rotation);
        }
    }
}
