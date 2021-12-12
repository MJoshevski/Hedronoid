using Hedronoid;
using Hedronoid.Core;
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
    protected override void Awake()
    {
        base.Awake();
        this.Inject(gameObject);
    }

    protected override void Start()
    {
        base.Start();

        if (!canvas) TryGetComponent(out canvas);
        if (!orbitCamera) orbitCamera = GameplaySceneContext.OrbitCamera.orbitCamera;

        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = orbitCamera;
        canvas.planeDistance = 1;
    }

    private void LateUpdate()
    {
        if (!player) player = GameplaySceneContext.Player;
        if (!player) return;

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
}
