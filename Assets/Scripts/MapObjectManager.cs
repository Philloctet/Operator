using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MapObjectData
{
    public string name;
    public GameObject prefab;
    public float spawnCooldown = 30f; // Через сколько появится новый
    public int maxOnMap = 1;          // Максимум таких объектов на карте одновременно

    [HideInInspector] public float currentTimer;
    [HideInInspector] public List<NodeInteractable> activeInstances = new List<NodeInteractable>();
}

public class MapObjectManager : MonoBehaviour
{
    public static MapObjectManager Instance;

    public List<MapObjectData> objectsToSpawn;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Даем начальную фору таймерам, чтобы всё не заспавнилось в 1-ю секунду игры
        foreach (var obj in objectsToSpawn)
        {
            obj.currentTimer = obj.spawnCooldown;
        }
    }

    void Update()
    {
        if (PlayerController.Instance == null) return;

        foreach (var obj in objectsToSpawn)
        {
            // Очищаем список от объектов, которые уже исчезли (были собраны или истекло время)
            obj.activeInstances.RemoveAll(item => item == null);

            // Если объектов на карте меньше максимума — крутим таймер спавна
            if (obj.activeInstances.Count < obj.maxOnMap)
            {
                obj.currentTimer -= Time.deltaTime;
                if (obj.currentTimer <= 0)
                {
                    SpawnObject(obj);
                    obj.currentTimer = obj.spawnCooldown; // Сбрасываем таймер
                }
            }
        }
    }

    private void SpawnObject(MapObjectData data)
    {
        Node[] allNodes = FindObjectsOfType<Node>();
        if (allNodes.Length == 0) return;

        // Ищем все пустые ноды
        List<Node> validNodes = new List<Node>();
        foreach (var node in allNodes)
        {
            if (node != PlayerController.Instance.currentNode && node.currentBuilding == null)
            {
                validNodes.Add(node);
            }
        }

        if (validNodes.Count > 0)
        {
            // Берем случайную ноду
            Node targetNode = validNodes[Random.Range(0, validNodes.Count)];
            Vector3 spawnPos = targetNode.buildingSpawnPoint != null 
                ? targetNode.buildingSpawnPoint.position 
                : targetNode.transform.position + new Vector3(0, 0.5f, 0);

            // Спавним
            GameObject go = Instantiate(data.prefab, spawnPos, Quaternion.identity);
            NodeInteractable interactable = go.GetComponent<NodeInteractable>();

            if (interactable != null)
            {
                interactable.Setup(targetNode);
                data.activeInstances.Add(interactable); // Добавляем в список активных
            }
        }
    }
}