using UnityEngine;

public class TurretManager : MonoBehaviour
{
    public static TurretManager Instance;

    public GameObject turretPrefab;
    public float respawnCooldown = 10f;
    
    private float _cooldownTimer = 0f;
    private bool _isCooldownActive = false;
    private bool _hasActiveTurret = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // Если скилла еще нет, ничего не делаем
        if (PlayerController.Instance == null || !PlayerController.Instance.hasTurretSkill) return;

        // Единоразовый спавн первой турели сразу после взятия скилла
        if (!_hasActiveTurret && !_isCooldownActive)
        {
            SpawnTurret();
        }

        // Логика кулдауна
        if (_isCooldownActive)
        {
            _cooldownTimer -= Time.deltaTime;
            if (_cooldownTimer <= 0)
            {
                _isCooldownActive = false;
                SpawnTurret();
            }
        }
    }

    public void StartCooldown()
    {
        _hasActiveTurret = false;
        _isCooldownActive = true;
        _cooldownTimer = respawnCooldown;
    }

    private void SpawnTurret()
    {
        Node[] allNodes = FindObjectsOfType<Node>();
        if (allNodes.Length == 0) return;

        // Ищем свободную ноду (не ту, где стоит игрок, и без других зданий)
        Node targetNode = null;
        int attempts = 10;
        
        while (attempts > 0)
        {
            Node randomNode = allNodes[Random.Range(0, allNodes.Length)];
            if (randomNode != PlayerController.Instance.currentNode && randomNode.currentBuilding == null)
            {
                targetNode = randomNode;
                break;
            }
            attempts--;
        }

        if (targetNode != null)
        {
            // Определяем позицию: берем точку спавна, а если ее нет - ставим по старинке чуть выше центра
            Vector3 spawnPos = targetNode.buildingSpawnPoint != null 
                ? targetNode.buildingSpawnPoint.position 
                : targetNode.transform.position + new Vector3(0, 0.5f, 0);

            GameObject turretObj = Instantiate(turretPrefab, spawnPos, Quaternion.identity);
            Turret turretScript = turretObj.GetComponent<Turret>();
            
            if (turretScript != null)
            {
                turretScript.Setup(targetNode);
                _hasActiveTurret = true;
            }
        }
    }
}