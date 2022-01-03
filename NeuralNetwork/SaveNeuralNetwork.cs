using System.IO;
using UnityEditor;
using UnityEngine;

namespace unab
{
    [System.Serializable]
    public class SaveNeuralNetwork
    {        
        public int[] layers;        
        public float[][][] weights;
        public float[][] biases;
        public float LearningRate;
    }
}