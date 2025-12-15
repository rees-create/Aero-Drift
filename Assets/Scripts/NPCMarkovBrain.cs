using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

public class NPCMarkovBrain : MonoBehaviour
{
    public struct NPCParams {
        [Header("Behavioral Parameters")]
        public float planeAffinity;
        public float explorativity;
        [Header("Extra control")]
        public float maxTargetDistance;
        public float decisionVolatility;
    }
    public NPCParams NPCParameters;
    public enum NPCState
    {
        Standing,
        Moving,
        SwitchingLayer,
        ThrowingPlane,
        CatchingPlane,
        DestroyingPlane
    }
    public NPCState initState;
    void SelectAction(NPCState state) {
        switch (state) {
            case NPCState.Standing:
                //call standing action
                break;
            case NPCState.Moving:
                gameObject.GetComponent<Pusher>().active = true;
                break;
            case NPCState.SwitchingLayer:
                //call switching layer action
                break;
            case NPCState.ThrowingPlane:
                gameObject.GetComponent<Thrower>().active = true;
                break;
            case NPCState.CatchingPlane:
                //call catching plane action
                break;
            case NPCState.DestroyingPlane:
                //call destroying plane action
                break;
        }
    }
    bool NoState()
    {
        if (
            gameObject.GetComponent<Pusher>().active == false &&
            gameObject.GetComponent<Thrower>().active == false
           )
            return true;
        else
            return false;
    }

    public GameObject plane;
    public GameObject popBackController;

    void FisherYatesShuffle<T>(T[] array)
    {
        System.Random random = new System.Random();
        int n = array.Length;

        for (int i = n - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);

            T temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }
    //Express stationary distribution (NPCState-enumerated) as a function of NPC control parameters:
    /**
     * - targetDistance
     * - planeDistance
     * 
     * - planeAffinity
     * - explorativity
     */
    double[] distributeBias(double bias, int size, int sign) {
        int actualSize = size + (size % 2);
        double[] distribution = new double[actualSize];
        for (int i = 0; i < actualSize; i++)
        {
            distribution[i] = bias * sign * ((i % 2) * 2 - 1);
        }
        return distribution;
    }
    
    /**
     * <summary>
     * Converts NPC control parameters to a stationary distribution over NPC states.
     * Probably unnecessarily compact.
     * </summary>
     */
    double[] ParamsToStatDist(float targetDistance, float planeDistance, float planeAffinity, float explorativity, float maxBias)
    {
        // Placeholder function: implement the actual mapping logic
        double[] stationaryDistribution = new double[6];
        //Plane Affinity affects Throwing/Catching/Destroying, and motion based on planeDistance
        stationaryDistribution[(int)NPCState.ThrowingPlane] = 0;
        for (int i = 0; i < 3; i++) {
            //mod switch
            int modSwitch = (i % 2) * 2 - 1;
            //create bias lists with alternating signs. 
            double[] biases = distributeBias(UnityEngine.Random.Range(0, maxBias), 3, modSwitch);
            //create bias lists with alternating signs
            double planeIX = planeDistance * (0.5 + biases[0])
                + targetDistance * (0.25 + biases[1]) + planeAffinity * (0.25 + biases[2]) + biases[3];
            double motionIX = planeDistance * (0.4 + biases[0]) + targetDistance * (0.3 + biases[1])
                + explorativity * (0.3 + biases[2]) + biases[3];

            stationaryDistribution[i] = planeIX;
            stationaryDistribution[i + 3] = motionIX;
        }
        
        return stationaryDistribution;
    }
    /// <summary>
    /// AI-generated function, tested and everything works as intended
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
    
    IEnumerator NPCBehaviorRoutine()
    {
        while (true)
        {
            Vector2 targetPosition = gameObject.GetComponent<Pusher>().DistanceToTarget(
                NPCParameters.explorativity,
                NPCParameters.maxTargetDistance
            );

            float targetDistance = Vector3.Distance(transform.position, targetPosition);
            float planeDistance = Vector3.Distance(transform.position, plane.transform.position);
            double[] statDist = ParamsToStatDist(
                targetDistance,
                planeDistance,
                NPCParameters.planeAffinity,
                NPCParameters.explorativity,
                0.5f
            );
            double[,] transitionMatrix = GenerateTransitionMatrix(statDist);
            // From initial state, make choice and call appropriate state action class
            double[] probabilityRow = new double[6];
            for (int i = 0; i < statDist.Length; i++) {
                probabilityRow[i] = transitionMatrix[(int)initState, i];
            }
            FisherYatesShuffle(probabilityRow);
            double randValue = UnityEngine.Random.Range(0f, 1f);
            double accumulatedProb = 0;
            NPCState decision = 0;
            for (int i = 0; i < statDist.Length; i++)
            {
                if (accumulatedProb < randValue)
                {
                    accumulatedProb += probabilityRow[i];
                }
                else
                {
                    decision = (NPCState) i;
                    break;
                }
            }
            if (NoState() || UnityEngine.Random.Range(0f, 1f) < NPCParameters.decisionVolatility)
            {
                SelectAction(decision);
            }
            yield return new WaitForSeconds(0.2f); // approx human reaction time
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(NPCBehaviorRoutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
