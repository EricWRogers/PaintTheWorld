using UnityEngine;

public class PlayerPaint : GetPaintColor
{
    public Color selectedPaint;

    void Start()
    {
        selectedPaint = PaintManager.instance.GetComponent<PaintColors>().damagePaint;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedPaint = PaintManager.instance.GetComponent<PaintColors>().damagePaint;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedPaint = PaintManager.instance.GetComponent<PaintColors>().jumpPaint;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            selectedPaint = PaintManager.instance.GetComponent<PaintColors>().movementPaint;
        }
    }
}
