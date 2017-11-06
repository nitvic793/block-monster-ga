using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPopulation : MonoBehaviour
{
    public float TimeScale = 1F;
    public float TimeLimit = 30F;
    public float TotalTime = 0F;
    public int Generation = 1;
    public int PopulationCount = 1;
    public List<float[]> PopulationGenomes = null;
    public float MutationProbality = 0.2F;

    private List<GameObject> Monsters = new List<GameObject>();
    SortedDictionary<float, BlockMonster> fitnessDictionary = new SortedDictionary<float, BlockMonster>();

    public GameObject MonsterPrefab;
    public Transform StartPosition;
    public Transform TargetTransform;

    const int GENE_LENGTH = 18;
    private BlockMonster FittestMonster = null;
    // Use this for initialization
    void Start()
    {
        CreatePopulation();
        InstantiatePopulation();
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = TimeScale;
        TotalTime += Time.deltaTime;
        if (TotalTime >= TimeLimit)
        {
            EvaluateFitness();
            SelectAndCrossover();
            Mutate();
            DestroyPopulation();
            InstantiatePopulation();
            TotalTime = 0F;
            Generation++;
        }
        else
        {
            if (FittestMonster != null)
                FittestMonster.DeHighlight();

            FittestMonster = GetFittestMonster();
            if (FittestMonster != null)
            {
                FittestMonster.Highlight();
            }
        }
    }

    private void EvaluateFitness()
    {
        fitnessDictionary.Clear();
        var monsters = GetComponentsInChildren<BlockMonster>();
        foreach (var monster in monsters)
        {
            try
            {
                fitnessDictionary.Add(monster.GetFitness(), monster);
            }
            catch (ArgumentException)
            {
                fitnessDictionary.Add(monster.GetFitness() - UnityEngine.Random.Range(0F, 1F), monster);
            }
        }
    }

    private BlockMonster GetFittestMonster()
    {
        var fitness = 0F;
        var monsters = GetComponentsInChildren<BlockMonster>();
        foreach (var monster in monsters)
        {
            if (monster.GetFitness() > fitness)
            {
                fitness = monster.GetFitness();
                FittestMonster = monster;
            }
        }
        return FittestMonster;
    }

    private float[] GenerateRandomGene()
    {
        var gene = new float[GENE_LENGTH];
        for (var i = 0; i < gene.Length; ++i)
        {
            if (i % 3 == 0)
            {
                gene[i] = UnityEngine.Random.Range(0F, 5F);
            }
            else
            {
                gene[i] = UnityEngine.Random.Range(0F, 60F);
            }
        }

        return gene;
    }

    private void CreatePopulation()
    {
        PopulationGenomes = new List<float[]>();
        for (var i = 0; i < PopulationCount; ++i)
        {
            PopulationGenomes.Add(GenerateRandomGene());
        }
    }

    private void InstantiatePopulation()
    {
        foreach (var gene in PopulationGenomes)
        {
            var monster = Instantiate(MonsterPrefab, transform);
            monster.GetComponent<BlockMonster>().Genome = gene;
            monster.transform.position = StartPosition.position;
            monster.GetComponent<BlockMonster>().ConvertGenomeToVector();
            monster.GetComponent<BlockMonster>().Target = TargetTransform;
            Monsters.Add(monster);
        }
    }

    private void DestroyPopulation()
    {
        foreach (var monster in Monsters)
        {
            Destroy(monster);
        }
        Monsters.Clear();
    }


    private void SelectAndCrossover()
    {
        List<float[]> genomes = new List<float[]>();
        var fitnessWeights = new float[fitnessDictionary.Count];
        int index = 0;
        foreach (var item in fitnessDictionary)
        {
            genomes.Add(item.Value.Genome);
            fitnessWeights[index] = item.Key;
            index++;
        }
        Array.Reverse(fitnessWeights);
        genomes.Reverse();
        PopulationGenomes.Clear();
        PopulationGenomes = genomes;
        Debug.Log("Fitness : " + fitnessWeights[0] + " Generation: " + Generation);
        var fittest = PopulationGenomes[0];
        var secondFittest = PopulationGenomes[1];
        var firstChild = CrossoverGenes(fittest, secondFittest);
        var secondChild = CrossoverGenes(secondFittest, fittest);
        PopulationGenomes[0] = firstChild;
        PopulationGenomes[1] = secondFittest;

        for (var i = 2; i < PopulationGenomes.Count; ++i)
        {
            var parent = PopulationGenomes[i];
            var selectedIndex = RouletteSelect(fitnessWeights);
            var child = CrossoverGenes(parent, PopulationGenomes[selectedIndex]);
            PopulationGenomes[i] = child;
        }
    }

    /// <summary>
    /// Returns the selected index based on the fitness weights(probabilities) - Fitness proportionate selection
    /// </summary>
    /// <param name="fitnessWeights"></param>
    /// <returns></returns>
    int RouletteSelect(float[] fitnessWeights)
    {
        float weightSum = 0;
        for (int i = 0; i < fitnessWeights.Length; i++)
        {
            weightSum += fitnessWeights[i];
        }

        float value = UnityEngine.Random.Range(0F, 1F) * weightSum;
        for (int i = 0; i < fitnessWeights.Length; i++)
        {
            value -= fitnessWeights[i];
            if (value <= 0) return i;
        }

        return fitnessWeights.Length - 1;
    }

    /// <summary>
    /// Uniform crossover
    /// </summary>
    /// <param name="parent1">Parent 1</param>
    /// <param name="parent2">Parent 2</param>
    /// <returns></returns>
    private float[] CrossoverGenes(float[] parent1, float[] parent2)
    {
        var childGenome = new float[GENE_LENGTH];
        var parents = new List<float[]>(new[] { parent1, parent2 });
        for (var i = 0; i < childGenome.Length; ++i)
        {
            int parentSelection = UnityEngine.Random.Range(0, 2);
            childGenome[i] = parents[parentSelection][i];
        }
        return childGenome;
    }

    private void Mutate()
    {
        for (var i = 0; i < PopulationGenomes.Count; ++i)
        {
            var random = UnityEngine.Random.Range(0F, 1F);
            if (random > MutationProbality)
            {
                PopulationGenomes[i] = MutateGenes(PopulationGenomes[i]);
            }
        }
    }

    private float[] MutateGenes(float[] genes)
    {
        var mutatedGenome = genes;
        var randomIndex = UnityEngine.Random.Range(0, GENE_LENGTH);
        if (randomIndex % 3 == 0)
        {
            mutatedGenome[randomIndex] = UnityEngine.Random.Range(0F, 10F);
        }
        else
        {
            mutatedGenome[randomIndex] = UnityEngine.Random.Range(0F, 50F);
        }
        return mutatedGenome;
    }

}
