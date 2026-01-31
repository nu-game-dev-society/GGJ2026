using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiplayerManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private int respawnTime = 5;
    [SerializeField] private Transform[] spawnLocations;

    private PlayerInputManager inputManager;
    private bool keyboardLeftJoined = false;
    private bool keyboardRightJoined = false;
    private HashSet<Gamepad> joinedGamepads = new HashSet<Gamepad>();

    private void Start()
    {
        inputManager = GetComponent<PlayerInputManager>();
        if (inputManager != null)
        {
            // Disable automatic joining - we'll handle it manually
            inputManager.DisableJoining();
        }
    }

    private void Update()
    {
        // Check for KeyboardLeft (WASD) join
        if (!keyboardLeftJoined && Keyboard.current != null &&
            (Keyboard.current.wKey.wasPressedThisFrame ||
            Keyboard.current.aKey.wasPressedThisFrame ||
            Keyboard.current.sKey.wasPressedThisFrame ||
            Keyboard.current.dKey.wasPressedThisFrame))
        {
            JoinPlayer("KeyboardLeft", Keyboard.current);
            keyboardLeftJoined = true;
        }

        // Check for KeyboardRight (IJKL) join
        if (!keyboardRightJoined && Keyboard.current != null &&
            (Keyboard.current.iKey.wasPressedThisFrame ||
            Keyboard.current.jKey.wasPressedThisFrame ||
            Keyboard.current.kKey.wasPressedThisFrame ||
            Keyboard.current.lKey.wasPressedThisFrame))
        {
            JoinPlayer("KeyboardRight", Keyboard.current);
            keyboardRightJoined = true;
        }

        // Check for Gamepad join
        foreach (var gamepad in Gamepad.all)
        {
            if (!joinedGamepads.Contains(gamepad))
            {
                // Check if any button was pressed on this gamepad
                if (gamepad.buttonSouth.wasPressedThisFrame ||
                    gamepad.buttonNorth.wasPressedThisFrame ||
                    gamepad.buttonEast.wasPressedThisFrame ||
                    gamepad.buttonWest.wasPressedThisFrame ||
                    gamepad.startButton.wasPressedThisFrame ||
                    gamepad.leftStick.ReadValue().magnitude > 0.1f)
                {
                    JoinPlayer("Gamepad", gamepad);
                    joinedGamepads.Add(gamepad);
                }
            }
        }
    }

    private void JoinPlayer(string controlScheme, InputDevice device)
    {
        if (playerPrefab != null)
        {
            Vector3 spawnPosition = findBestSpawnLocation();

            // Instantiate the player GameObject
            GameObject playerObj = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

            // Get the PlayerInput component
            var playerInput = playerObj.GetComponent<PlayerInput>();

            if (playerInput != null)
            {
                // Name the player based on index
                playerObj.name = $"Player{playerInput.playerIndex}";

                // Set color
                PlayerController controller = playerObj.GetComponent<PlayerController>();
                controller.playerColor = Color.HSVToRGB((playerInput.playerIndex * 0.618034f) % 1f, 1f, 1f);
                controller.OnPlayerKilled.AddListener(() =>
                {
                    StartCoroutine(RespawnPlayer(playerObj, controlScheme, device));
                });

                // Switch to the correct control scheme and pair with device
                playerInput.SwitchCurrentControlScheme(controlScheme, device);

                // Enable the input actions
                if (playerInput.actions != null)
                {
                    playerInput.actions.Enable();
                }

                // Activate the player input
                playerInput.ActivateInput();

                OnPlayerJoined(playerInput);
            }
        }
    }

    private IEnumerator RespawnPlayer(GameObject playerObj, string controlScheme, InputDevice device)
    {
        yield return new WaitForSeconds(respawnTime);

        var playerInput = playerObj.GetComponent<PlayerInput>();

        playerObj.transform.position = findBestSpawnLocation();
        playerObj.SetActive(true);

        // Re-enable input
        playerInput.SwitchCurrentControlScheme(controlScheme, device);
        playerInput.actions.Enable();
        playerInput.ActivateInput();
    }

    // Called when a player joins
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        Debug.Log($"Player {playerInput.playerIndex} joined with {playerInput.currentControlScheme}");

        // Debug: Print active actions
        Debug.Log($"Actions enabled: {playerInput.actions.enabled}, Current action map: {playerInput.currentActionMap?.name}");
    }

    private Vector3 findBestSpawnLocation()
    {
        // Safety check
        if (spawnLocations == null || spawnLocations.Length == 0)
        {
            Debug.LogError("No spawn locations defined!");
            return Vector3.zero;
        }

        // If no active players exist, use the first spawn
        var activePlayers = PlayerInput.all
            .Where(p => p != null && p.gameObject != null && p.gameObject.activeSelf)
            .ToList();

        if (activePlayers.Count == 0)
        {
            Debug.Log("First player spawning at first location");
            return spawnLocations[0].position;
        }

        float maxDistance = -1f;
        Vector3 bestLocation = spawnLocations[0].position; // Start with first spawn as default

        foreach (var spawn in spawnLocations)
        {
            float distanceToClosestPlayer = float.MaxValue;

            foreach (var player in activePlayers)
            {
                float dist = Vector3.Distance(spawn.position, player.transform.position);
                if (dist < distanceToClosestPlayer)
                {
                    distanceToClosestPlayer = dist;
                }
            }

            if (distanceToClosestPlayer > maxDistance)
            {
                maxDistance = distanceToClosestPlayer;
                bestLocation = spawn.position;
            }
        }

        return bestLocation;
    }
}
