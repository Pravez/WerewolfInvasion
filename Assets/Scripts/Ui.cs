using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ui : MonoBehaviour
{
    public Text Peasants;
    public Text Werewolves;
    public Text Armed;

    public Text Forges;
    public Text PlacedForges;
    public Text Resources;

    public void SetPeasants(int number)
    {
        Peasants.text = "Peasants : " + number;
    }

    public void SetWerewolves(int number)
    {
        Werewolves.text = "Werewolves : " + number;
    }

    public void SetArmed(int number)
    {
        Armed.text = "Armed : " + number;
    }

    public void SetForgeText(string text)
    {
        Forges.text = text;
    }

    public void SetPlacedForges(int placed)
    {
        PlacedForges.text = "Placed Forges : " + placed;
    }

    public void ChangeForgeTextColor(Color color)
    {
        Forges.color = color;
    }

    public void SetResources(int value)
    {
        Resources.text = "Resources : " + value;
    }
}
