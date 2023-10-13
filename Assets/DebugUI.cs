using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugUI : MonoBehaviour
{
    public PlayerAdvanced playerAdvanced;
    // Text
    private TextMeshProUGUI debugUiText;

    // Start is called before the first frame update
    void Start()
    {
        debugUiText = GetComponent<TextMeshProUGUI> ();
        
    }

    // Update is called once per frame
    void Update()
    {
        debugUiText.text = "Current Slope: " + playerAdvanced.currentSlopeText;
    }
}
