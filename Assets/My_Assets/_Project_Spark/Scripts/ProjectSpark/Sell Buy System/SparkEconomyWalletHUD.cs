using TMPro;
using UnityEngine;

namespace ProjectSpark.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class SparkEconomyWalletHUD : MonoBehaviour
    {
        [SerializeField] private SparkEconomyService economy;
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private TMP_Text diamondsText;
        private SparkEconomyService subscribed;

        private void OnEnable() { Bind(); }
        private void Start() { Bind(); }
        private void OnDisable() { Unbind(); }

        private void Bind()
        {
            if (economy == null) economy = SparkEconomyService.Instance;
            if (economy != subscribed)
            {
                Unbind();
                subscribed = economy;
                if (subscribed != null) subscribed.Changed += Refresh;
            }
            Refresh();
        }

        private void Unbind()
        {
            if (subscribed != null) subscribed.Changed -= Refresh;
            subscribed = null;
        }

        public void Refresh()
        {
            if (coinsText != null) coinsText.text = economy != null ? economy.Coins.ToString("N0") : "0";
            if (diamondsText != null) diamondsText.text = economy != null ? economy.Diamonds.ToString("N0") : "0";
        }
    }
}
