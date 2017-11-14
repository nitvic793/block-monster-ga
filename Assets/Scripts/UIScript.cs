// Author: Nitish Victor (nithishvictor@gmail.com)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{

    public Text GenerationText = null;
    public Text PopulationText = null;
    public Slider TimeSlider = null;
    public Text Time = null;
    public Text TimeScale = null;
    public MonsterPopulation MonsterController = null;

    // Use this for initialization
    void Start()
    {
        GenerationText.text = "Generation: " + MonsterController.Generation;
        PopulationText.text = "Population: " + MonsterController.PopulationCount;
        Time.text = "Time: " + MonsterController.TotalTime;
        TimeSlider.value = MonsterController.TimeScale;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        GenerationText.text = "Generation: " + MonsterController.Generation;
        PopulationText.text = "Population: " + MonsterController.PopulationCount;
        Time.text = "Time: " + MonsterController.TotalTime;
        TimeScale.text = "TimeScale: " + MonsterController.TimeScale;
        //if (MonsterController.GetFittestMonster() != null)
        //{
        //    //FitnessText.text = "Current Fittest: " + MonsterController.GetFittestMonster()?.GetFitness();
        //}

    }

    public void SetSliderValue()
    {
        MonsterController.TimeScale = TimeSlider.value;
    }
}
