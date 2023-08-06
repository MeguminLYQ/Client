using System;
using UnityEngine;
using UnityEngine.InputSystem;
using WeCraft.Core.C2S;

namespace Script
{
    [DefaultExecutionOrder(-100)]
    public class GameManager: MonoBehaviour
    {
        public PlayerInput PlayerInput;
        public static GameManager Instance; 
        [NonSerialized]
        public WeCraftClient Client;
        public C2S_PlayerProfile PlayerProfile;
        private void Awake()
        {
            Instance = this; 
            PlayerInput = PlayerInput ?? GameObject.FindObjectOfType<PlayerInput>();
            Client = WeCraftClient.GetInstance();
        }

        private void OnEnable()
        {
            Client.Start();
        }

        private void OnDisable()
        {
            Client.Stop();
        }
    }
}