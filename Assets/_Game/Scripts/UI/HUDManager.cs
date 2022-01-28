using Hedronoid;
using Hedronoid.Core;
using Hedronoid.Events;
using Hedronoid.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDManager : HNDMonoBehaviour, IGameplaySceneContextInjector
{
    public GameplaySceneContext GameplaySceneContext { get; set; }

    // Start is called before the first frame update
    public Canvas canvas;
    public Camera orbitCamera;
    public RectTransform dynamicCrosshair;
    public float restingSize, maxSize, speed;
    private float currentSize;
    private PlayerFSM player;
    private PlayerActionSet m_PlayerActions;
    private bool playerIntialized = false;
    protected override void Awake()
    {
        base.Awake();
        this.Inject(gameObject);

        m_PlayerActions = InputManager.Instance.PlayerActions;

        HNDEvents.Instance.AddListener<PlayerCreatedAndInitialized>(OnPlayerCreatedAndInitialized);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        HNDEvents.Instance.RemoveListener<PlayerCreatedAndInitialized>(OnPlayerCreatedAndInitialized);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();

        HNDEvents.Instance.RemoveListener<PlayerCreatedAndInitialized>(OnPlayerCreatedAndInitialized);
    }
    protected override void Start()
    {
        base.Start();

        if (!playerIntialized) return;

        if (!canvas) TryGetComponent(out canvas);
        if (!orbitCamera) orbitCamera = GameplaySceneContext.OrbitCamera.orbitCamera;

        dynamicCrosshair.gameObject.SetActive(false);

        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = orbitCamera;
        canvas.planeDistance = 1;
    }

    private void LateUpdate()
    {
        if (!playerIntialized) return;

        if (!player) player = GameplaySceneContext.Player;
        if (!player) return;

        if (m_PlayerActions.Aim.WasPressed)
            dynamicCrosshair.gameObject.SetActive(true);
        else if (m_PlayerActions.Aim.WasReleased)
            dynamicCrosshair.gameObject.SetActive(false);

        if (player.IsShooting)
        {
            currentSize = Mathf.Lerp(currentSize, maxSize, Time.deltaTime * speed);
        }
        else
        {
            currentSize = Mathf.Lerp(currentSize, restingSize, Time.deltaTime * speed);
        }

        dynamicCrosshair.sizeDelta = new Vector2(currentSize, currentSize);
    }

    private void OnPlayerCreatedAndInitialized(PlayerCreatedAndInitialized e)
    {
        Start();
        playerIntialized = true;
    }
}
