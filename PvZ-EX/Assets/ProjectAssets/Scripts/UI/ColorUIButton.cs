using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorUIButton : MonoBehaviour
{
    [Header("Button Colours")]
    public Color selectedColor = new();
    public Color resetColor = new();
    public void SelectedButtonColor(Button button)
    {
        ColorBlock colorBlock = button.colors;
        colorBlock.normalColor = selectedColor;
        colorBlock.selectedColor = selectedColor;
        colorBlock.highlightedColor = selectedColor;
        colorBlock.pressedColor = resetColor;
        button.colors = colorBlock;
    }
    public void ResetButtonColor(Button button)
    {
        ColorBlock colorBlock = button.colors;
        colorBlock.normalColor = resetColor;
        colorBlock.selectedColor = resetColor;
        colorBlock.highlightedColor = resetColor;
        colorBlock.pressedColor = selectedColor;
        button.colors = colorBlock;
    }
}
