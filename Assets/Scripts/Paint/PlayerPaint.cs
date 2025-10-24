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
        selectedPaint = PaintManager.instance.GetComponent<PaintColors>().colorDict[colorKey];
        CheckOnPaint();
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            colorKey++;
            if (colorKey > 2)
            {
                colorKey = 0;
            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            colorKey--;
            if (colorKey < 0)
            {
                colorKey = 2;
            }
        }

        // if (Input.GetKeyDown(KeyCode.Alpha1))
        // {
        //     selectedPaint = PaintManager.instance.GetComponent<PaintColors>().damagePaint;
        // }
        // if (Input.GetKeyDown(KeyCode.Alpha2))
        // {
        //     selectedPaint = PaintManager.instance.GetComponent<PaintColors>().jumpPaint;
        // }
        // if (Input.GetKeyDown(KeyCode.Alpha3))
        // {
        //     selectedPaint = PaintManager.instance.GetComponent<PaintColors>().movementPaint;
        // }
    }
}
