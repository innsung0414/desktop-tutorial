using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Turns the player left and right, and the camera up and down, using the mouse.
/// DeepBlueMenus owns cursor lock and Escape while the menu system is present.
/// </summary>
public class FirstPersonLook : MonoBehaviour
{
    private const float MaxMouseDeltaPerFrame = 12f;
    private const float CursorRecaptureIgnoreSeconds = 0.12f;

    [SerializeField] private Transform cameraPivot;
    [SerializeField, Range(0.05f, 1f)] private float sensitivity = 0.15f;
    [SerializeField, Range(30f, 89f)] private float maxPitch = 80f;

    private float pitch;
    private float ignoreMouseUntil;
    private bool lookEnabled;

    private void Start()
    {
        // Game 뷰가 아직 포커스를 받기 전에 커서를 잠그면, 첫 클릭 때 생기는
        // 큰 포인터 이동량이 시점 회전으로 전달될 수 있어 입력을 받을 때 잠급니다.
        if (DeepBlueMenus.Instance == null) SetCursorLocked(false);

        if (cameraPivot == null)
        {
            Debug.LogError("FirstPersonLook: Assign Main Camera to Camera Pivot in the Inspector.", this);
        }
    }

    private void Update()
    {
        if (DeepBlueMenus.BlocksGameplay) return;
        if (DeepBlueMenus.Instance == null && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            lookEnabled = !lookEnabled;
            SetCursorLocked(lookEnabled);
        }

        Mouse mouse = Mouse.current;
        if (!lookEnabled && mouse != null
            && (mouse.leftButton.wasPressedThisFrame
                || mouse.rightButton.wasPressedThisFrame
                || mouse.middleButton.wasPressedThisFrame))
        {
            lookEnabled = true;
            SetCursorLocked(true);
        }

        if (!lookEnabled || cameraPivot == null || mouse == null)
        {
            return;
        }

        // Game 뷰를 클릭하거나 Escape로 커서를 다시 잠글 때 생기는 포인터 이동을
        // 잠시 무시해, 포커스를 옮긴 것만으로 카메라가 갑자기 돌아가지 않게 합니다.
        if (mouse.leftButton.wasPressedThisFrame
            || mouse.rightButton.wasPressedThisFrame
            || mouse.middleButton.wasPressedThisFrame)
        {
            ignoreMouseUntil = Time.unscaledTime + CursorRecaptureIgnoreSeconds;
        }

        if (Time.unscaledTime < ignoreMouseUntil)
        {
            return;
        }

        Vector2 mouseDelta = mouse.delta.ReadValue();
        // Game 뷰에 처음 포커스를 줄 때 포인터 위치가 크게 이동할 수 있습니다.
        // 한 프레임의 입력값을 제한해 시점이 갑자기 위나 아래로 튀지 않게 합니다.
        mouseDelta.x = Mathf.Clamp(mouseDelta.x, -MaxMouseDeltaPerFrame, MaxMouseDeltaPerFrame);
        mouseDelta.y = Mathf.Clamp(mouseDelta.y, -MaxMouseDeltaPerFrame, MaxMouseDeltaPerFrame);

        // Rotate the player body left and right.
        float activeSensitivity = DeepBlueMenus.Instance != null ? DeepBlueProfile.Sensitivity : sensitivity;
        transform.Rotate(0f, mouseDelta.x * activeSensitivity, 0f);

        // Rotate the camera up and down, while preventing it from flipping over.
        pitch -= mouseDelta.y * activeSensitivity;
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    public void SetMenuCursorLocked(bool locked)
    {
        lookEnabled = locked;
        SetCursorLocked(locked);
    }

    private void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
        if (locked)
        {
            ignoreMouseUntil = Time.unscaledTime + CursorRecaptureIgnoreSeconds;
        }
    }
}
