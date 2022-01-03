using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System.Text.RegularExpressions;

namespace unab
{
    public class LineFollowerController : MonoBehaviour
    {
        [Header("Type of handling")]
        public bool isManual = false;
        public bool isPID = false;
        public bool isNeuralNetwork = false;
        public bool isNeuralNetworkTraining = false;

        [Header("Neural Network Files")]
        public int[] layers;
        public int epochs = 0;
        public float learningRate;
        public TextAsset inputTrainingFile;
        public TextAsset outputTrainingFile;
        public TextAsset modelTrained;
        public float[] sensor_lecture;
        public float[] output;

        [Header("Wheels Settings")]
        public List<Transform> wheelModels;

        public List<WheelCollider> wheelColliders;
        public List<Transform> wheelSteering;
        public List<WheelCollider> wheelSteeringCollider;

        [Header("Motor and Steering Settings")]
        public Transform centerOfMass;

        public float torque = 0f;
        public float brakeStrength = 0f;
        public float maxSteering = 0f;

        [Header("Sensor array")]
        public List<SensorColorController> sensorColorControllers;

        [Header("PID Settings")]
        public float pid_value = 0;
        public float k_p = 0f;
        public float k_i = 0f;
        public float k_d = 0f;
        public float set_point = 0f;
        public float current_error = 0f;
        public float last_error = 0f;
        public float correction = 0f;

        private float P = 0f;
        private float I = 0f;
        private float D = 0f;

        [Header("HUD Settings")]
        public TextMeshProUGUI lbl_timeValue;
        public TextMeshProUGUI lbl_speedValue;
        public TextMeshProUGUI lbl_errorMessage;
        public TextMeshProUGUI lbl_outputAccel;
        public TextMeshProUGUI lbl_outputAngle;

        private float m_forwardSpeed = 0f;
        private float m_steeringAngle = 0f;
        private Vector3 m_position;
        private Rigidbody rb;

        private NeuralNetwork m_NeuralNetwork;
        [SerializeField]
        private float[][] m_inputs;
        private float[][] m_outputs;
        private float m_currentTime = 0.0f;
        private float m_speedValue = 0.0f;
        private bool m_start = false;

        // Start is called before the first frame update
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.centerOfMass = centerOfMass.localPosition;

            m_position = transform.position;            
            isManual = false;
            isPID = false;
            isNeuralNetwork = false;



            if (inputTrainingFile != null)
            {
                m_NeuralNetwork = new NeuralNetwork(layers, learningRate);
                Debug.Log("Network Created...\n");
                m_inputs = ReadCSV(inputTrainingFile);
                m_outputs = ReadCSV(outputTrainingFile);
                // Debug.Log($"ROWS: {m_inputs.Length}, COLUMNS: {m_inputs[0].Length}\n");
                sensor_lecture = new float[layers[0]];
                output = new float[layers[layers.Length - 1]];
            }
            else
            {
                Debug.Log("Asset NULL\n");
            }            
        }

        // Update is called once per frame
        private void Update()
        {
            if (m_start && isManual)
            {
                m_forwardSpeed = Input.GetAxis("Vertical");
                m_steeringAngle = Input.GetAxis("Horizontal");
            }
        }

        private void FixedUpdate()
        {
            if (m_start)
            {
                m_currentTime = Time.time;
                m_speedValue = rb.velocity.magnitude;

                if (isPID)
                {
                    pid_value = PID();
                    pid_value = Mathf.Clamp(pid_value, -torque, torque);
                    Accelerate(torque + pid_value);

                    Steering(GetAngle());
                }
                else if (isManual)
                {
                    Accelerate(m_forwardSpeed);
                    Steering(m_steeringAngle);
                }
                else if (isNeuralNetwork)
                {
                    if (isNeuralNetworkTraining)
                    {
                        modelTrained = m_NeuralNetwork.Training(m_inputs, m_outputs, epochs, $"nn_model_{layers[0]}_{layers[1]}_{layers[2]}");
                        isNeuralNetworkTraining = false;
                        m_start = false;
                        isNeuralNetwork = false;
                        transform.position = m_position;
                    }
                    else
                    {
                        //if (m_NeuralNetwork == null)
                        //{
                        //    if (!modelTrained)
                        //    {                                 
                        //        m_NeuralNetwork = new NeuralNetwork(modelTrained);
                        //    }
                        //    else
                        //    {
                        //        m_start = false;
                        //        isNeuralNetwork = false;                                
                        //    }
                        //}                        

                        for (int i = 0; i < sensorColorControllers.Count; i++)
                        {
                            sensor_lecture[i] = sensorColorControllers[i].isLineDetected ? 0.0f : 1.0f;
                        }

                        output = m_NeuralNetwork.FeedForward(sensor_lecture, m_NeuralNetwork.Sigmoid);

                        lbl_outputAccel.text = output[0].ToString();
                        lbl_outputAngle.text = output[1].ToString();

                        Accelerate(output[0]);
                        Steering(output[1] * GetAngle());
                    }
                }

                WheelAnimation();
            }
            else
            {
                m_currentTime = 0f;
                m_speedValue = 0f;
                m_forwardSpeed = 0f;
                m_steeringAngle = 0f;
                transform.position = m_position;
            }

            lbl_timeValue.text = m_currentTime.ToString();
            lbl_speedValue.text = m_speedValue.ToString();
        }

        /// <summary>
        /// Gives the line follower robot speed with a given direction
        /// </summary>
        /// <param name="acceleration">dictates if the robot move backwards or forward (-1, 1)</param>
        public void Accelerate(float acceleration)
        {
            if (acceleration == 0f)
            {
                for (int i = 0; i < wheelColliders.Count; i++)
                {
                    wheelColliders[i].motorTorque = 0f;
                    wheelColliders[i].brakeTorque = brakeStrength;
                }
            }
            else
            {
                for (int i = 0; i < wheelColliders.Count; i++)
                {
                    wheelColliders[i].motorTorque = torque * acceleration;
                    wheelColliders[i].brakeTorque = 0f;
                }
            }
        }

        public void AccelerateLeft(float acceleration)
        {
            wheelColliders[0].motorTorque = acceleration;
            wheelColliders[2].motorTorque = acceleration;
        }

        public void AccelerateRight(float acceleration)
        {
            wheelColliders[1].motorTorque = acceleration;
            wheelColliders[3].motorTorque = acceleration;
        }

        /// <summary>
        /// Give the angle to turn left or right
        /// </summary>
        /// <param name="angle">degrees of rotation (-maxSteering, maxSteering)</param>
        public void Steering(float angle)
        {
            for (int i = 0; i < wheelSteering.Count; i++)
            {
                if (isPID || isNeuralNetwork)
                {
                    wheelSteeringCollider[i].steerAngle = angle;
                }
                else
                {
                    wheelSteeringCollider[i].steerAngle = angle * maxSteering;
                }
            }
        }

        /// <summary>
        /// Handles visualizations on the model
        /// </summary>
        public void WheelAnimation()
        {
            Vector3[] _pos = new Vector3[4];
            Quaternion[] _rot = new Quaternion[4];

            for (int i = 0; i < _pos.Length; i++)
            {
                wheelColliders[i].GetWorldPose(out _pos[i], out _rot[i]);
            }

            for (int i = 0; i < _pos.Length; i++)
            {
                wheelModels[i].position = _pos[i];
                wheelModels[i].rotation = _rot[i];
                wheelModels[i].localEulerAngles = new Vector3(wheelModels[i].localEulerAngles.x, wheelModels[i].localEulerAngles.y, -90f);
            }
        }

        /// <summary>
        /// Calculate angle between center of mass and sensor on line detection
        /// </summary>
        /// <returns>the computed angle from center of mass position and sensor position in degrees</returns>
        public float GetAngle()
        {
            Vector3 sensorOnLine = new Vector3();

            foreach (var item in sensorColorControllers)
            {
                if (item.isLineDetected)
                {
                    sensorOnLine = item.transform.localPosition;
                    break;
                }
            }

            var distance_z = sensorOnLine.z - centerOfMass.localPosition.z;
            var distance_x = sensorOnLine.x - centerOfMass.localPosition.x;

            var angle = Mathf.Atan2(distance_x, distance_z) * 180f / Mathf.PI;

            return angle;
        }

        public float PID()
        {
            float position = 0f;
            for (int i = 0; i < sensorColorControllers.Count; i++)
            {
                if (sensorColorControllers[i].isLineDetected)
                {
                    position = sensorColorControllers[i].sensorValue;
                    break;
                }
            }

            current_error = set_point - position;

            P = current_error;
            I += current_error;
            D = (current_error - last_error);

            correction = k_p * P + k_i * I + k_d * D;
            last_error = current_error;

            return correction;
        }

        public void PauseGame()
        {
            if (!isManual && !isPID && !isNeuralNetwork)
            {
                lbl_errorMessage.gameObject.SetActive(true);
            }
            else
            {
                m_start = !m_start;
            }
        }

        public void ActivateManual()
        {
            if (lbl_errorMessage.gameObject.activeInHierarchy)
                lbl_errorMessage.gameObject.SetActive(false);

            isManual = true;
            isPID = false;
            isNeuralNetwork = false;
        }

        public void ActivatePID()
        {
            if (lbl_errorMessage.gameObject.activeInHierarchy)
                lbl_errorMessage.gameObject.SetActive(false);

            isManual = false;
            isPID = true;
            isNeuralNetwork = false;
        }

        public void ActivateNeuralNetwork()
        {
            if (lbl_errorMessage.gameObject.activeInHierarchy)
                lbl_errorMessage.gameObject.SetActive(false);

            isManual = false;
            isPID = false;
            isNeuralNetwork = true;

            if (!modelTrained)
                isNeuralNetworkTraining = true;
        }

        public void TrainNetwork()
        {

        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("EndRoad"))
            {
                isManual = false;
                isPID = false;
                isNeuralNetwork = false;
                m_start = false;
            }
        }

        public float[][] ReadCSV(TextAsset asset)
        {
            if (asset == null)
            {
                Debug.Log("ERROR READING CSV\n");
                return null;
            }

            float[][] values;

            string fs = asset.text;

            string[] reader = Regex.Split(fs, "\n|\r|\r\n", RegexOptions.Multiline);
            string[] splitted = new string[reader.Length / 2];

            // Debug.Log($"reader length: {reader.Length}\n");
            List<string> lines = new List<string>();
            foreach (var item in reader)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    lines.Add(item);
                }
            }
            splitted = lines.ToArray();

            values = new float[splitted.Length][];

            for (int i = 0; i < splitted.Length; i++)
            {
                if (splitted[i] == "" || splitted[i] == null) break;

                string[] row = Regex.Split(splitted[i], ";|,");
                // Debug.Log($"IN PASS: {i}, ROW LENGTH: {row.Length}\n");

                values[i] = new float[row.Length];

                for (int j = 0; j < row.Length; j++)
                {
                    float.TryParse(row[j], out values[i][j]);
                }
            }

            return values;
        }        
    }
}