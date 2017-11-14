// Author: Nitish Victor (nithishvictor@gmail.com)
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Controls the "limbs" of the creature
/// </summary>
public class MonsterComponent : MonoBehaviour
{
    public float Thrust = 1F;
    public Vector3 Force;
    public Rigidbody rigidBody;

    float totalTime = 0f;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float deltaTime = Time.deltaTime;
        totalTime += deltaTime;
        if (totalTime > 0.5F)
        {
            totalTime = 0;
            rigidBody.AddForce(Force * Thrust); // Applies force to this limb.
        }
    }

    private void Update()
    {
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!GetComponentInParent<BlockMonster>().IsDead)
            GetComponentInParent<BlockMonster>().FitnessPoints += 0.01F; // Add fitness points for step taken
        //Debug.Log("On Ground " + GetComponentInParent<BlockMonster>().gameObject.name);
    }
}
