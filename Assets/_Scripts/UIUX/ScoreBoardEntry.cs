using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreBoardEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text nameTxt, killsTxt, deathsTxt;

    public void SetData(string name, int kills, int deaths)
    {
        nameTxt.text = name;
        killsTxt.text = kills.ToString();
        deathsTxt.text = deaths.ToString();
    }

}
