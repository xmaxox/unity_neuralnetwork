using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace unab
{
    [CreateAssetMenu(fileName = "Component", menuName = "New Component Data", order = 0)]
    public class ComponentData : ScriptableObject
    {
        [Header("Component Data")]
        public string componentName;
        public Sprite componentImage;
        public string description;
        public string datasheetURL;
    }
}