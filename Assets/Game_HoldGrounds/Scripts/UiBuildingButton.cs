using General.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game_HoldGrounds.Scripts
{
    public class UiBuildingButton : MonoBehaviour
    {
        [Header("====== PROP")]
        [Tooltip("Prop that will be used by this button. It will auto set.")]
        [SerializeField] [ReadOnly] private PropData propData;

        [Header("====== UI")]
        [SerializeField] private TextMeshProUGUI uiPropName;
        [SerializeField] private TextMeshProUGUI uiGoldCosts;
        [SerializeField] private Image uiPropImage;
        
        // =============================================================================================================
        /// <summary>
        /// Set prop data for this button.
        /// </summary>
        /// <param name="prop"></param>
        public void SetProp(PropData prop)
        {
            propData = prop;
            uiPropName.text = propData.propName;
            uiGoldCosts.text = propData.goldCost.ToString();
            uiPropImage.sprite = propData.picture;
        }
        // =============================================================================================================
        /// <summary>
        /// Called from UI when a user press the button to select this building.
        /// </summary>
        public void SelectBuilding()
        {
            GameManager.Instance.BuildingSelToBuild(propData);
        }
        // =============================================================================================================
    }
}