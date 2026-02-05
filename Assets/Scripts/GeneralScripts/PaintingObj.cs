using UnityEngine;

public class PaintingObj : MonoBehaviour
{
    private Paintable m_paintable;
    public bool objComplete;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_paintable = GetComponent<Paintable>();
        objComplete = false;
        GameManager.instance.objectives.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        if(m_paintable.covered && !objComplete)
        {
            ObjComplete();
            objComplete = true;
        }
        else
        {
            ObjUncomplete();
            objComplete = false;
        }
    }
    public void ObjComplete()
    {
        GameManager.instance.amountOfObjComplete++;
    }
    public void ObjUncomplete()
    {
        GameManager.instance.amountOfObjComplete--;
    }
}
