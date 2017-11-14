using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Controls all the population.
/// </summary>
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
    private string GenerationDataFileName = null;
    private string FittestDataFileName = null;
    private StreamWriter GenerationDataStream = null;
    private StreamWriter FittestDataStream = null;

    void Start()
    {
        CreatePopulation();
        InstantiatePopulation();
        InitializeLogFiles();
        Application.runInBackground = true;
    }

    void Update()
    {
        Time.timeScale = TimeScale;
        TotalTime += Time.deltaTime;
        if (TotalTime >= TimeLimit)
        {
            EvaluateFitness(); //Check fitness
            SelectAndCrossover(); //Select and crossover
            Mutate();// Mutate population
            LogGenerationData();
            LogFittestData();
            DestroyPopulation();
            InstantiatePopulation();
            TotalTime = 0F;
            Generation++; //Increment Generation counter
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

    /// <summary>
    /// Evaluates fitness of the population and maps them into a dictionary
    /// </summary>
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

    /// <summary>
    /// Get the current fittest gene in the gene pool.
    /// </summary>
    /// <returns></returns>
    public BlockMonster GetFittestMonster()
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

    /// <summary>
    /// Create a random Gene
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Create population with random genes
    /// </summary>
    private void CreatePopulation()
    {
        PopulationGenomes = new List<float[]>();
        for (var i = 0; i < PopulationCount; ++i)
        {
            PopulationGenomes.Add(GenerateRandomGene());
        }
    }

    /// <summary>
    /// Instantiate population using the available genes
    /// </summary>
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

    /// <summary>
    /// Destroys all the instances of BlockMonster
    /// </summary>
    private void DestroyPopulation()
    {
        foreach (var monster in Monsters)
        {
            Destroy(monster);
        }
        Monsters.Clear();
        fitnessDictionary.Clear();
    }

    /// <summary>
    /// Select population based on Fitness and Crossover. 
    /// <para>Note: Uses a mix of Elitism and Roulette selection. Crossover is performed using Uniform Crossover with 50% chance. </para>
    /// </summary>
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
        PopulationGenomes = genomes; //Sorted genes

        Debug.Log("Fitness : " + fitnessWeights[0] + " Generation: " + Generation);

        var fittest = PopulationGenomes[0];
        var secondFittest = PopulationGenomes[1];
        var firstChild = CrossoverGenes(fittest, secondFittest);
        var secondChild = CrossoverGenes(secondFittest, fittest);

        //Crossed over using the top two genes. 
        PopulationGenomes[0] = firstChild;
        PopulationGenomes[1] = secondFittest;

        //Crossover rest of the population using Roulette Select.
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
    /// <param name="fitnessWeights">Genome fitness values array</param>
    /// <returns>Index of selected gene</returns>
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
            if (value <= 0)
            {
                int returnValue = i;
                return returnValue;
            }
        }

        return fitnessWeights.Length - 1;
    }

    /// <summary>
    /// Performs an uniform crossover with 50% chance of getting the genes from either of the parents
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

    /// <summary>
    /// Mutates the population based on the mutation probability configuration
    /// </summary>
    private void Mutate()
    {
        for (var i = 1; i < PopulationGenomes.Count; ++i)
        {
            var random = UnityEngine.Random.Range(0F, 1F);
            if (random < MutationProbality)
            {
                PopulationGenomes[i] = MutateGenes(PopulationGenomes[i]);
            }
        }
    }

    /// <summary>
    /// Mutates a single gene
    /// </summary>
    /// <param name="genes">Gene to be mutated</param>
    /// <returns></returns>
    private float[] MutateGenes(float[] genes)
    {
        var mutatedGenome = genes;
        var randomIndex = UnityEngine.Random.Range(0, GENE_LENGTH);
        if (randomIndex % 3 == 0)
        {
            mutatedGenome[randomIndex] = UnityEngine.Random.Range(0F, 10F);
            mutatedGenome[randomIndex+1] = UnityEngine.Random.Range(0F, 50F);
        }
        else
        {
            mutatedGenome[randomIndex] = UnityEngine.Random.Range(0F, 50F);
        }

        return mutatedGenome;
    }

    private void InitializeLogFiles()
    {
        GenerationDataFileName = "GenerationData.csv";
        FittestDataFileName = "Fittest.csv";
        FileStream generationDataFile = File.Create(Environment.CurrentDirectory + "\\" + GenerationDataFileName);
        FileStream fittestDataFile = File.Create(Environment.CurrentDirectory + "\\" + FittestDataFileName);
        GenerationDataStream = new StreamWriter(generationDataFile);
        FittestDataStream = new StreamWriter(fittestDataFile);
        GenerationDataStream.WriteLine("generation,fitness,distanceFromTarget");
        FittestDataStream.WriteLine("generation,fitness,distanceFromTarget");
    }

    private void LogGenerationData()
    {
        var monsters = GetComponentsInChildren<BlockMonster>();
        foreach (var monster in monsters)
        {
            GenerationDataStream.WriteLine($"{Generation},{monster.GetFitness()},{monster.GetDistanceFromTarget()}");
        }
    }

    private void LogFittestData()
    {
        var monster = GetFittestMonster();
        FittestDataStream.WriteLine($"{Generation},{monster.GetFitness()},{monster.GetDistanceFromTarget()}");
    }

    void OnDestroy()
    {
        GenerationDataStream.Close();
        FittestDataStream.Close();
    }

}
