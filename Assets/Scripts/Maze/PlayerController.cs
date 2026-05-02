using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Movimiento")]
    public float moveSpeed      = 20f;
    public float swipeThreshold = 40f;

    [Header("Animación")]
    public bool  animateScale   = true;
    [Range(0.05f, 0.3f)]
    public float squashDuration = 0.12f;

    [Header("Efectos")]
    public ParticleSystem moveParticles;
    public ParticleSystem bounceParticles;

    private Rigidbody    rb;
    private AudioSource  audioSource;
    private Vector2      touchStart;
    private bool         isMoving          = false;
    private Vector3      targetPosition;
    private Vector3      lastMoveDir       = Vector3.forward;
    private bool         bouncedThisLevel  = false;
    private int          wallHitsThisLevel = 0;
    private bool         levelCompleted    = false;

    private Vector3 baseScale;
    private float   squashTimer = 0f;
    private bool    isSquashing = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Mesh esfera
        var mf = GetComponent<MeshFilter>();
        if (mf != null)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mf.mesh = sphere.GetComponent<MeshFilter>().sharedMesh;
            Destroy(sphere);
        }

        // Material verde brillante
        var rend = GetComponent<Renderer>();
        if (rend != null)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.1f, 0.9f, 0.2f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.05f, 0.4f, 0.05f));
            mat.SetFloat("_Metallic",   0.1f);
            mat.SetFloat("_Glossiness", 0.9f);
            rend.material = mat;
        }

        baseScale = transform.localScale;

        // Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic            = true;
        rb.useGravity             = false;
        rb.interpolation          = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.constraints = RigidbodyConstraints.FreezePositionY; // Solo Y fijo, rotación libre

        // Collider
        foreach (var c in GetComponents<Collider>()) Destroy(c);
        var bc       = gameObject.AddComponent<BoxCollider>();
        bc.isTrigger = false;
        bc.size      = new Vector3(0.55f, 0.55f, 0.55f);
        bc.center    = Vector3.zero;

        // AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume      = 0.8f;
        audioSource.pitch       = 1f;
    }

    private void Start()
    {
        targetPosition     = new Vector3(1f, 0.5f, 1f);
        transform.position = targetPosition;
    }

    private void Update()
    {
        HandleInput();
        AnimateScale();
        CheckExitOverlap();
    }

    private void FixedUpdate()
    {
        SmoothMove();
    }

    private void HandleInput()
    {
        if (isMoving || levelCompleted) return;

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began) touchStart = t.position;
            if (t.phase == TouchPhase.Ended)
            {
                Vector2 d = t.position - touchStart;
                if (d.magnitude < swipeThreshold) return;
                if (Mathf.Abs(d.x) > Mathf.Abs(d.y))
                    TryMove(d.x > 0 ? Vector3.right : Vector3.left);
                else
                    TryMove(d.y > 0 ? Vector3.forward : Vector3.back);
            }
        }
        else // Simular touch con el Mouse en la computadora
        {
            if (Input.GetMouseButtonDown(0))
            {
                touchStart = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                Vector2 d = (Vector2)Input.mousePosition - touchStart;
                if (d.magnitude >= swipeThreshold)
                {
                    if (Mathf.Abs(d.x) > Mathf.Abs(d.y))
                        TryMove(d.x > 0 ? Vector3.right : Vector3.left);
                    else
                        TryMove(d.y > 0 ? Vector3.forward : Vector3.back);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow)    || Input.GetKeyDown(KeyCode.W)) TryMove(Vector3.forward);
        if (Input.GetKeyDown(KeyCode.DownArrow)  || Input.GetKeyDown(KeyCode.S)) TryMove(Vector3.back);
        if (Input.GetKeyDown(KeyCode.LeftArrow)  || Input.GetKeyDown(KeyCode.A)) TryMove(Vector3.left);
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) TryMove(Vector3.right);
    }

    public void MoveInDirection(Vector3 direction)
    {
        if (!isMoving && !levelCompleted)
            TryMove(direction);
    }

    private void TryMove(Vector3 dir)
    {
        Vector3 current = new Vector3(
            Mathf.Round(transform.position.x),
            transform.position.y,
            Mathf.Round(transform.position.z));

        Vector3 dest = current + dir;

        bool wallFound = false;
        Collider[] hits = Physics.OverlapBox(
            dest + Vector3.up * 0.5f,
            new Vector3(0.3f, 0.4f, 0.3f),
            Quaternion.identity);

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            if (hit.CompareTag("Wall")) { wallFound = true; break; }
        }

        if (wallFound) { OnWallHit(dir); return; }

        lastMoveDir    = dir;
        targetPosition = dest;
        isMoving       = true;
        TriggerSquash(dir);
        moveParticles?.Play();

        // Sonido de movimiento — pitch aleatorio para que no suene repetitivo
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.2f);
            audioSource.Play();
        }
        AudioManager.Instance?.Play("move");

#if UNITY_ANDROID && !UNITY_EDITOR
        if (GameManager.Instance != null && GameManager.Instance.vibrationsEnabled)
            Handheld.Vibrate();
#endif
    }

    private void OnWallHit(Vector3 dir)
    {
        bouncedThisLevel = true;
        wallHitsThisLevel++;
        StartCoroutine(BounceAnim(dir));
        bounceParticles?.Play();
        AudioManager.Instance?.Play("bounce");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpendLife();
            if (GameManager.Instance.lives <= 0)
                GameController.Instance?.OnGameOver();
        }
    }

    private void SmoothMove()
    {
        if (!isMoving) return;
        Vector3 next = Vector3.MoveTowards(rb.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(next);

        // Rotación instantánea al presionar — sin lerp
        if (lastMoveDir != Vector3.zero)
            rb.rotation = Quaternion.LookRotation(lastMoveDir, Vector3.up);

        if (Vector3.Distance(rb.position, targetPosition) < 0.02f)
        {
            rb.position = targetPosition;
            isMoving    = false;
        }
    }

    private void CheckExitOverlap()
    {
        if (levelCompleted || isMoving) return;
        Collider[] hits = Physics.OverlapSphere(transform.position, 0.4f);
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            if (hit.CompareTag("Exit")) { CompletarNivel(); return; }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Exit")) CompletarNivel();
    }

    private void CompletarNivel()
    {
        if (levelCompleted) return;
        levelCompleted = true;
        Debug.Log("[Player] ¡Nivel completado!");
        if (!bouncedThisLevel)
            AchievementManager.Instance?.TrackEvent(AchievementEvent.LevelCompletedNoBounce);
        GameController.Instance?.OnLevelComplete(0f, wallHitsThisLevel);
    }

    private void TriggerSquash(Vector3 moveDir) { isSquashing = true; squashTimer = 0f; }

    private System.Collections.IEnumerator BounceAnim(Vector3 dir)
    {
        float t = 0f;
        Vector3 origin = transform.position;
        Vector3 pushed = origin - dir * 0.15f;
        while (t < 0.12f)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(origin, pushed, Mathf.PingPong(t / 0.06f, 1f));
            yield return null;
        }
        transform.position = origin;
    }

    private void AnimateScale()
    {
        if (!animateScale || !isSquashing) return;
        squashTimer += Time.deltaTime;
        float p = squashTimer / squashDuration;
        if (p >= 1f) { transform.localScale = baseScale; isSquashing = false; return; }
        float s = Mathf.Sin(p * Mathf.PI);
        transform.localScale = new Vector3(
            baseScale.x * (1f + s * 0.25f),
            baseScale.y * (1f - s * 0.35f),
            baseScale.z * (1f + s * 0.25f));
    }

    public void ResetForNewLevel()
    {
        isMoving          = false;
        isSquashing       = false;
        bouncedThisLevel  = false;
        levelCompleted    = false;
        wallHitsThisLevel = 0;
        targetPosition    = new Vector3(1f, 0.5f, 1f);
        transform.position   = targetPosition;
        transform.localScale = baseScale != Vector3.zero ? baseScale : Vector3.one;
        transform.rotation   = Quaternion.identity;
    }
}
