using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

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
            rigidBody.AddForce(Force * Thrust);
        }
    }

    private void Update()
    {
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!GetComponentInParent<BlockMonster>().IsDead)
            GetComponentInParent<BlockMonster>().FitnessPoints += 0.01F;
        //Debug.Log("On Ground " + GetComponentInParent<BlockMonster>().gameObject.name);
    }
}
