using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    // Used for the picked algorithm
    private List<Node> openList = new List<Node>();
    private HashSet<Node> closeSet = new HashSet<Node>();
    private List<Node> allNodes = new List<Node>();
    private Stack<Node> path = new Stack<Node>();

    // Start and end
    public Vector3 start;
    public Vector3 end;
    // May collide with other npcs
    public Vector3 avoid = new Vector3(-100, -100, -100);
    // Statistics
    public int numberOfPathing = 0;
    public float totalTime = 0;


    // Start is called before the first frame update
    void Start()
    {
        Invoke("Initialize", 0.1f);
    }

    // Call corresponding algorithm based on the mode number
    public void GeneratePath(){
        numberOfPathing ++;
        if(NPCGenerator.mode == 0){
            GenerateAStar();
        }
        else{
            GenerateJPS();
        }
    }  

    // Help to call the main A star algorithm
    public void GenerateAStar(){
        try{
            AStar(start, end);
        }
        catch(System.Exception){
            end = SelectDestination();
        }
        while(path.Count == 0){
            end = SelectDestination();
            try{
                AStar(start, end);
            }
            catch(System.Exception){
                end = SelectDestination();
            }
        }
    }

    // Help to call the main Jump points Search Algorithm
    public void GenerateJPS(){
        try{
            JPS(start, end);
        }
        catch(System.Exception){
            end = SelectDestination();
        }
        while(path.Count == 0){
            end = SelectDestination();
            try{
                JPS(start, end);
            }
            catch(System.Exception){
                end = SelectDestination();
            }
        }
    }

    // Walk to the next node on the path
    public void Walk(){
        if(path.Count != 0){
            Vector3 pos = path.Pop().position + new Vector3(0, 0.25f, 0);
            gameObject.transform.position = pos;
            start = pos;
        }
    }

    // Update is called once per frame
    void Update()
    {   
    }

    public Vector3 SelectDestination(){
        int index = Random.Range(0, allNodes.Count);
        while(isTeleport(allNodes[index].position) || onBridge(allNodes[index].position)){
            index = Random.Range(0, allNodes.Count);
        }
        return allNodes[index].position;
    }

    void Initialize(){
        // Plane
        for(int x = -4; x < 5; x ++){
            for(int z = -2; z < 3; z ++){
                Vector3 pos = new Vector3(x, 0, z);
                if(ObstaclesGenerator.checkObstacle(pos) || isTeleport(pos)){
                    allNodes.Add(new Node(pos));
                }
            }
        }
        // Low
        for(int x = -4; x < 5; x ++){
            for(int z = 12; z < 19; z ++){
                Vector3 pos = new Vector3(x, 0, z);
                if(ObstaclesGenerator.checkObstacle(pos) || isTeleport(pos)){
                    allNodes.Add(new Node(pos));
                }
            }
        }
        // High
        for(int x = -4; x < 5; x ++){
            for(int z = 12; z < 19; z ++){
                Vector3 pos = new Vector3(x, 8, z);
                if(ObstaclesGenerator.checkObstacle(pos) || isTeleport(pos)){
                    allNodes.Add(new Node(pos));
                }
            }
        }
        // bridge1
        int y1 = 0;
        for(int z = 3; z < 12; z ++){
            Vector3 pos = new Vector3(-3, y1, z);
            allNodes.Add(new Node(pos));
            y1 ++;
        }
        // bridge2
        int y2 = 0;
        for(int z = 3; z < 12; z ++){
            Vector3 pos = new Vector3(3, y2, z);
            allNodes.Add(new Node(pos));
            y2 ++;
        }
        // bridge3
        for(int z = 3; z < 12; z ++){
            Vector3 pos = new Vector3(0, 0, z);
            allNodes.Add(new Node(pos));
        }
        // Initialize the start and destination
        start = gameObject.transform.position + new Vector3(0, -0.25f, 0);
        end = SelectDestination();
        GeneratePath();
    }

    // Check if the given position is in the waiting area
    public bool isTeleport(Vector3 position){
        if(position == new Vector3(-2, 0, 15)){
            return true;
        }
        if(position == new Vector3(2, 0, 15)){
            return true;
        }
        if(position == new Vector3(-2, 8, 15)){
            return true;
        }
        if(position == new Vector3(2, 8, 15)){
            return true;
        }
        return false;
    }

    // Check if the given position is on the bridge
    public bool onBridge(Vector3 position){
        return position.z < 12 && position.z > 2;
    }

    // Main A star algorithm
    private void AStar(Vector3 start, Vector3 destination){
        float startTime = Time.realtimeSinceStartup;
        Node startNode = new Node(start);
        startNode.gCost = 0;
        startNode.hCost = (int) Vector3.Distance(start, destination);
        Node endNode = new Node(destination);
        endNode.gCost = (int) Vector3.Distance(start, destination);
        endNode.hCost = 0;
        openList.Clear();
        closeSet.Clear();
        path.Clear();
        openList.Add(startNode);
        while(openList.Count > 0){
            Node currentNode = openList[0];
            for(int i = 0; i < openList.Count; i ++){
                Node node = openList[i];
                if(node.FCost() < currentNode.FCost() || node.FCost() == currentNode.FCost() && node.hCost < currentNode.hCost){
                    currentNode = node;
                }
            }
            openList.Remove(currentNode);
            closeSet.Add(currentNode);
            // If Found
            if(currentNode.Equals(endNode)){
                while(!(currentNode.Equals(startNode))){
                    path.Push(currentNode);
                    currentNode = currentNode.parent;
                }
                path.Push(path.Peek().parent);
                totalTime += (Time.realtimeSinceStartup - startTime);
                return;
            }
            // Get all neighbours
            List<Node> neighbours = GetNeighbour(currentNode);
            foreach(Node node in neighbours){
                if(closeSet.Contains(node)){
                    continue;
                }
                int gCost = currentNode.gCost + GetDistanceNode(currentNode, node);
                if(gCost < node.gCost || !Contain(openList, node)){
                    node.gCost = gCost;
                    node.hCost = GetDistanceNode(node, endNode);
                    node.parent = currentNode;
                    if(!Contain(openList, node)){
                        openList.Add(node);
                    }
                }
            }
        }
        totalTime += (Time.realtimeSinceStartup - startTime);
    }

    // Get all neighbour for A star algorithm
    public List<Node> GetNeighbour(Node current){
        List<Node> neighbours = new List<Node>();
        // Consider 10 possible nodes and check if they are walkable
        Node n1 = new Node(current.position + new Vector3(1, 0, 0));
        Node n2 = new Node(current.position + new Vector3(-1, 0, 0));
        Node n3 = new Node(current.position + new Vector3(0, 0, 1));
        Node n4 = new Node(current.position + new Vector3(0, 0, -1));
        Node n5 = new Node(current.position + new Vector3(0, 1, 1));
        Node n6 = new Node(current.position + new Vector3(0, -1, -1));
        Node n7 = new Node(current.position + new Vector3(1, 0, 1));
        Node n8 = new Node(current.position + new Vector3(-1, 0, -1));
        Node n9 = new Node(current.position + new Vector3(1, 0, -1));
        Node n10 = new Node(current.position + new Vector3(-1, 0, 1));

        // Check the teleport first
        if(current.position == new Vector3(-2, 0, 15)){
            Node tmp = new Node(new Vector3(-2, 8, 15));
            if(Contain(allNodes, tmp) && ifAvoid(tmp.position)){
                neighbours.Add(tmp);
            }
        }
        if(current.position == new Vector3(2, 0, 15)){
            Node tmp2 = new Node(new Vector3(2, 8, 15));
            if(Contain(allNodes, tmp2) && ifAvoid(tmp2.position)){
                neighbours.Add(tmp2);
            }
        }
        if(current.position == new Vector3(-2, 8, 15)){
            Node tmp3 = new Node(new Vector3(-2, 0, 15));
            if(Contain(allNodes, tmp3) && ifAvoid(tmp3.position)){
                neighbours.Add(tmp3);
            }
        }
        if(current.position == new Vector3(2, 8, 15)){
            Node tmp4 = new Node(new Vector3(2, 0, 15));
            if(Contain(allNodes, tmp4) && ifAvoid(tmp4.position)){
                neighbours.Add(tmp4);
            }
        }
        // Check if they are walkable
        if(Contain(allNodes, n1) && ifAvoid(n1.position)){
            neighbours.Add(n1);
        }
        if(Contain(allNodes, n2) && ifAvoid(n2.position)){
            neighbours.Add(n2);
        }
        if(Contain(allNodes, n3) && ifAvoid(n3.position)){
            neighbours.Add(n3);
        }
        if(Contain(allNodes, n4) && ifAvoid(n4.position)){
            neighbours.Add(n4);
        }
        if(Contain(allNodes, n5) && ifAvoid(n5.position)){
            neighbours.Add(n5);
        }
        if(Contain(allNodes, n6) && ifAvoid(n6.position)){
            neighbours.Add(n6);
        }
        if(Contain(allNodes, n7) && ifAvoid(n7.position)){
            neighbours.Add(n7);
        }
        if(Contain(allNodes, n8) && ifAvoid(n8.position)){
            neighbours.Add(n8);
        }
        if(Contain(allNodes, n9) && ifAvoid(n9.position)){
            neighbours.Add(n9);
        }
        if(Contain(allNodes, n10) && ifAvoid(n10.position)){
            neighbours.Add(n10);
        }
        return neighbours;
    }

    // Helper function, similar to List.Contains()
    public bool Contain(List<Node> lst, Node node){
        foreach(Node n in lst){
            if(n.position == node.position){
                return true;
            }
        }
        return false;
    }

    // set the distance between two teleports to zero
    public int GetDistanceNode(Node node1, Node node2){
        
        if(node1.position == new Vector3(-2, 0, 15) && node2.position == new Vector3(-2, 8, 15)){
            return 0;
        }
        if(node1.position == new Vector3(-2, 8, 15) && node2.position == new Vector3(-2, 0, 15)){
            return 0;
        }
        if(node1.position == new Vector3(2, 8, 15) && node2.position == new Vector3(2, 0, 15)){
            return 0;
        }
        if(node1.position == new Vector3(2, 0, 15) && node2.position == new Vector3(2, 8, 15)){
            return 0;
        }
        return (int) Vector3.Distance(node1.position, node2.position);
    }

    public Vector3 NextStep(){
        if(path.Count != 0){
            return path.Peek().position;
        }
        return new Vector3(-100, -100, -100);
    }

    public bool ifAvoid(Vector3 position){
        return position != avoid;
    }


    // ---------------------Jump Node Search-----------------------
    // Main JPS algorithm
    void JPS(Vector3 start, Vector3 end){
        float startTime = Time.realtimeSinceStartup;
        Node startNode = new Node(start);
        Node endNode = new Node(end);
        openList.Clear();
        closeSet.Clear();
        path.Clear();
        openList.Add(startNode);
        while(openList.Count > 0){
            Node currentNode = openList[0];
            for(int i = 0; i < openList.Count; i ++){
                Node node = openList[i];
                if(node.fCost < currentNode.fCost || (node.fCost == currentNode.fCost && node.hCost < currentNode.hCost)){
                    currentNode = node;
                }
            }
            openList.Remove(currentNode);
            closeSet.Add(currentNode);
            // If Found
            if(currentNode.position == endNode.position){
                // need to generate path, since we only have the jump nodes in open list
                JPSGeneratePath(currentNode);
                totalTime += (Time.realtimeSinceStartup - startTime);
                return;
            }
            // Only get the forced neighbours here
            List<Node> neighbours = GetForcedNeighbours(currentNode);
            foreach(Node neighbour in neighbours){
                Node jump = GetJumpNode(currentNode.position, neighbour.position);
                if(jump != null){
                    if(closeSet.Contains(jump)){
                        continue;
                    }
                    int g = currentNode.gCost + CalculateG(currentNode, jump);
                    if(g < jump.gCost || !Contain(openList, jump)){
                        jump.gCost = g;
                        jump.hCost = CalculateH(jump, endNode);
                        jump.UpdateF();
                        jump.parent = currentNode;
                        if(!Contain(openList, jump)){
                            openList.Add(jump);
                        }
                    }
                }
            }
        }
        totalTime += (Time.realtimeSinceStartup - startTime);
    }

    // Help generate the path
    void JPSGeneratePath(Node currentNode){
        Stack<Node> tmp = new Stack<Node>();
        Stack<Node> jumpPoints = new Stack<Node>();
        while(!(currentNode.position == start)){
            tmp.Push(currentNode);
            currentNode = currentNode.parent;
        }
        tmp.Push(tmp.Peek().parent);

        while(tmp.Count != 0){
            jumpPoints.Push(tmp.Pop());
        }

        Node n1 = jumpPoints.Pop();
        Node n2 = jumpPoints.Pop();
        while(jumpPoints.Count > 0){
            foreach(Node node in PathBetweenTwoNodes(n1, n2)){
                path.Push(node);
            }
            n1 = n2;
            n2 = jumpPoints.Pop();
        }
        foreach(Node node in PathBetweenTwoNodes(n1, n2)){
            path.Push(node);
        }
    }
    // Generate path from n1 to n2
    List<Node> PathBetweenTwoNodes(Node n1, Node n2){
        // Same x, Same y
        List<Node> path = new List<Node>();
        if((n1.position.x == n2.position.x) && (n1.position.y == n2.position.y)){
            if(n1.position.z > n2.position.z){
                for(int z = (int)n1.position.z; z >= n2.position.z; z -- ){
                    if(IsWalkable(new Vector3(n1.position.x, n1.position.y, z))){
                        path.Add(new Node(new Vector3(n1.position.x, n1.position.y, z)));
                    }
                }
                return path;
            }
            else{
                for(int z = (int)n1.position.z; z <= n2.position.z; z ++ ){
                    if(IsWalkable(new Vector3(n1.position.x, n1.position.y, z))){
                        path.Add(new Node(new Vector3(n1.position.x, n1.position.y, z)));
                    }
                }
                return path;
            }
        }
        // Same z, Same y
        if((n1.position.z == n2.position.z) && (n1.position.y == n2.position.y)){
            if(n1.position.x > n2.position.x){
                for(int x = (int)n1.position.x; x >= n2.position.x; x -- ){
                    if(IsWalkable(new Vector3(x, n1.position.y, n1.position.z))){
                        path.Add(new Node(new Vector3(x, n1.position.y, n1.position.z)));
                    }
                }
                return path;
            }
            else{
                for(int x = (int)n1.position.x; x <= n2.position.x; x ++ ){
                    if(IsWalkable(new Vector3(x, n1.position.y, n1.position.z))){
                        path.Add(new Node(new Vector3(x, n1.position.y, n1.position.z)));
                    }
                }
                return path;
            }
        }
        // Same y, diff x, diff z, diagonal
        if(n1.position.y == n2.position.y){
            if(n1.position.x > n2.position.x){
                int z = (int) n1.position.z;
                for(int x = (int)n1.position.x; x >=  n2.position.x; x --){
                    if(IsWalkable(new Vector3(x, n1.position.y, z))){
                        path.Add(new Node(new Vector3(x, n1.position.y, z)));
                    }
                    if(n1.position.z > n2.position.z){
                        z --;
                    }
                    else{  
                        z ++;
                    }
                }
                return path;
            }
            else{
                int z = (int) n1.position.z;
                for(int x = (int)n1.position.x; x <=  n2.position.x; x ++){
                    if(IsWalkable(new Vector3(x, n1.position.y, z))){
                        path.Add(new Node(new Vector3(x, n1.position.y, z)));
                    }
                    if(n1.position.z > n2.position.z){
                        z --;
                    }
                    else{  
                        z ++;
                    }
                }
                return path;
            }
        }

        // left bridge, from lower to upper
        if((n2.position.y > n1.position.y) && (n1.position.x < 0)){
            int y1 = 0;
            for(int z = 3; z < 12; z ++){
                Vector3 pos = new Vector3(-3, y1, z);
                if(IsWalkable(pos)){
                    path.Add(new Node(pos));
                }
                y1 ++;
            }
            return path;
        }
        // right bridge, from lower to upper
        if((n2.position.y > n1.position.y) && (n1.position.x > 0)){
            int y2 = 0;
            for(int z = 3; z < 12; z ++){
                Vector3 pos = new Vector3(3, y2, z);
                if(IsWalkable(pos)){
                    path.Add(new Node(pos));
                }
                y2 ++;
            }
            return path;
        }
        // left bridge, from upper to lower
        if((n2.position.y < n1.position.y) && (n1.position.x < 0)){
            int y1 = 8;
            for(int z = 11; z > 3; z --){
                Vector3 pos = new Vector3(-3, y1, z);
                if(IsWalkable(pos)){
                    path.Add(new Node(pos));
                }
                y1 --;
            }
            return path;
        }
        // right bridge, from upper to lower
        if((n2.position.y < n1.position.y) && (n1.position.x > 0)){
            int y2 = 8;
            for(int z = 11; z > 3; z --){
                Vector3 pos = new Vector3(3, y2, z);
                if(IsWalkable(pos)){
                    path.Add(new Node(pos));
                }
                y2 --;
            }
            return path;
        }
        // Other case, we simply add the jump nodes
        path.Add(n1);
        path.Add(n2);
        return path;
    }

    // To get the forced neighbours
    public List<Node> GetForcedNeighbours(Node current){
        List<Node> all = new List<Node>();
        Node parent = current.parent;
        Vector3 pos = current.position;
        if(parent == null){
            return GetNeighbour(current);
        }
        // if not the start node
        int xDirection = Mathf.Clamp((int)(pos.x - parent.position.x), -1, 1);
        int zDirection = Mathf.Clamp((int)(pos.z - parent.position.z), -1, 1);
        if (xDirection != 0 && zDirection != 0)
        {
            //diagonal
            bool neighbourForward =IsWalkable(new Vector3(pos.x, pos.y, pos.z + zDirection));
            bool neighbourBack =IsWalkable(new Vector3(pos.x, pos.y, pos.z - zDirection));
            bool neighbourRight =IsWalkable(new Vector3(pos.x + xDirection, pos.y, pos.z));
            bool neighbourLeft =IsWalkable(new Vector3(pos.x - xDirection, pos.y, pos.z));
            if (neighbourForward)
            {
                Vector3 newPos = new Vector3(pos.x, pos.y, pos.z + zDirection);
                all.Add(new Node(newPos));
            }
            if (neighbourRight)
            {
                Vector3 newPos = new Vector3(pos.x + xDirection, pos.y, pos.z);
                all.Add(new Node(newPos));
            }
            if ((neighbourForward || neighbourRight) && IsWalkable(new Vector3(pos.x + xDirection, pos.y, pos.z + zDirection)))
            {
                Vector3 newPos = new Vector3(pos.x + xDirection, pos.y, pos.z + zDirection);
                all.Add(new Node(newPos));
            }
            //handle the forced neighbours
            if (!neighbourLeft && neighbourForward)
            {
                if (IsWalkable(new Vector3(pos.x - xDirection, pos.y, pos.z + zDirection)))
                {
                    Vector3 newPos = new Vector3(pos.x - xDirection, pos.y, pos.z + zDirection);
                    all.Add(new Node(newPos));
                }
            }
            if (!neighbourBack && neighbourRight)
            {
                if (IsWalkable(new Vector3(pos.x + xDirection, pos.y, pos.z - zDirection)))
                {
                    Vector3 newPos = new Vector3(pos.x + xDirection, pos.y, pos.z - zDirection);
                    all.Add(new Node(newPos));
                }
            }
        }
        else
        {
            if (xDirection == 0)
            {
                // Z direction
                if (IsWalkable(new Vector3(pos.x, pos.y, pos.z + zDirection)))
                {
                    Vector3 newPos = new Vector3(pos.x , pos.y, pos.z + zDirection);
                    all.Add(new Node(newPos));
                    // handle the forced neighbours
                    if (!IsWalkable(new Vector3(pos.x + 1, pos.y, pos.z)) &&IsWalkable(new Vector3(pos.x + 1, pos.y, pos.z + zDirection)))
                    {
                        Vector3 newPos2 = new Vector3(pos.x + 1, pos.y, pos.z + zDirection);
                        all.Add(new Node(newPos2));
                    }
                    if (!IsWalkable(new Vector3(pos.x - 1, pos.y, pos.z)) &&IsWalkable(new Vector3(pos.x - 1, pos.y, pos.z + zDirection)))
                    {
                        Vector3 newPos3 = new Vector3(pos.x - 1, pos.y, pos.z + zDirection);
                        all.Add(new Node(newPos3));
                    }
                }
            }
            else
            {
                // x direction
                if (IsWalkable(new Vector3(pos.x + xDirection, pos.y, pos.z)))
                {
                    Vector3 newPos = new Vector3(pos.x + xDirection, pos.y, pos.z);
                    all.Add(new Node(newPos));
                    // handle the forced neighbours
                    if (!IsWalkable(new Vector3(pos.x, pos.y, pos.z + 1)) &&IsWalkable(new Vector3(pos.x + xDirection, pos.y, pos.z + 1)))
                    {
                        Vector3 newPos2 = new Vector3(pos.x + xDirection, pos.y, pos.z + 1);
                        all.Add(new Node(newPos2));
                    }
                    if (!IsWalkable(new Vector3(pos.x, pos.y, pos.z - 1)) &&IsWalkable(new Vector3(pos.x + xDirection, pos.y, pos.z - 1)))
                    {
                        Vector3 newPos3 = new Vector3(pos.x + xDirection, pos.y, pos.z - 1);
                        all.Add(new Node(newPos3));
                    }
                }
            }
        }
        return all;
        
    }

    // Help get the jump node
    Node GetJumpNode(Vector3 current, Vector3 neighbour){
        int x = (int) (neighbour.x - current.x);
        int y = (int) (neighbour.y - current.y);
        int z = (int) (neighbour.z - current.z);
        return FindJumpPoint(neighbour, x, y, z, 100);
    }
    // Recursive method to get the jump node
    private Node FindJumpPoint(Vector3 current, int xDirection, int yDirection, int zDirection, int depth){
        if(!IsWalkable(current)){
            return null;
        }
        if(depth == 0 || (current == end)){
            return new Node(current);
        }

        // Diagonal
        if(xDirection != 0 && zDirection != 0){
            if((IsWalkable(new Vector3(current.x + xDirection, current.y, current.z - zDirection)) 
            && !IsWalkable(new Vector3(current.x, current.y, current.z - zDirection))) 
            || (IsWalkable(new Vector3(current.x - xDirection, current.y, current.z + zDirection)) 
            && !IsWalkable(new Vector3(current.x - xDirection, current.y, current.z)))){
                return new Node(current);
            }
            if(FindJumpPoint(new Vector3(current.x + xDirection, current.y, current.z), xDirection, yDirection, 0, depth - 1) != null){
                return new Node(current);
            }
            if(FindJumpPoint(new Vector3(current.x, current.y, current.z + zDirection), 0, yDirection, zDirection, depth - 1) != null){
                return new Node(current);
            }
        }
        // Horizontal
        else if(xDirection != 0){
            if((IsWalkable(new Vector3(current.x + xDirection, current.y, current.z + 1)) 
            && !IsWalkable(new Vector3(current.x, current.y, current.z + 1))) 
            || (IsWalkable(new Vector3(current.x + xDirection, current.y, current.z - 1)) 
            && !IsWalkable(new Vector3(current.x, current.y, current.z - 1)))){
                return new Node(current);
            }
        }
        // Vertical
        else if(zDirection != 0){
            if((IsWalkable(new Vector3(current.x + 1, current.y, current.z + zDirection)) 
            && !IsWalkable(new Vector3(current.x + 1, current.y, current.z))) 
            || (IsWalkable(new Vector3(current.x -1, current.y, current.z + zDirection)) 
            && !IsWalkable(new Vector3(current.x - 1, current.y, current.z)))){
                return new Node(current);
            }
        }



        // -------------Need to modify-------------------
        // Considering cross the bridge1 and bridge2
        if((current == new Vector3(3, 0, 2)) && (end.y == 8)){
            if(IsWalkable(current)){
                return new Node(current);
            }
        }
        if((current == new Vector3(-3, 0, 2)) && (end.y == 8)){
            if(IsWalkable(current)){
                return new Node(current);
            }
            
        }
        if((current == new Vector3(3, 8, 12)) && (end.y == 0)){
            if(IsWalkable(current)){
                return new Node(current);
            }
        }
        if((current == new Vector3(-3, 8, 12)) && (end.y == 0)){
            if(IsWalkable(current)){
                return new Node(current);
            }
        }

        
        if((current == new Vector3(3, 0, 3)) && (end.y == 8)){
            Vector3 newPos = new Vector3(3, 8, 11);
            if(IsWalkable(newPos)){
                return new Node(newPos);
            }
        }
        if((current == new Vector3(-3, 0, 3)) && (end.y == 8)){
            Vector3 newPos = new Vector3(-3, 8, 11);
            if(IsWalkable(newPos)){
                return new Node(newPos);
            }
        }
        if((current == new Vector3(3, 8, 11)) && (end.y == 0)){
            Vector3 newPos = new Vector3(3, 0, 3);
            if(IsWalkable(newPos)){
                return new Node(newPos);
            }
        }
        if((current == new Vector3(-3, 8, 11)) && (end.y == 0)){
            Vector3 newPos = new Vector3(-3, 0, 3);
            if(IsWalkable(newPos)){
                return new Node(newPos);
            }
        }

        // Teleport
        if((current == new Vector3(-2, 0, 15)) && (end.y == 8)){
            Vector3 newPos = new Vector3(-2, 8, 15);
            if(IsWalkable(newPos)){
                return new Node(newPos);
            }
        }
        if((current == new Vector3(2, 0, 15)) && (end.y == 8)){
            Vector3 newPos = new Vector3(2, 8, 15);
            if(IsWalkable(newPos)){
                return new Node(newPos);
            }
        }
        if((current == new Vector3(-2, 8, 15)) && (end.y == 0)){
            Vector3 newPos = new Vector3(-2, 0, 15);
            if(IsWalkable(newPos)){
                return new Node(newPos);
            }
        }
        if((current == new Vector3(2, 8, 15)) && (end.y == 0)){
            Vector3 newPos = new Vector3(2, 0, 15);
            if(IsWalkable(newPos)){
                return new Node(newPos);
            }
        }

        Vector3 next = new Vector3(current.x + xDirection, current.y, current.z + zDirection);
        return FindJumpPoint(next, xDirection, yDirection, zDirection, depth - 1);
    }

    // Helper function
    public bool IsWalkable(Vector3 position){
        return Contain(allNodes, new Node(position)) && ifAvoid(position)
        && ObstaclesGenerator.checkObstacle(position);
    }

    // To calculate the g cost
    int CalculateG(Node startNode, Node node){
        if(startNode.position == new Vector3(-2, 0, 15) && node.position == new Vector3(-2, 8, 15)){
            return 0;
        }
        if(startNode.position == new Vector3(-2, 8, 15) && node.position == new Vector3(-2, 0, 15)){
            return 0;
        }
        if(startNode.position == new Vector3(2, 8, 15) && node.position == new Vector3(2, 0, 15)){
            return 0;
        }
        if(startNode.position == new Vector3(2, 0, 15) && node.position == new Vector3(2, 8, 15)){
            return 0;
        }
        return (int) Vector3.Distance(startNode.position, node.position);
    }

    // To calculate the h cost
    int CalculateH(Node endNode, Node node){
        if(endNode.position == new Vector3(-2, 0, 15) && node.position == new Vector3(-2, 8, 15)){
            return 0;
        }
        if(endNode.position == new Vector3(-2, 8, 15) && node.position == new Vector3(-2, 0, 15)){
            return 0;
        }
        if(endNode.position == new Vector3(2, 8, 15) && node.position == new Vector3(2, 0, 15)){
            return 0;
        }
        if(endNode.position == new Vector3(2, 0, 15) && node.position == new Vector3(2, 8, 15)){
            return 0;
        }
        return (int) Vector3.Distance(endNode.position, node.position);
    }
}
