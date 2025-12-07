using UnityEngine;

public class PlayerPaint : GetPaintColor
{
    public Color selectedPaint;
    public int colorKey = 0;
    public Material playerModel;

    void Start()
    {
        selectedPaint = PaintManager.instance.GetComponent<PaintColors>().colorDict[colorKey];
        if(playerModel != null)
            playerModel.color = selectedPaint;
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
            
            if(playerModel != null)
                playerModel.color = selectedPaint;
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            colorKey--;
            if (colorKey < 0)
            {
                colorKey = 2;
            }
            selectedPaint = PaintManager.instance.GetComponent<PaintColors>().colorDict[colorKey];

            if(playerModel != null)
                playerModel.color = selectedPaint;
        }

        
    }
}
