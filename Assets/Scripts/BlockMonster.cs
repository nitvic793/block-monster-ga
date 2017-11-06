using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockMonster : MonoBehaviour
{
    public List<Vector3> LeftForce = new List<Vector3>();
    public List<Vector3> RightForce;
    public List<Vector3> HeadForce;
    public Transform Target;

    public float GlobalThrust = 1F;
    public float StepTime = 0.5F;
    public bool IsDead = false;

    public MonsterComponent LeftBlock;
    public MonsterComponent RightBlock;
    public MonsterComponent HeadBlock;

    public float FitnessPoints = 0F;

    private float totalTime = 0F;
    private Vector3 ZeroVector = new Vector3(0F, 0F, 0F);
    private int currentStep = 0;
    private Color DefaultColor;
    private float InitialDistanceToTarget;
    private int currentRhythm = 0;

    private const int TOTALSTEPS = 3;
    private const int GENE_LENGTH = 18;
    private const int RHYTHM_LENGTH = 2;
    private const int SEGMENT_LENGTH = 9;

    public float[] Genome = new float[GENE_LENGTH];

    public void ConvertGenomeToVector()
    {
        LeftForce.Clear();
        RightForce.Clear();
        HeadForce.Clear();
        for (var i = 0; i < RHYTHM_LENGTH; ++i)
        {
            var currentIteration = SEGMENT_LENGTH * i;
            LeftForce.Add(new Vector3(Genome[currentIteration + 0], Genome[currentIteration + 1], Genome[currentIteration + 2]));
            RightForce.Add(new Vector3(Genome[currentIteration + 3], Genome[currentIteration + 4], Genome[currentIteration + 5]));
            HeadForce.Add(new Vector3(Genome[currentIteration + 6], Genome[currentIteration + 7], Genome[currentIteration + 8]));
        }
        //LeftForce.Add(new Vector3(Genome[0], Genome[1], Genome[2]));
        //RightForce.Add(new Vector3(Genome[3], Genome[4], Genome[5]));
        //HeadForce.Add(new Vector3(Genome[6], Genome[7], Genome[8]));
        //LeftForce.Add(new Vector3(Genome[9], Genome[10], Genome[11]));
        //RightForce.Add(new Vector3(Genome[12], Genome[13], Genome[14]));
        //HeadForce.Add(new Vector3(Genome[15], Genome[16], Genome[17]));
    }

    void Start()
    {
        ConvertGenomeToVector();
        HeadBlock.Thrust = LeftBlock.Thrust = RightBlock.Thrust = GlobalThrust;
        var targetPosition = Target.position;
        InitialDistanceToTarget = (HeadBlock.GetComponentInParent<Transform>().position - targetPosition).magnitude;
    }

    void Update()
    {
    }

    void FixedUpdate()
    {
        totalTime += Time.deltaTime;
        if (totalTime > StepTime && !IsDead)
        {
            LeftBlock.Force = RightBlock.Force = HeadBlock.Force = ZeroVector;
            switch (currentStep)
            {
                case 0:
                    LeftBlock.Force = LeftForce[currentRhythm];
                    break;
                case 1:
                    RightBlock.Force = RightForce[currentRhythm];
                    break;
                case 2:
                    HeadBlock.Force = HeadForce[currentRhythm];
                    currentRhythm = (currentRhythm + 1) % RHYTHM_LENGTH;
                    break;
            }
            totalTime = 0F;
            currentStep = (currentStep + 1) % TOTALSTEPS;
        }
        else if (IsDead)
        {
            LeftBlock.Force = RightBlock.Force = HeadBlock.Force = ZeroVector;
        }
    }

    public float GetFitness()
    {
        var targetPosition = Target.position;
        var distanceFromTarget = (HeadBlock.GetComponentInParent<Transform>().position - targetPosition).magnitude;
        var fitnessDistance = (InitialDistanceToTarget - distanceFromTarget) * 2;
        if (fitnessDistance < 0 || IsDead) fitnessDistance = 0;
        var totalFitness = (fitnessDistance + FitnessPoints);
        return totalFitness < 0 ? 0 : totalFitness;
    }

    public void Highlight()
    {
        DefaultColor = HeadBlock.gameObject.GetComponent<Renderer>().material.color;
        HeadBlock.gameObject.GetComponent<Renderer>().material.color = Color.green;
    }

    public void DeHighlight()
    {
        HeadBlock.gameObject.GetComponent<Renderer>().material.color = DefaultColor;
    }
}
