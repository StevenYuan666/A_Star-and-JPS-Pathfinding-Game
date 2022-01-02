using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector3 position;
    public int gCost;
    public int hCost;
    public int fCost;
    public Node parent;

    public int FCost(){
        return fCost;
    }

    public void UpdateF(){
        fCost = gCost + hCost;
    }
    public Node(Vector3 position){
        this.position = position;
        this.parent = null;
    }

    // Override the equals method, to compare two Nodes
    public override bool Equals(object obj)
    {
        if((obj == null) || !this.GetType().Equals(obj.GetType())){
            return false;
        }
        else{
            Node node = (Node) obj;
            return (this.position == node.position);
        }
    }

    public override int GetHashCode()
    {
        return this.position.GetHashCode();
    }

}
