using UnityEngine;

public class PaintBurstVFX : MonoBehaviour
{
  public float lifetime = 1.2f;
  void OnEnable() => Destroy(gameObject, lifetime);
}
