using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class NPCMarkovBrain : MonoBehaviour
{
    enum NPCState { 
        Standing,
        Moving,
        SwitchingLayer,
        ThrowingPlane,
        CatchingPlane,
        DestroyingPlane
    }
    /// <summary>
    /// TODO: AI-generated function, validate that everything works as intended
    /// 
    /// 
    /// Generates a transition matrix for a Markov chain with the given stationary distribution.
    /// Uses a simple approach that creates a reversible Markov chain by ensuring detailed balance.
    /// </summary>
    /// <param name="stationaryDistribution">Array representing the stationary distribution (must sum to 1)</param>
    /// <returns>2D array representing the transition matrix</returns>
    /// <exception cref="ArgumentException">Thrown when input is invalid</exception>
    public static double[,] GenerateTransitionMatrix(double[] stationaryDistribution)
    {
        // Input validation
        if (stationaryDistribution == null)
        {
            throw new ArgumentNullException(nameof(stationaryDistribution));
        }

        if (stationaryDistribution.Length == 0)
        {
            throw new ArgumentException("Stationary distribution cannot be empty", nameof(stationaryDistribution));
        }

        // Validate that all values are non-negative
        foreach (double value in stationaryDistribution)
        {
            if (value < 0)
                throw new ArgumentException("All values in stationary distribution must be non-negative", nameof(stationaryDistribution));
        }

        // Validate that sum is approximately 1
        double sum = 0;
        foreach (double value in stationaryDistribution) { 
            sum += value;
        }
        if (Math.Abs(sum - 1.0) > 1e-10)
        {
            throw new ArgumentException("Stationary distribution must sum to 1", nameof(stationaryDistribution));
        }

        int n = stationaryDistribution.Length;
        double[,] transitionMatrix = new double[n, n];

        // Handle single state case
        if (n == 1)
        {
            transitionMatrix[0, 0] = 1.0;
            return transitionMatrix;
        }

        // Generate transition matrix using detailed balance condition
        // P[i,j] * pi[i] = P[j,i] * pi[j]
        // We'll use a simple approach: for each pair (i,j), set transition probabilities
        // proportional to the target stationary distribution

        System.Random random = new System.Random();

        for (int i = 0; i < n; i++)
        {
            double remainingProbability = 1.0;

            // For each state i, distribute probability to other states
            for (int j = 0; j < n; j++)
            {
                if (i == j)
                {
                    continue;
                }

                // Calculate maximum probability we can assign to transition i->j
                // while ensuring we can still satisfy detailed balance
                double maxProb = remainingProbability;

                // Use a fraction of the remaining probability, weighted by stationary distribution
                // This ensures states with higher stationary probability receive more transitions
                double weight = stationaryDistribution[j];
                double totalWeight = 0;

                // Calculate total weight for remaining states
                for (int k = j; k < n; k++)
                {
                    if (k != i)
                        totalWeight += stationaryDistribution[k];
                }

                if (totalWeight > 0)
                {
                    // Assign probability proportional to weight, but leave some room for diagonal
                    double prob = (weight / totalWeight) * remainingProbability * 0.9; // 0.9 factor leaves room for self-loop
                    transitionMatrix[i, j] = prob;
                    remainingProbability -= prob;
                }
            }

            // Assign remaining probability to self-loop
            transitionMatrix[i, i] = Math.Max(0, remainingProbability);
        }

        // Normalization: Ensure rows sum to exactly 1 (handle floating point errors)
        for (int i = 0; i < n; i++)
        {
            double rowSum = 0;
            for (int j = 0; j < n; j++)
            {
                rowSum += transitionMatrix[i, j];
            }

            if (Math.Abs(rowSum - 1.0) > 1e-10)
            {
                // Normalize the row
                for (int j = 0; j < n; j++)
                {
                    transitionMatrix[i, j] /= rowSum;
                }
            }
        }

        return transitionMatrix;
    }
    //then make the probability transition matrix
    float[,] transitionMatrix = new float[6, 6];
    void MapState(NPCState state, float[] mapping) {
        int i = (int) state;
        for (int j = 0; j < mapping.Length; j++)
        {
            transitionMatrix[i, j] = mapping[j];
        }
    }
    //Now handle stationary distribution. This is the transition matrix/left eigenvalue
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
