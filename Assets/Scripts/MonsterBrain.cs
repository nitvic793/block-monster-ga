using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBrain : MonoBehaviour {


    private void OnCollisionEnter(Collision collision)
    {
        //Just checks if the creature has fallen down. If so, it is considered to be dead.
        if(collision.gameObject.name == "Terrain")
        {
            GetComponentInParent<BlockMonster>().IsDead = true;
        }
        else
        {
            GetComponentInParent<BlockMonster>().IsDead = false;
        }
    }

    void Start () {
		
	}
	
	void Update () {
		
	}
}
