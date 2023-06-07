// https://github.com/furkansancu/unity-character-controller

// GameObject Installion:
// Player = +Rigidbody +CapsuleCollider +PlayerController
// Player -> Layer = "Player"

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Controller Customizer")]
    public bool resetPropsOnStart = true;

    [Header("Player Properties")]
    public int health = 100;
    public float stamina = 100;
    public float speed = 5;

    [Header("Player Settings")]
    public int maxHealth = 100;
    public float maxStamina = 100;
    public float jumpSize = 3f;
    public float moveSpeed = 3f;
    public float sprintSpeed = 5f;
    public float sprintDuration = 7f;
    public float sprintMinStamina = 15f;

    [Header("Player Status")]
    public bool isGrounded = false;
    public bool isMoving = false;
    public bool isSprinting = false;

    [Header("Keybindings")]
    public KeyCode moveForwardKey = KeyCode.W;
    public KeyCode moveBackwardKey = KeyCode.S;
    public KeyCode moveLeftKey = KeyCode.A;
    public KeyCode moveRightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private Vector3 moveVelocity = Vector3.zero;

    void OnValidate () {
        TryGetComponent<Rigidbody>(out rb);
        TryGetComponent<CapsuleCollider>(out capsuleCollider);
    }

    void Awake () {
        if (resetPropsOnStart) {
            health = maxHealth;
            stamina = maxStamina;
            speed = moveSpeed;
        }
    }
    
    void Update () {
        isGrounded = CheckGrounded();
        isMoving = rb.velocity.x != 0 || rb.velocity.z != 0;
        isSprinting = CharacterSprint();
        speed = isSprinting ? sprintSpeed : moveSpeed;
        stamina = UpdateStamina();
        CharacterMovement();

        // Jump
        if (isGrounded && Input.GetKeyDown(jumpKey))
            rb.velocity = new Vector3(rb.velocity.x, jumpSize, rb.velocity.z);
    }

    void FixedUpdate () {
        Vector3 newVelocity = moveVelocity;
        if (newVelocity.y == 0) newVelocity.y = rb.velocity.y;
        newVelocity += FallMultiplier(newVelocity);
        rb.velocity = newVelocity;
    }

    void CharacterMovement () {
        moveVelocity.z = ((Input.GetKey(moveForwardKey) ? 1 : 0) - (Input.GetKey(moveBackwardKey) ? 1 : 0)) * speed;
        moveVelocity.x = ((Input.GetKey(moveRightKey) ? 1 : 0) - (Input.GetKey(moveLeftKey) ? 1 : 0)) * speed;
        moveVelocity.y = rb.velocity.y;
    }

    bool CharacterSprint () {
        if (Input.GetKeyDown(sprintKey) && stamina > sprintMinStamina)
            return true;
        else if (Input.GetKeyUp(sprintKey) || stamina <= 0)
            return false;

        return isSprinting;
    }

    float UpdateStamina () {
        if (isSprinting && isMoving) {
            if (stamina > 0)
                return stamina - Time.deltaTime * (maxStamina / sprintDuration);
            else if (stamina < 0)
                return 0;
        } else {
            if (stamina < maxStamina)
                return stamina + Time.deltaTime * (maxStamina / sprintDuration);
            else if (stamina > maxStamina)
                return maxStamina;
        }

        return stamina;
    }

    // !! Make sure to set player collider tag as "Player"
    bool CheckGrounded () {
        int layerMask =~ LayerMask.GetMask("Player");
        float sphereRadius = 0.25f;
        float offset = (capsuleCollider.height / 2) - (sphereRadius - (sphereRadius / 5));
        return Physics.CheckSphere(
            transform.position - transform.up * offset,
            sphereRadius,
            layerMask
        );
    }

    // Makes fall behaviour more realistic.
    Vector3 FallMultiplier (Vector3 velocity) {
        if (velocity.y == 0 || Input.GetKeyDown(jumpKey)) return Vector3.zero;
        float fallMultp = velocity.y < 0 ? 1.5f : 1f;
        return Vector3.up * Physics.gravity.y * fallMultp * Time.deltaTime;
    }
}
