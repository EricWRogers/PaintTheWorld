using UnityEngine;

public class PlayerPaint : GetPaintColor
{
    public Color selectedPaint;
    public int colorKey = 0;
    public Material playerModel;
    public float waitTime = 0.2f;
    private float timer = 0f;

    void Start()
    {
        selectedPaint = PaintManager.instance.GetComponent<PaintColors>().colorDict[colorKey];
    }
    void Update()
    {
        timer += Time.deltaTime;
        if(timer > waitTime)
        {
            CheckOnPaint();
            timer = 0f;
        }
            
        if (PlayerManager.instance.playerInputs.Next.WasPressedThisFrame())
        {
            Debug.Log("next color");
            colorKey++;
            if (colorKey > 2)
            {
                colorKey = 0;
            }
            selectedPaint = PaintManager.instance.GetComponent<PaintColors>().colorDict[colorKey];
            Debug.Log("selected paint:  " + selectedPaint + " color key: " + colorKey);
            if(playerModel != null)
                playerModel.SetColor("_EmissionColor", selectedPaint);
        }

        if (PlayerManager.instance.playerInputs.Previous.WasPressedThisFrame())
        {
            Debug.Log("prev color");
            colorKey--;
            if (colorKey < 0)
            {
                colorKey = 2;
            }
            selectedPaint = PaintManager.instance.GetComponent<PaintColors>().colorDict[colorKey];
            Debug.Log("selected paint:  " + selectedPaint + " color key: " + colorKey);
            if(playerModel != null)
                playerModel.SetColor("_EmissionColor", selectedPaint);
        }
        
        
    }
}
