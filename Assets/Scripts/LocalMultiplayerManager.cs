using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class LocalMultiplayerManager : MonoBehaviour
{
    [SerializeField] private PlayerInputManager inputManager;

    [SerializeField] private List<string> keyboardControlSchemeNames = new();
    [SerializeField] private List<GameObject> keyboardControlledPlayers = new();

    private void Awake()
    {
        inputManager.onPlayerJoined += OnPlayerJoined;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Read Input Actions asset for Keyboard_P2 bindings
        // For example, "Move" action in Player2 map
        var inputActions = inputManager.playerPrefab.GetComponent<PlayerInput>().actions;
        var player2ControlScheme = inputActions.FindControlScheme("KeyboardRight");
        Debug.Log(player2ControlScheme);

        foreach (var map in inputActions.actionMaps)
        {
            foreach (var action in map.actions)
            {
                // Check if action has any binding in this control scheme
                foreach (var binding in action.bindings)
                {
                    if (binding.groups.Contains(player2ControlScheme.Value.name))
                    {
                        // Get the bound control
                        var control = action.controls.Count > 0 ? action.controls[0] : null;

                        // Check if pressed
                        if (control != null && control.IsPressed())
                        {
                            // Spawn Player2 with this control scheme
                            PlayerInput.Instantiate(
                                inputManager.playerPrefab,
                                playerIndex: 1,
                                controlScheme: player2ControlScheme.Value.name,
                                pairWithDevice: keyboard
                            );
                            return;
                        }
                    }
                }
            }
        }
    }

    private void OnPlayerJoined(PlayerInput player)
    {
        this.AssignControlScheme(player);
    }

    private void AssignControlScheme(PlayerInput player)
    {
        // other devices are auto-assigned control schemes
        if (player.devices.Count != 1 || player.devices[0] is not Keyboard)
        {
            return;
        }

        if (player.playerIndex >= keyboardControlSchemeNames.Count)
        {
            Debug.LogError($"New player's index (#{player.playerIndex}) is out of range!");
            return;
        }

        string keyboardControlSchemeName = keyboardControlSchemeNames[player.playerIndex];
        if (string.IsNullOrWhiteSpace(keyboardControlSchemeName))
        {
            Debug.LogError($"New player's control scheme name is invalid!");
            return;
        }

        player.SwitchCurrentControlScheme(keyboardControlSchemeName, Keyboard.current);
        Debug.Log($"New player (#{player.playerIndex}) was assigned to control scheme '{keyboardControlSchemeName}'");
    }
}
