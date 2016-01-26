﻿
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class GeneticAlgorithm<T> {

	// Genetic Algorithm delegates
	public delegate void GAInitGenome(out T values);
	public delegate void GACrossover(ref Genome<T> genome1, ref Genome<T> genome2, out Genome<T> child1, out Genome<T> child2);
	public delegate void GAMutation (ref Genome<T> genome1);

	public delegate float GAFitnessFunction(T values, int genomeIdx);

	// Genetic Algorithm attributes
	private int _populationSize;
	private int _generationSize;

	private float _mutationRate;
	private float _crossoverRate;
	private float _totalFitness;

	private bool _elitism;

	// Genetic Algorithm data structures
	private ArrayList _thisGeneration;
	private ArrayList _nextGeneration;
	
	static private GAFitnessFunction getFitness;

	static private GAInitGenome getInitGenome;
	static private GACrossover  getCrossover;
	static private GAMutation   getMutation;

	public GeneticAlgorithm() {

		_mutationRate = 0.05f;
		_crossoverRate = 0.80f;
		_populationSize = 100;
		_generationSize = 2000;
	}
	
	public GeneticAlgorithm(float crossoverRate, float mutationRate, int populationSize, int generationSize, bool elitism = false) {

		_mutationRate = mutationRate;
		_crossoverRate = crossoverRate;
		_populationSize = populationSize;
		_generationSize = generationSize;
		_elitism = elitism;
	}

	public GAFitnessFunction FitnessFunction {

		get  {
			return getFitness;
		}
		set {
			getFitness = value;
		}
	}

	public GACrossover Crossover {
		
		get  {
			return getCrossover;
		}
		set {
			getCrossover = value;
		}
	}

	public GAMutation Mutation {
		
		get  {
			return getMutation;
		}
		set {
			getMutation = value;
		}
	}

	public GAInitGenome InitGenome {
		
		get  {
			return getInitGenome;
		}
		set {
			getInitGenome = value;
		}
	}
	
	//  Properties
	public int PopulationSize {

		get {
			return _populationSize;
		}
		set {
			_populationSize = value;
		}
	}
	
	public int Generations {

		get {
			return _generationSize;
		}
		set {
			_generationSize = value;
		}
	}
	
	public float CrossoverRate {

		get {
			return _crossoverRate;
		}
		set {
			_crossoverRate = value;
		}
	}
	public float MutationRate {

		get {
			return _mutationRate;
		}
		set {
			_mutationRate = value;
		}
	}
	
	/// Keep previous generation's fittest individual in place of worst in current
	public bool Elitism {

		get {
			return _elitism;
		}
		set {
			_elitism = value;
		}
	}
	
	public void GetBest(out T values, out float fitness) {

		_thisGeneration.Sort(new GenomeComparer<T>());
		GetNthGenome(0, out values, out fitness);
	}

	public void GetWorst(out T values, out float fitness) {

		_thisGeneration.Sort(new GenomeComparer<T>());
		GetNthGenome(_populationSize - 1, out values, out fitness);
	}
	
	public void GetNthGenome(int n, out T values, out float fitness) {

		if (n < 0 || n > _populationSize - 1)
			throw new ArgumentOutOfRangeException("n too large, or too small");

		Genome<T> g = ((Genome<T>)_thisGeneration[n]);

		values = g.Genes;
		fitness = (float)g.Fitness;
	}

	// Method which starts the GA executing.
	public void StartEvolution() {

		if (getFitness == null)
			throw new ArgumentNullException("Need to supply fitness function");

		if (getInitGenome == null)
			throw new ArgumentNullException("Need to supply initialization function");

		if (getCrossover == null)
			throw new ArgumentNullException("Need to supply crossover function");

		if (getMutation == null)
			throw new ArgumentNullException("Need to supply mutation function");

		// Create current and next generations
		_thisGeneration = new ArrayList(_generationSize);
		_nextGeneration = new ArrayList(_generationSize);

		Genome<T>.MutationRate = _mutationRate;

		InitializePopulation();
		//RankPopulation();
		
		//for (int i = 0; i < _generationSize; i++) {

		//	CreateNextGeneration();
		//	RankPopulation();
		//}
	}

	private Genome<T> TournamentSelection(int size = 2) {

		Genome<T> []tournamentPopulation = new Genome<T>[size];

		for(int i = 0; i < size; i++)
			tournamentPopulation[i] = (Genome<T>)_thisGeneration[UnityEngine.Random.Range(0, _thisGeneration.Count)];

		Array.Sort(tournamentPopulation, new GenomeComparer<T>());
	
		return tournamentPopulation[0];
	}
	
	public void CreateNextGeneration()
	{
		_nextGeneration.Clear();

		for (int i = 0; i < _populationSize; i += 2) {
			
			Genome<T> parent1, parent2, child1, child2;
			
			parent1 = ((Genome<T>) TournamentSelection());
			parent2 = ((Genome<T>) TournamentSelection());
				
			Crossover(ref parent1, ref parent2, out child1, out child2);
			
			Mutation(ref child1);
			Mutation(ref child2);
			
			_nextGeneration.Add(child1);
			_nextGeneration.Add(child2);
		}
		
		if (_elitism)
			_nextGeneration[UnityEngine.Random.Range(0, _nextGeneration.Count)] = (Genome<T>)_thisGeneration[0];
		
		_thisGeneration.Clear();
		
		for (int i = 0; i < _populationSize; i++)
			_thisGeneration.Add(_nextGeneration[i]);
	}

	// Rank population and sort in order of fitness.
	public void RankPopulation() {

		_totalFitness = 0f;

		for (int i = 0; i < _populationSize; i++) {
			
			Genome<T> g = ((Genome<T>) _thisGeneration[i]);
			g.Fitness = FitnessFunction(g.Genes, i);
			_totalFitness += g.Fitness;
		}
		
		_thisGeneration.Sort(new GenomeComparer<T>());
	}
	
	// Create the initial genomes by repeated calling the supplied fitness function
	private void InitializePopulation() {
		
		for (int i = 0; i < _populationSize ; i++) {
			
			Genome<T> g = new Genome<T>();

			T genes = g.Genes;
			InitGenome(out genes);
			g.Genes = genes;

			_thisGeneration.Add(g);
		}
	}
}