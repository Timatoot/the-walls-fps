using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] Vector3 offset = new Vector3(0, 2.2f, 0);

    Transform target;

    public void Init(Transform followTarget, float maxHp)
    {
        target = followTarget;
        slider.maxValue = maxHp;
        slider.value = maxHp;
    }

    public void OnHealthChanged(float value) => slider.value = value;

    void LateUpdate()
    {
        if (!target) return;
        transform.position = target.position + offset;
        transform.forward = Camera.main.transform.forward;
    }
}

