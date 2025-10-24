using UnityEngine;

public class PlayerPaint : GetPaintColor
{
    public Color selectedPaint;
    public int colorKey = 0;

    void Start()
    {
        selectedPaint = PaintManager.instance.GetComponent<PaintColors>().colorDict[colorKey];
    }
    void Update()
    {
        
        CheckOnPaint();
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            colorKey++;
            if (colorKey > 2)
            {
                colorKey = 0;
            }
            selectedPaint = PaintManager.instance.GetComponent<PaintColors>().colorDict[colorKey];
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            colorKey--;
            if (colorKey < 0)
            {
                colorKey = 2;
            }
            selectedPaint = PaintManager.instance.GetComponent<PaintColors>().colorDict[colorKey];
        }

        
    }
}
