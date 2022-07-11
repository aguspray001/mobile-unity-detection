using UnityEngine;


namespace Message
{
    [SerializeField]
    public class PredictResponse
    {
        public string error;
        public string[] classes;
        public float[] confidences;
        public float[,] box;
    }
}