using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectSpark.Level125
{
    [DisallowMultipleComponent]
    public sealed class SparkL125PlacementSlot : MonoBehaviour
    {
        [SerializeField] private string componentRef;
        [SerializeField] private string assetId;
        [SerializeField] private string componentKind;
        [SerializeField] private bool physicalFootprintVerified;
        [SerializeField] private bool installed;
        public string ComponentRef => componentRef;
        public string AssetId => assetId;
        public bool IsInstalled => installed;
        public bool IsPlacementCertifiable => physicalFootprintVerified;
        public void Configure(string referenceId, string manifestAssetId, string kind)
        {
            componentRef = referenceId;
            assetId = manifestAssetId;
            componentKind = kind;
            physicalFootprintVerified = false;
            installed = false;
        }
    }
}
