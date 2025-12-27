using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class NPCMarkovBrain : MonoBehaviour
{
    [Serializable]
    public struct NPCParams {
        [Header("Behavioral Parameters")]
        [Range(0f, 1f)] public float planeAffinity;
        [Range(0f, 1f)] public float explorativity;
        [Header("Extra control")]
        public float maxTargetDistance;
        [Range(0f, 1f)] public float decisionVolatility;
        [Range(0f, 10f)] public float distributionSharpness;
        [Range(0f, 0.2f)] public float maxBias;
    }
    public NPCParams NPCParameters;
    public enum NPCState
    {
        Standing,
        Moving,
        SwitchingLayer,
        DestroyingPlane,
        CatchingPlane,
        ThrowingPlane
    }
    public NPCState initState;
    void SelectAction(NPCState state, NPCState previousState, bool randomExit) {
        switch (state) {
            case NPCState.Standing:
                //call standing action
                if (!randomExit)
                {
                    gameObject.GetComponent<Stand>().SetActive();
                }
                break;
            case NPCState.Moving:
                gameObject.GetComponent<Pusher>().active = true;
                break;
            case NPCState.SwitchingLayer:
                //call switching layer action
                if (!randomExit)
                {
                    gameObject.GetComponent<NPCSwitchLayer>().active = true;
                }
                break;
            case NPCState.ThrowingPlane:
                if (!randomExit)
                {
                    gameObject.GetComponent<NPCThrower>().SetActive(); //to give action scripts control over activation decision
                }
                break;
            case NPCState.CatchingPlane:
                //call catching plane action
                if (!randomExit)
                {
                    gameObject.GetComponent<CatchPlane>().SetActive();
                }
                break;
            case NPCState.DestroyingPlane:
                //call destroying plane action
                if (!randomExit)
                {
                    gameObject.GetComponent<DestroyPlane>().active = true;
                }
                break;
        }
    }
    bool NoState()
    {
        if (
            gameObject.GetComponent<Pusher>().active == false &&
            gameObject.GetComponent<Stand>().active == false &&
            gameObject.GetComponent<NPCSwitchLayer>().active == false &&
            gameObject.GetComponent<NPCThrower>().active == false &&
            gameObject.GetComponent<CatchPlane>().active == false &&
            gameObject.GetComponent<DestroyPlane>().active == false
           )
        {
            return true;
        }
        else
        {
            return false;
        }
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
            //string biasesStr = "Biases: ";
            //foreach (double element in biases)
            //{
            //    biasesStr += element.ToString() + ", ";
            //}
            //print(biasesStr);
            //create bias lists with alternating signs
            double normalizedPlaneDistance = Math.Min(planeDistance / NPCParameters.maxTargetDistance, 1);
            double normalizedTargetDistance = Math.Min(targetDistance / NPCParameters.maxTargetDistance, 1);
            double planeIX = normalizedPlaneDistance * (0.3 + biases[0])
                + normalizedTargetDistance * (0.2 + biases[1]) + planeAffinity * (0.5 + biases[2]) + biases[3];
            double motionIX = normalizedPlaneDistance * (0.35 + biases[0]) + normalizedTargetDistance * (0.2 + biases[1])
                + explorativity * (0.45 + biases[2]) + biases[3];

            int distAffirmSwitch = i > 0 ? 1 : -1;
            float switchablePlaneAffinity = (planeAffinity - 0.5f) * 2f;
            float switchableExplorativity = (explorativity - 0.5f) * 2f;
            stationaryDistribution[i] = Math.Pow(motionIX, -distAffirmSwitch * switchableExplorativity * NPCParameters.distributionSharpness); 
            stationaryDistribution[i + 3] = Math.Pow(planeIX, -distAffirmSwitch * switchablePlaneAffinity * NPCParameters.distributionSharpness); //power of 4 to accentuate differences
        }
        // Force normalize the distribution
        double sum = 0;
        foreach (double value in stationaryDistribution)
        {
            sum += value;
        }
        for (int i = 0; i < stationaryDistribution.Length; i++)
        {
            stationaryDistribution[i] /= sum;
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

    NPCState DistributionSelect(double[] dist) 
    {
        double topSelectionScore = -1;
        double topRandValue = 0;
        NPCState decision = 0;
        double randValue = (UnityEngine.Random.Range(0f, 10f) * dist.Max() * 1.5f) / 10f; //does this make it work better?
        for (int i = 0; i < dist.Length; i++)
        {
            //double randValue = (UnityEngine.Random.Range(0f, 10f) * probabilityRow.Max() * 1.5f) / 10f; // expanding random range to avoid float overload for smooth distribution
            double selectionScore = (dist[i] - randValue) / dist[i];
            if (selectionScore > topSelectionScore)
            {
                //update top selection score and decision
                topSelectionScore = selectionScore;
                topRandValue = randValue;
                decision = (NPCState)i;
            }
        }
        return decision;
    }
    
    IEnumerator NPCBehaviorRoutine()
    {
        NPCState currentState = initState;
        NPCState previousState = initState;
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
                NPCParameters.maxBias
            );
            //print($"StatDist first 2: {statDist[0]}, {statDist[1]}");
            double[,] transitionMatrix = GenerateTransitionMatrix(statDist);
            // From initial state, make choice and call appropriate state action class
            double[] probabilityRow = new double[6];
            for (int i = 0; i < statDist.Length; i++) {
                probabilityRow[i] = transitionMatrix[(int)currentState, i];
            }
            previousState = currentState;
            //FisherYatesShuffle(probabilityRow);


            NPCState decision = DistributionSelect(probabilityRow);
            currentState = decision;
            bool randomExit = (UnityEngine.Random.Range(0f, 10f) / 10) > 1 - NPCParameters.decisionVolatility;
            if (NoState() || randomExit)
            {
                print($"{gameObject.name} next action: {decision.ToString()}, random exit = {randomExit}");
                SelectAction(decision, previousState, randomExit);

                //test code
                //string statDistStr = $"{gameObject.name} Stationary Distribution: ";
                //foreach (double element in statDist)
                //{
                //    statDistStr += element.ToString() + ", ";
                //}

                //print(statDistStr);

                //string probRowStr = $"{gameObject.name} Probability Row: ";
                //foreach (double element in probabilityRow)
                //{
                //    probRowStr += element.ToString() + ", ";
                //}
                //probRowStr += "Top Selection Score: " + topSelectionScore + "rand: " + topRandValue;
                //print(probRowStr);
            }
            yield return new WaitForSeconds(0.2f); // approx human reaction time
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SelectAction(initState, (NPCState) (-1), false);
        StartCoroutine(NPCBehaviorRoutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
