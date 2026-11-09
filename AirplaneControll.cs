using UnityEngine;
using UnityEngine.InputSystem;

public class AirplaneController : MonoBehaviour
{
    public float baseSpeed = 0.3f, minX = -15f, maxX = 15f;
    public AudioSource engineAudio, hitAudio, winAudio;

    private float curSpeed, hSpeed;
    private bool isDone;
    private Vector2 lastMouse;
    private Rigidbody rb;

    void Start()
    {
        Application.targetFrameRate = 60;
        curSpeed = baseSpeed;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isDone) return;

        // 1. 速度恢復 (撞擊後平滑加速回穩)
        if (curSpeed < baseSpeed) curSpeed += Time.deltaTime * 0.5f;

        // 2. 操控偵測 (滑鼠/觸控與鍵盤)
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
                lastMouse = Mouse.current.position.ReadValue();
            else if (Mouse.current.leftButton.isPressed)
            {
                Vector2 now = Mouse.current.position.ReadValue();
                hSpeed = (now.x - lastMouse.x) * 0.008f;
                lastMouse = now;
            }
        }
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) hSpeed = -0.3f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) hSpeed = 0.3f;
        }

        // 3. 移動與邊界鎖定 (運算合併)
        transform.Translate(hSpeed, 0, curSpeed, Space.World);
        hSpeed *= 0.85f; // 摩擦力

        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, minX, maxX);
        transform.position = p;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDone) return;

        if (other.CompareTag("Obstacle"))
        {
            if (hitAudio) hitAudio.Play();
            curSpeed = -0.5f; // 反彈
        }
        else if (other.name.Contains("Wall"))
        {
            isDone = true;
            if (engineAudio) engineAudio.Stop();
            if (winAudio) winAudio.Play();
            if (rb) rb.isKinematic = true; // 凍結物理
        }
    }
}
