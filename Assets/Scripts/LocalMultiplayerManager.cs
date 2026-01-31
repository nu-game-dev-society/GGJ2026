using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using System.Linq;
using System;

public class InputControlObserver : IObserver<InputControl>
{
    private readonly Action<InputControl> _onNext;

    public InputControlObserver(Action<InputControl> onNext)
    {
        _onNext = onNext;
    }

    public void OnCompleted() { }
    public void OnError(Exception error) { }
    public void OnNext(InputControl value)
    {
        _onNext?.Invoke(value);
    }
}

public class LocalMultiplayerManager : MonoBehaviour
{
    [SerializeField] private PlayerInputManager inputManager;

    //1st should be the default
    [SerializeField] private List<string> keyboardControlSchemeNames = new();
    [SerializeField] private List<GameObject> keyboardControlledPlayers = new();

    private IReadOnlyList<InputControlScheme> keyboardControlSchemes;
    private InputActionAsset inputActionAsset;

    private void Awake()
    {
        this.inputActionAsset = inputManager.playerPrefab.GetComponent<PlayerInput>().actions;
        this.inputManager.onPlayerJoined += OnPlayerJoined;

        this.keyboardControlSchemes = keyboardControlSchemeNames
            .Select(name => this.inputActionAsset.FindControlScheme(name))
            .Where(scheme => scheme.HasValue)
            .Select(scheme => scheme.Value)
            .ToArray();
    }

    private void OnEnable()
    {
        StartListeningForButtonPressFromAlternateKeyboardControlSchemes();
    }

    private void StartListeningForButtonPressFromAlternateKeyboardControlSchemes()
    {
        InputSystem.onAnyButtonPress.Subscribe(new InputControlObserver((control) =>
        {
            Debug.Log("Got input");

            // Only care about keyboards
            if (control.device is not Keyboard keyboard) {
                Debug.Log("Not keyboard " + control.device);
                return;
            }

            foreach (var scheme in inputActionAsset.controlSchemes)
            {
                foreach (var dev in scheme.deviceRequirements)
                {
                    if (dev == control.device) 
                    {
                        Debug.Log($"Control scheme: {scheme.name}");
                        break;
                    }
                }
            }
            
            // Try joining a new player for this keyboard
            foreach (var scheme in keyboardControlSchemes.Skip(1)) // skip default
            {
                PlayerInput.Instantiate(
                    inputManager.playerPrefab,
                    controlScheme: scheme.name,
                    pairWithDevice: control.device
                );

                Debug.Log($"Joined new player with scheme '{scheme.name}' and device '{keyboard.displayName}'");
                break;
            }
        }));
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
