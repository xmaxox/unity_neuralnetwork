using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace unab
{
    public class UIController : MonoBehaviour
    {
        public List<ComponentData> components;

        public TMP_Dropdown dropdown;

        public TextMeshProUGUI component_description;
        public TextMeshProUGUI component_datasheet;
        public Image component_image;

        // Start is called before the first frame update
        void Start()
        {
            component_datasheet.richText = true;

            if (dropdown != null)
                dropdown.options.Clear();

            foreach (var component in components)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData() { text = component.componentName });
            }
            
            dropdown.onValueChanged.AddListener(delegate { DropdownItemSelected(dropdown, components); });
            
            
            component_description.text = components[0].description;
            component_image.sprite = components[0].componentImage;
            component_datasheet.text = components[0].datasheetURL;
        }

        void DropdownItemSelected(TMP_Dropdown dropdown, List<ComponentData> componentList)
        {
            int index = dropdown.value;
            component_description.text = componentList[index].description;
            component_image.sprite = componentList[index].componentImage;
            component_datasheet.text = componentList[index].datasheetURL;
        }

        public void Open()
        {
            if (component_datasheet.text != null && component_datasheet.text.Length > 0)
            {
                Application.OpenURL(component_datasheet.text);
            }
        }
    }
}