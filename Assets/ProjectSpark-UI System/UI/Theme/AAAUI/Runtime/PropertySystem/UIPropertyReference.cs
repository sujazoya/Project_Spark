using System;
using UnityEngine;

namespace AAAUI
{
    [Serializable]
    public sealed class UIPropertyReference
    {
        [SerializeField]
        private string property = string.Empty;

        [SerializeField]
        private UIPropertyType type = UIPropertyType.Float;

        public string Property => property;

        public UIPropertyType Type => type;

        public int PropertyId
        {
            get
            {
                if (string.IsNullOrEmpty(property))
                    return -1;

                return Shader.PropertyToID(property);
            }
        }

        public bool IsValid =>
            !string.IsNullOrEmpty(property);

        public UIPropertyReference()
        {
        }

        public UIPropertyReference(
            string propertyName,
            UIPropertyType propertyType)
        {
            property = propertyName;
            type = propertyType;
        }

        public void Set(
            string propertyName,
            UIPropertyType propertyType)
        {
            property = propertyName ?? string.Empty;
            type = propertyType;
        }
    }
}