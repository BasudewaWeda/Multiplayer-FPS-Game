using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviourPunCallbacks, iDamageable
{
    // Movement
    [Header("Movement")]
    PhotonView view;
    public Transform orientation;
    Rigidbody rb;
    public LayerMask ground;

    private float moveHorizontal;
    private float moveVertical;
    Vector3 moveDirection;
    [SerializeField] float moveSpeed;
    [SerializeField] float speedSmooth;
    [SerializeField] float airMultiplier;
    [SerializeField] float groundDrag;
    float desiredSpeed;
    float currentMoveSpeed;

    public bool isGrounded;

    [SerializeField] float jumpForce;
    [SerializeField] float fallingMultiplier;

    [SerializeField] float wallCheckDistance;
    [SerializeField] float minJumpHeight;
    [SerializeField] float wallRunGravity;
    [SerializeField] float sideWallJumpForce;
    [SerializeField] float wallJumpForce;
    [SerializeField] bool isAboveGround;
    private RaycastHit rightWallhit;
    private RaycastHit leftWallhit;
    public bool rightWall;
    public bool leftWall;
    public bool isWallRunning;

    bool frontWall;
    public bool isWallClimbing;
    RaycastHit frontWallHit;
    [SerializeField] float climbForce;
    [SerializeField] float backJumpForce;

    private RaycastHit slopeHit;
    bool onSlope;
    [SerializeField] float maxSlopeAngle;

    [SerializeField] float maxSlideTimer;
    [SerializeField] float slideSpeed;
    [SerializeField] float slideYScale;
    [SerializeField] float startYScale;
    [SerializeField] float slideTimer;
    public bool isSliding;

    // Look Around
    private float mouseX;
    private float mouseY;
    private float xRotation;
    private float yRotation;
    [HideInInspector]
    public float mouseSensitivity;

    [Header("Look Around")]
    public Camera mainCamera;
    [SerializeField] float tilt;
    [SerializeField] float zoomOut;
    [SerializeField] float zoomIn;
    [SerializeField] float defaultZoom;
    [SerializeField] float smooth;
    float tiltAmount;
    float zoomAmount;

    [SerializeField] Transform camTransform;
    [SerializeField] Transform cameraHolder;
    float lowestYSpeed;
    float currentLowestYSpeed;
    float bounceAmount;
    [SerializeField] float scopeZoom;
    bool isScoping;

    // Gun Holding
    [Header("Gun Holding")]
    [SerializeField] Item[] items;
    public int itemIndex;
    int previousItemIndex = -1;
    bool canSwitch = true;
    [SerializeField] CanvasGroup weaponHud;
    [SerializeField] RectTransform[] weaponHudPositions;
    [SerializeField] RectTransform weaponHudHover;
    [SerializeField] float smoothness;

    // Health Stuff
    [Header("Health Stuff")]
    public float maxHealth = 100f;
    public float currentHealth;

    // UI
    [Header("UI")]
    [SerializeField] GameObject UI;
    [SerializeField] Slider healthBar;
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject scopeImage;
    [SerializeField] GameObject ammoText;

    [Header("Damage Indicator")]
    [SerializeField] RectTransform damageIndicatorParent;
    [SerializeField] RectTransform damageIndicatorPrefab;

    PlayerManager playerManager;
    Player killer;

    AudioSource ac;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        view = GetComponent<PhotonView>();
        playerManager = PhotonView.Find((int)view.InstantiationData[0]).GetComponent<PlayerManager>();
        ac = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (view.IsMine)
        {
            EquipItem(0);
        }
        else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
            Destroy(UI);
        }
        zoomAmount = defaultZoom;

        Cursor.lockState = CursorLockMode.Locked;

        healthBar.maxValue = maxHealth;

        currentHealth = maxHealth;

        mouseSensitivity = PlayerPrefs.GetFloat("sensitivity");
    }

    void Update()
    {
        if (!view.IsMine) return;

        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveVertical = Input.GetAxisRaw("Vertical");

        isGrounded = IsGrounded();
        isAboveGround = AboveGround();
        onSlope = OnSlope();

        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }

        if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C)) && (moveHorizontal != 0 || moveVertical != 0) && isGrounded)
        {
            ShrinkPlayer();
        }

        if ((Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.C)) && isSliding)
        {
            UnShrinkPlayer();
        }

        if (!pausePanel.activeSelf) // Can't do these if paused
        {
            Look();
            Shooting();
            Reload();
            WeaponSwitch();
        }

        if (transform.position.y <= -10f) Die();

        healthBar.value = Mathf.Lerp(healthBar.value, currentHealth, smooth * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!pausePanel.activeSelf)
            {
                pausePanel.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
            }
            else if (pausePanel.activeSelf)
            {
                pausePanel.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        if ((moveHorizontal != 0 || moveVertical != 0) && (isGrounded || isWallClimbing || isWallRunning)) view.RPC(nameof(RPC_PlayFootstepSound), RpcTarget.All);
        else view.RPC(nameof(RPC_StopFootstepSound), RpcTarget.All);

        weaponHudHover.anchoredPosition = Vector3.Lerp(weaponHudHover.anchoredPosition, weaponHudPositions[itemIndex].anchoredPosition, smoothness * Time.deltaTime);

        weaponHud.alpha = Mathf.Lerp(weaponHud.alpha, 0f, Time.deltaTime);

        if (items[itemIndex].itemInfo.itemName == "AWP" && Input.GetMouseButton(1))
        {
            mouseSensitivity = PlayerPrefs.GetFloat("sensitivity") / 2;
            isScoping = true;
            scopeImage.SetActive(true);
        }
        else
        {
            mouseSensitivity = PlayerPrefs.GetFloat("sensitivity");
            isScoping = false;
            scopeImage.SetActive(false);
        }

        if (items[itemIndex].itemInfo.itemName == "Knife")
        {
            ammoText.SetActive(false);
        }
        else
        {
            ammoText.SetActive(true);
        }
    }

    void FixedUpdate()
    {
        if (!view.IsMine) return;

        if (moveHorizontal != 0 || moveVertical != 0)
        {
            if (isSliding)
            {
                desiredSpeed = slideSpeed;
            }
            else
            {
                desiredSpeed = moveSpeed;
            }
        }

        currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, desiredSpeed, speedSmooth * Time.deltaTime);

        moveDirection = orientation.forward * moveVertical + orientation.right * moveHorizontal; // Calculating movement

        if (isWallClimbing && isAboveGround && (moveVertical > 0))
        {
            rb.AddForce(climbForce * moveVertical * Vector3.up, ForceMode.Force);
        }
        else if (onSlope && !Input.GetKey(KeyCode.Space))
        {
            rb.AddForce(Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized * currentMoveSpeed, ForceMode.Force);
            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        else if (!isWallRunning && isGrounded && !onSlope)
        {
            rb.AddForce(moveDirection.normalized * currentMoveSpeed, ForceMode.Force);
        }
        else if (!isWallRunning && !isGrounded)
        {
            rb.AddForce(moveDirection.normalized * currentMoveSpeed * airMultiplier, ForceMode.Force);
        }

        Vector3 currentSpeed = new(rb.velocity.x, 0f, rb.velocity.z);

        if (currentSpeed.magnitude >= (currentMoveSpeed / 10)) // Speed Limit
        {
            Vector3 limitedSpeed = currentSpeed.normalized * (currentMoveSpeed / 10);
            rb.velocity = new Vector3(limitedSpeed.x, rb.velocity.y, limitedSpeed.z);
        }

        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        if (rb.velocity.y <= 0) // Fall faster
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + (rb.velocity.y * fallingMultiplier), rb.velocity.z);
        }

        rightWall = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, ground);
        leftWall = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance, ground);
        frontWall = Physics.SphereCast(transform.position, .5f, orientation.forward, out frontWallHit, wallCheckDistance, ground);

        if ((rightWall || leftWall) && moveVertical > 0 && isAboveGround) // Wallrun
        {
            isWallRunning = true;

            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

            Vector3 wallNormal = rightWall ? rightWallhit.normal : leftWallhit.normal;

            Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up);

            if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            {
                wallForward = -wallForward;
            }

            rb.AddForce(wallForward * currentMoveSpeed + wallRunGravity * Vector3.down, ForceMode.Force);

            if (Input.GetKey(KeyCode.Space))
            {
                WallJump();
            }
        }
        else
        {
            isWallRunning = false;
        }

        if (isSliding)
        {
            if (!onSlope || (onSlope && (rb.velocity.y > 0)))
            {
                Vector3 slideDirection = orientation.forward * moveVertical + orientation.right * moveHorizontal;

                rb.AddForce(slideDirection.normalized * currentMoveSpeed, ForceMode.Force);

                slideTimer += Time.deltaTime;
            }
            else if (onSlope && rb.velocity.y < 0)
            {
                rb.AddForce(Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized * currentMoveSpeed, ForceMode.Force);
            }

            if (slideTimer >= maxSlideTimer)
            {
                UnShrinkPlayer();
            }
        }

        if (frontWall && !(leftWall || rightWall) && isAboveGround) // Wall climbing
        {
            isWallClimbing = true;
            if (Input.GetKey(KeyCode.Space))
            {
                BackWallJump();
            }
            else if (!Input.GetKey(KeyCode.Space))
            {
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
            }
        }
        else if (!frontWall)
        {
            isWallClimbing = false;
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, 1.2f, ground);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, -Vector3.up, minJumpHeight, ground);
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void WallJump()
    {
        Vector3 wallNormal = rightWall ? rightWallhit.normal : leftWallhit.normal;

        Vector3 appliedForce = Vector3.up * wallJumpForce + wallNormal * sideWallJumpForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(appliedForce, ForceMode.Impulse);
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, 1.2f, ground))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private void ShrinkPlayer() // Start Slide
    {
        isSliding = true;
        transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    private void UnShrinkPlayer() // Stop Slide
    {
        isSliding = false;
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        slideTimer = 0;
    }

    private void BackWallJump()
    {
        Vector3 wallNormal = frontWallHit.normal;

        Vector3 appliedForce = Vector3.up * wallJumpForce + wallNormal * backJumpForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(appliedForce, ForceMode.Impulse);
    }

    private void Look()
    {
        mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        yRotation += mouseX;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.Rotate(mouseX * Vector3.up);
        cameraHolder.localEulerAngles = Vector3.right * xRotation + Vector3.forward * tiltAmount;

        mainCamera.fieldOfView = zoomAmount;

        if (isScoping)
        {
            StartCoroutine(Scoping(0f));
        }
        else if (isWallRunning || isSliding)
        {
            zoomAmount = Mathf.Lerp(zoomAmount, zoomOut, smooth * Time.deltaTime);
            if (rightWall)
            {
                tiltAmount = Mathf.Lerp(tiltAmount, tilt, smooth * Time.deltaTime);
            }
            else if (leftWall)
            {
                tiltAmount = Mathf.Lerp(tiltAmount, -tilt, smooth * Time.deltaTime);
            }
        }
        else if (isWallClimbing)
        {
            zoomAmount = Mathf.Lerp(zoomAmount, zoomIn, smooth * Time.deltaTime);
        }

        zoomAmount = Mathf.Lerp(zoomAmount, defaultZoom, smooth * Time.deltaTime);
        tiltAmount = Mathf.Lerp(tiltAmount, 0, smooth * Time.deltaTime);

        if (rb.velocity.y <= 0)
        {
            currentLowestYSpeed = rb.velocity.y;
            if (currentLowestYSpeed < lowestYSpeed)
            {
                lowestYSpeed = currentLowestYSpeed;
            }
        }

        if (isGrounded) StartCoroutine(CameraFeedBack(0.25f));
    }

    private void WeaponSwitch()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                EquipItem(i);
                break;
            }
        }

        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            if (itemIndex >= items.Length - 1) EquipItem(0);
            else EquipItem(itemIndex + 1);
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            if (itemIndex <= 0) EquipItem(items.Length - 1);
            else EquipItem(itemIndex - 1);
        }
    }

    IEnumerator CameraFeedBack(float delay)
    {
        bounceAmount = Mathf.Lerp(bounceAmount, lowestYSpeed / 25, smooth * Time.deltaTime);
        camTransform.localPosition = new Vector3(0f, bounceAmount, 0f);
        yield return new WaitForSeconds(delay);
        bounceAmount = Mathf.Lerp(bounceAmount, 0, smooth * Time.deltaTime);
        camTransform.localPosition = new Vector3(0f, bounceAmount, 0f);
        lowestYSpeed = 0f;
    }

    private void EquipItem(int _index)
    {
        if (previousItemIndex == _index || !canSwitch) return;

        itemIndex = _index;
        items[itemIndex].itemGameObject.SetActive(true);

        if (previousItemIndex != -1)
        {
            items[previousItemIndex].itemGameObject.SetActive(false);
        }

        previousItemIndex = itemIndex;

        if (view.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

            Hashtable hash2 = new Hashtable();
            hash2.Add("itemName", items[itemIndex].itemInfo.itemName);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash2);
        } 

        StartCoroutine(WeaponSwitchCooldown(0.25f));

        weaponHud.alpha = 1f;
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("itemIndex") && !view.IsMine && targetPlayer == view.Owner)
        {
            EquipItem((int)changedProps["itemIndex"]);
        }
    }

    void Shooting()
    {
        if (Input.GetMouseButton(0))
        {
            items[itemIndex].Use();
        }
    }

    void Reload()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            items[itemIndex].SecondUse();
        }
    }

    public void TakeDamage(float damage)
    {
        view.RPC(nameof(RPC_TakeDamage), view.Owner, damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage, PhotonMessageInfo info)
    {
        currentHealth -= damage;

        killer = info.Sender;

        if (info.Sender != view.Owner)
        {
            StartCoroutine(ShowDamageIndicator(PlayerManager.Find(info.Sender).transform));
        }

        if (currentHealth <= 0)
        {
            Die();
            StopAllCoroutines();
            if (killer != view.Owner)
            {
                PlayerManager.Find(info.Sender).GetKill(view.Owner, false);
            }
            else
            {
                PlayerManager.Find(info.Sender).GetKill(view.Owner, true);
            }
        }
    }

    void Die()
    {
        if (killer == null) killer = view.Owner;
        playerManager.Die(killer);
    }

    public void LeaveRoom()
    {
        ResetStats();
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }

    IEnumerator WeaponSwitchCooldown(float delay)
    {
        canSwitch = false;
        yield return new WaitForSeconds(delay);
        canSwitch = true;
    }

    void ResetStats()
    {
        Hashtable hash = new Hashtable();
        hash.Add("kills", null);
        hash.Add("deaths", null);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    [PunRPC]
    void RPC_PlayFootstepSound()
    {
        ac.enabled = true;
    }

    [PunRPC]
    void RPC_StopFootstepSound()
    {
        ac.enabled = false;
    }
    
    IEnumerator Scoping(float delay)
    {
        zoomAmount = Mathf.Lerp(zoomAmount, scopeZoom, smooth * Time.deltaTime);
        float temp = zoomAmount;
        yield return new WaitForSeconds(delay);
        zoomAmount = temp;
    }

    IEnumerator ShowDamageIndicator(Transform damageSource)
    {
        RectTransform damageIndicator = Instantiate(damageIndicatorPrefab, damageIndicatorParent);
        Image graphic = damageIndicator.GetChild(0).GetComponent<Image>();
        while(graphic.color.a > 0)
        {
            Vector3 direction = damageSource.position - transform.position;
            Quaternion sourceRot = Quaternion.LookRotation(direction);
            sourceRot.z = -sourceRot.y;
            sourceRot.y = sourceRot.x = 0f;
            Vector3 northDirection = new Vector3(0f, 0f, orientation.eulerAngles.y);
            damageIndicator.localRotation = sourceRot * Quaternion.Euler(northDirection);
            yield return new WaitForEndOfFrame();
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, graphic.color.a - Time.deltaTime);
        }
    }

    public void ExplosionKnockBack(float knockBackForce, Vector3 explosionPos, float explosionRadius)
    {
        view.RPC(nameof(RPC_ExplosionKnockback), RpcTarget.All, knockBackForce, explosionPos, explosionRadius);
    }

    [PunRPC]
    void RPC_ExplosionKnockback(float knockBackForce, Vector3 explosionPos, float explosionRadius)
    {
        rb.AddExplosionForce(knockBackForce, explosionPos, explosionRadius, 1f, ForceMode.Impulse);
    }
}
