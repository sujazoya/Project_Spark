using UnityEngine;

namespace ProjectSpark.Gameplay
{
    public class SparkComponent : SparkElectronicObject
    {
        [SerializeField] private string componentFamily = "Generic";
        public string ComponentFamily => componentFamily;
    }
}
