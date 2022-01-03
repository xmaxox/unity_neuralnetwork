using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace unab
{
    /// <summary>
    /// Artificial Neural Network - Unsupervised learning
    /// </summary>
    public class NeuralNetwork
    {
        public float LearningRate { get; set; }

        public int[] _layers;                // Numbers of layer in the neural network
        public float[][] _neurons;           // Numbers of neurons per layer [layer][neurons]
        public float[][][] _weights;         // Weights values for each connection from previous neurons to current layer neurons [layer][neuron_in_next_layer_index][neuron_in_previous_layer_index]
        public float[][] _biases;            // Biases values per layer with values per neuron [layer][neuron_in_next_layer_index]

        /// <summary>
        /// Initialize neural network with random weights
        /// </summary>
        /// <param name="layers">Layers for the neural network</param>
        public NeuralNetwork(int[] layers, float learningRate)
        {
            // Deep Copy of layers
            _layers = new int[layers.Length];

            for (int i = 0; i < layers.Length; i++)
            {
                _layers[i] = layers[i];
            }

            LearningRate = learningRate;

            // Generate matrix
            InitNeurons();
            Debug.Log($"Neuron initialized\n");
            InitWeights();
            Debug.Log($"Weights initialization Randomized, layer last: {_weights[1][0].Length}\n");
            InitBiases();
            Debug.Log($"Biases initialization Randomized\n");
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="other">Neural network to copy from</param>
        public NeuralNetwork(NeuralNetwork other)
        {
            _layers = new int[other._layers.Length];

            for (int i = 0; i < other._layers.Length; i++)
            {
                _layers[i] = other._layers[i];
            }

            InitNeurons();
            Debug.Log($"Neurons initialized\n");
            InitWeights(other._weights);
            Debug.Log($"Weights initialized\n");
            InitBiases(other._biases);
            Debug.Log($"Biases initialized\n");
        }

        public NeuralNetwork(TextAsset other)
        {
            LoadModel(other);
            InitNeurons();
        }

        /// <summary>
        /// Start training the neural network with given inputs
        /// </summary>
        /// <param name="inputs">Training input data set</param>
        /// <param name="outputs">Training output data set</param>
        /// <param name="epochs">Number of repetition for learning</param>
        /// <returns>Saved file</returns>
        public TextAsset Training(float[][] inputs, float[][] outputs, int epochs, string trainingName)
        {
            float[] ffOutputs;

            for (int i = 0; i < epochs; i++)
            {
                for (int j = 0; j < inputs.GetLength(0); j++)
                {
                    ffOutputs = FeedForward(inputs[j], Sigmoid);
                    //BackPropagation(ffOutputs, DSigmoid);
                }                
            }
            
            Debug.Log($"Finished training, epochs: {epochs}\n");

            var model = new SaveNeuralNetwork();
            model.layers = _layers;
            model.weights = _weights;
            model.biases = _biases;  
            model.LearningRate = LearningRate;

            TextAsset ptrFile = SaveModel(model, trainingName);
            
            return ptrFile;
        }

        /// <summary>
        /// Initialize neuron matrix
        /// </summary>
        private void InitNeurons()
        {
            _neurons = new float[_layers.Length][];

            for (int i = 0; i < _layers.Length; i++)
            {
                _neurons[i] = new float[_layers[i]];
            }            
        }

        /// <summary>
        /// Initialize weight matrix with random numbers between (-0.5, 0.5)
        /// Example for a neural network with layers[] = {7, 8, 2}
        /// weights[0][8][7]; weights[1][7][2]
        /// </summary>
        private void InitWeights()
        {
            _weights = new float[_layers.Length - 1][][];

            for (int i = 0;i < _layers.Length - 1; i++)
            {
                _weights[i] = new float[_layers[i + 1]][];

                for (int j = 0; j < _layers[i + 1]; j++)
                {
                    _weights[i][j] = new float[_layers[i]];

                    for (int k = 0; k < _layers[i]; k++)
                    {
                        _weights[i][j][k] = Random.Range(-.5f, .5f);
                    }
                }
            }
        }

        /// <summary>
        /// Initialize weights with given values
        /// </summary>
        /// <param name="other">Neural network array to copy weights from</param>
        private void InitWeights(float[][][] other)
        {
            _weights = new float[other.Length][][];

            for (int i = 0; i < other.Length; i++)
            {
                _weights[i] = new float[other[i].Length][];

                for (int j = 0; j < other[i].Length; j++)
                {
                    _weights[i][j] = new float[other[i][j].Length];
                    for (int k = 0; k < other[i][j].Length; k++)
                    {
                        _weights[i][j][k] = other[i][j][k];
                    }
                }
            }
        }

        /// <summary>
        /// Initialize biases matrix with random numbers between (-0.5, 0.5)
        /// Example for a neural network with layers[] = {7, 8, 2}
        /// biases[0][8]; biases[1][2]
        /// </summary>
        private void InitBiases()
        {
            _biases = new float[_layers.Length - 1][];

            for (int i = 0; i < _layers.Length - 1; i++)
            {
                _biases[i] = new float[_layers[i + 1]];

                for (int j = 0; j < _layers[i + 1]; j++)
                {
                    _biases[i][j] = Random.Range(-.5f, .5f);
                }
            }
        }

        /// <summary>
        /// Initialize bias with given values
        /// </summary>
        /// <param name="other">Neural network array to copy biases from</param>
        private void InitBiases(float[][] other)
        {
            _biases = new float[other.Length][];

            for (int i = 0; i < other.Length; i++)
            {
                _biases[i] = new float[other[i].Length];

                for (int j = 0; j < other[i].Length; j++)
                {
                    _biases[i][j] = other[i][j];
                }
            }
        }

        /// <summary>
        /// Feedforward the neural network with given input array
        /// </summary>
        /// <param name="inputs">Inputs to the neural network</param>
        /// <returns>Return the output layer</returns>
        public float[] FeedForward(float[] inputs, System.Func<float, float> activation)
        {
            // Add inputs to input layer
            for (int i = 0; i < inputs.Length; i++)
            {
                _neurons[0][i] = inputs[i];
            }

            // Go through all layers next from input layer
            for (int i = 1; i < _layers.Length; i++)
            {
                 float value = 0.0f;

                // Iterates over neurons on layer i
                for (int j = 0; j < _layers[i]; j++)
                {
                    value += _biases[i - 1][j];

                    // Iterates over neurons on layer i - 1
                    for (int k = 0; k < _layers[i - 1]; k++)
                    {
                        value += (_weights[i - 1][j][k] * _neurons[i][j]);
                    }

                    _neurons[i][j] = activation(value);
                }
            }

            // return output layer
            return _neurons[_layers.Length - 1];
        }

        public void BackPropagation(float[] inputs, System.Func<float, float> activation)
        {
            float[] errors = new float[_layers[_layers.Length - 1]];

            for (int i = 0; i < inputs.Length; i++)
            {
                errors[i] = inputs[i] - _neurons[_layers.Length - 1][i];
            }

            float[][] gamma = new float[_layers.Length - 1][];

            // Calculate gamma errors
            for (int i = 0; i < _layers.Length - 1; i++)
            {
                gamma[i] = new float[_layers[_layers.Length - 2 - i]];

                for (int j = 0; j < _layers[_layers.Length - 1]; j++)
                {
                    float value = 0;

                    for (int k = 0; k < gamma[i].Length; k++)
                    { 
                        value += _weights[_layers.Length - 2 - i][j][k] * errors[j];
                    }

                    gamma[i][j] = value;
                }
            }

            // Correct weights connected to output layer
            for (int i = 0; i < _layers[_layers.Length - 1]; i++)
            {
                float value = 0.0f;

                // Run through every connection from previous layer
                for (int j = 0; j < _layers[_layers.Length - 2]; j++)
                {        
                    float input_to_neuron = 0.0f;
                    input_to_neuron += (_weights[_layers.Length - 2][i][j] * _neurons[_layers[_layers.Length - 1]][i]);
                    input_to_neuron += _biases[_layers.Length - 2][i];
                    
                    value += activation(input_to_neuron) + (errors[i] * -LearningRate);
                    value *= input_to_neuron;

                    _weights[_layers.Length - 2][i][j] += value;
                }
            }

            // Iterates over the rest of the layer through end to start
            for (int i = _layers.Length - 2; i < -1; i--)
            {
                // Iterates over neurons in current layer
                for (int j = 0; j < _layers[i]; j++)
                {                
                    float value = 0.0f;

                    // Iterates over neuron on previous layers to the current one
                    for (int k = 0; k < _layers[i - 1]; k++)
                    {
                        float input_to_neuron = 0.0f;

                        input_to_neuron += (_weights[i - 1][j][k] * _neurons[i][j]);
                        input_to_neuron += _biases[i - 1][j];

                        value += activation(input_to_neuron) * (gamma[i][j] * -LearningRate) * input_to_neuron;
                    }
                }
            }
        }

        public float Sigmoid(float value)
        {
            return 1.0f / (1.0f - Mathf.Exp(-value));
        }

        public float DSigmoid(float value)
        {
            return Sigmoid(value) * (1.0f - Sigmoid(value));
        }

        public float TanH(float value)
        {
            return (float)System.Math.Tanh(value);
        }

        public float DTanH(float value)
        {
            return (float)(1 - Mathf.Pow(TanH(value), 2));
        }

        public float[][] Transposed(float[][] matrix)
        {
            var rows = matrix.GetLength(1);
            var cols = matrix.GetLength(0);

            float[][] result = new float[rows][];

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                result[i] = new float[matrix[i].Length];
            }

            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    result[j][i] = matrix[i][j];
                }
            }

            return result;
        }
        #if UNITY_EDITOR
        public TextAsset SaveModel(SaveNeuralNetwork model, string fileName)
        {
            TextAsset textAsset = new TextAsset(JsonUtility.ToJson(model));
            
            var savePath = $"Assets/Resources/models/{fileName}.asset";

            AssetDatabase.CreateAsset(textAsset, savePath);
            UnityEditor.AssetDatabase.Refresh();

            return textAsset;
        }
        #endif

        public void LoadModel(TextAsset model)
        {
            SaveNeuralNetwork loadedModel = JsonUtility.FromJson<SaveNeuralNetwork>(model.text);
            _layers = loadedModel.layers;
            _weights = loadedModel.weights;
            _biases = loadedModel.biases;
            LearningRate = loadedModel.LearningRate;
        }
    }
}