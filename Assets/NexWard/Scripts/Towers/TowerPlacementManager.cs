
using UnityEngine;

public class TowerPlacementManager : MonoBehaviour
{
    public static TowerPlacementManager Instance { get; private set; }
    public static bool IsPlacing { get; private set; }

    [Header("Tower Prefabs")]
    [SerializeField] private TowerAttack laserTowerPrefab;
    [SerializeField] private TowerAttack teslaTowerPrefab;

    [Header("Construction Costs")]
    [SerializeField] private int laserTowerCost = 10;
    [SerializeField] private int teslaTowerCost = 15;

    [Header("Tower Capacity")]
    [SerializeField] private int additionalCapacityCost = 30;
    [SerializeField] private int startingCapacity = 1;
    [SerializeField] private int maximumCapacity = 5;

    [Header("Placement Area")]
    [SerializeField]
    private Vector2 minBounds =
        new Vector2(-7.5f, -3.5f);

    [SerializeField]
    private Vector2 maxBounds =
        new Vector2(7.5f, 3.5f);

    [SerializeField]
    private Vector2 placementSize =
        new Vector2(0.9f, 0.9f);

    private Camera mainCamera;
    private PlayerResources playerResources;

    private TowerAttack selectedTowerPrefab;
    private int selectedTowerCost;

    private GameObject preview;
    private SpriteRenderer previewRenderer;
    private TowerRangeIndicator previewRange;

    private Vector3 previewPosition;

    private int towerCapacity;
    private int towersPlaced;

    private bool canPlace;

    public int TowerCapacity => towerCapacity;
    public int TowersPlaced => towersPlaced;

    public string SelectedTowerName =>
        selectedTowerPrefab != null
        ? selectedTowerPrefab.name
        : "None";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        mainCamera = Camera.main;
        playerResources = FindAnyObjectByType<PlayerResources>();

        towerCapacity = startingCapacity;

        SelectTower(laserTowerPrefab, laserTowerCost);
    }

    private void Update()
    {
        if (!IsPlacing &&
            (TowerUpgradePanel.IsOpen ||
             CoreUpgradePanel.IsOpen ||
             PlayerUpgradePanel.IsOpen))
        {
            return;
        }

        // Select Laser Tower.
        if (!IsPlacing && Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectTower(laserTowerPrefab, laserTowerCost);
        }

        // Select Tesla Tower.
        if (!IsPlacing && Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectTower(teslaTowerPrefab, teslaTowerCost);
        }

        // Purchase another tower slot.
        if (Input.GetKeyDown(KeyCode.U) && !IsPlacing)
        {
            BuyAdditionalCapacity();
        }

        // Start or cancel placement mode.
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (IsPlacing)
                CancelPlacement();
            else
                BeginPlacement();
        }

        if (!IsPlacing)
            return;

        if (preview == null)
        {
            IsPlacing = false;
            return;
        }

        UpdatePreview();

        if (Input.GetMouseButtonDown(1) ||
            Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacement();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            AttemptPlace();
        }
    }

    // ---------------- TOWER SELECTION ----------------

    private void SelectTower(TowerAttack prefab, int cost)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Tower prefab is not assigned!");
            return;
        }

        selectedTowerPrefab = prefab;
        selectedTowerCost = cost;

        Debug.Log(
            $"Selected {prefab.name}. Build cost: {cost} Scrap."
        );
    }

    // ---------------- PLACEMENT ----------------

    private void BeginPlacement()
    {
        if (selectedTowerPrefab == null ||
            mainCamera == null ||
            playerResources == null)
            return;

        if (towersPlaced >= towerCapacity)
        {
            Debug.Log(
                "Tower capacity full! Press U to purchase another slot."
            );
            return;
        }

        if (playerResources.Scrap < selectedTowerCost)
        {
            Debug.Log("Not enough Scrap to build this tower.");
            return;
        }

        preview = Instantiate(selectedTowerPrefab.gameObject);

        preview.name =
            selectedTowerPrefab.name + "_PlacementPreview";

        TowerAttack attack = preview.GetComponent<TowerAttack>();

        if (attack != null)
            attack.enabled = false;

        TowerHealth health = preview.GetComponent<TowerHealth>();

        if (health != null)
            health.enabled = false;

        Collider2D[] colliders =
            preview.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        previewRenderer =
            preview.GetComponentInChildren<SpriteRenderer>();

        if (previewRenderer == null)
        {
            Destroy(preview);
            preview = null;
            return;
        }

        previewRenderer.sortingOrder = 10;

        previewRange =
            preview.GetComponentInChildren<TowerRangeIndicator>();

        if (previewRange != null)
        {
            previewRange.Show(true);
            previewRange.SetColor(Color.green);
        }

        IsPlacing = true;

        Debug.Log(
            $"Placing {selectedTowerPrefab.name}. " +
            $"Cost: {selectedTowerCost} Scrap."
        );
    }

    private void UpdatePreview()
    {
        Vector3 screenPosition = Input.mousePosition;

        screenPosition.z =
            Mathf.Abs(mainCamera.transform.position.z);

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(screenPosition);

        previewPosition = new Vector3(
            worldPosition.x,
            worldPosition.y,
            selectedTowerPrefab.transform.position.z
        );

        preview.transform.position = previewPosition;

        canPlace = CanPlaceAt(previewPosition);

        previewRenderer.color = canPlace
            ? new Color(0.2f, 1f, 0.3f, 0.55f)
            : new Color(1f, 0.2f, 0.2f, 0.55f);

        if (previewRange != null)
        {
            previewRange.SetColor(canPlace
                ? new Color(0.2f, 1f, 0.3f, 0.8f)
                : new Color(1f, 0.2f, 0.2f, 0.8f));
        }
    }

    private bool CanPlaceAt(Vector2 position)
    {
        float halfWidth = placementSize.x / 2f;
        float halfHeight = placementSize.y / 2f;

        if (position.x < minBounds.x + halfWidth ||
            position.x > maxBounds.x - halfWidth ||
            position.y < minBounds.y + halfHeight ||
            position.y > maxBounds.y - halfHeight)
        {
            return false;
        }

        Collider2D[] overlaps = Physics2D.OverlapBoxAll(
            position,
            placementSize,
            0f
        );

        foreach (Collider2D hit in overlaps)
        {
            if (hit.GetComponentInParent<CoreHealth>() != null ||
                hit.GetComponentInParent<TowerHealth>() != null ||
                hit.GetComponentInParent<EnemyAI>() != null ||
                hit.GetComponentInParent<PlayerResources>() != null ||
                hit.GetComponentInParent<ResourcePickup>() != null)
            {
                return false;
            }
        }

        return true;
    }

    private void AttemptPlace()
    {
        if (!canPlace)
        {
            Debug.Log("Invalid tower placement position.");
            return;
        }

        if (towersPlaced >= towerCapacity)
            return;

        if (!playerResources.SpendScrap(selectedTowerCost))
        {
            Debug.Log("Not enough Scrap.");
            return;
        }

        TowerAttack builtTower = Instantiate(
            selectedTowerPrefab,
            previewPosition,
            preview.transform.rotation
        );

        TowerHealth towerHealth =
            builtTower.GetComponent<TowerHealth>();

        if (towerHealth != null)
        {
            towerHealth.SetBuildCost(selectedTowerCost);
        }

        towersPlaced++;

        Debug.Log(
            $"{builtTower.name} built! " +
            $"Capacity: {towersPlaced}/{towerCapacity}"
        );

        CancelPlacement();
    }

    // ---------------- CAPACITY ----------------

    private void BuyAdditionalCapacity()
    {
        if (towerCapacity >= maximumCapacity)
        {
            Debug.Log("Maximum tower capacity reached.");
            return;
        }

        if (playerResources == null)
            return;

        if (!playerResources.SpendScrap(additionalCapacityCost))
        {
            Debug.Log("Not enough Scrap to purchase capacity.");
            return;
        }

        towerCapacity++;

        Debug.Log(
            $"Tower capacity increased: {towerCapacity}"
        );
    }

    public void OnTowerDestroyed()
    {
        towersPlaced = Mathf.Max(0, towersPlaced - 1);

        Debug.Log(
            $"Tower removed. Capacity: {towersPlaced}/{towerCapacity}"
        );
    }

    // ---------------- CANCEL ----------------

    private void CancelPlacement()
    {
        if (preview != null)
            Destroy(preview);

        preview = null;
        previewRenderer = null;
        previewRange = null;

        IsPlacing = false;

        Debug.Log("Tower placement cancelled.");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            IsPlacing = false;
        }
    }
}
