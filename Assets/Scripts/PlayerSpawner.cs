using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField]
    private Transform _entrySpawn; // Where player spawns when coming from previous scene (going forward)

    [SerializeField]
    private Transform _exitSpawn;  // Where player spawns when coming from next scene (going backward)

    [Header("Player")]
    [SerializeField]
    private GameObject _player;

    private void Start()
    {
        // Get which spawn point to use
        string spawnPoint = PlayerPrefs.GetString("SpawnPoint", "Entry");
        Debug.Log("[PlayerSpawner] Spawning at: " + spawnPoint);

        Transform targetSpawn = null;

        if (spawnPoint == "Entry")
        {
            targetSpawn = _entrySpawn;
        }
        else if (spawnPoint == "Exit")
        {
            targetSpawn = _exitSpawn;
        }

        // Move player to spawn point
        if (targetSpawn != null && _player != null)
        {
            _player.transform.position = targetSpawn.position;
            _player.transform.rotation = targetSpawn.rotation;
            Debug.Log("Player moved to: " + targetSpawn.position);

            // Clear the saved spawn point only after successful spawn
            PlayerPrefs.DeleteKey("SpawnPoint");
        }
        else
        {
            Debug.LogWarning("No target spawner");
        }
    }
}