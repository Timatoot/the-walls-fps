using UnityEngine;

public class GunMovement : MonoBehaviour
{
    [Header("Kick-back")]
    [SerializeField] float kickDistance = 0.06f;   // metres (local –Z)
    [SerializeField] float returnSpeed = 14f;      // how fast it snaps back
    [SerializeField] AnimationCurve recoilCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Walk sway / bob")]
    [SerializeField] float swayAmount = 0.01f;
    [SerializeField] float bobFrequency = 7f;
    [SerializeField] float swayStartSpeed = 0.2f;
    [SerializeField] float fullSwaySpeed = 2.0f;
    [SerializeField] CharacterController cc;

    Vector3 defaultPos;
    float recoilTime;

    void Awake() => defaultPos = transform.localPosition;

    public void Fire() => recoilTime = 0f;

    void Update()
    {
        if (recoilTime < 1f) recoilTime += Time.deltaTime * returnSpeed;
        Vector3 kick = Vector3.back * kickDistance * (1f - recoilCurve.Evaluate(recoilTime));

        float speed = cc ? cc.velocity.magnitude : 0f;

        float intensity = Mathf.InverseLerp(swayStartSpeed, fullSwaySpeed, speed);
        float bob = Mathf.Sin(Time.time * bobFrequency) * swayAmount * intensity;

        Vector3 sway = new Vector3(bob * 0.5f, -Mathf.Abs(bob), 0f);

        transform.localPosition = defaultPos + kick + sway;
    }
}
