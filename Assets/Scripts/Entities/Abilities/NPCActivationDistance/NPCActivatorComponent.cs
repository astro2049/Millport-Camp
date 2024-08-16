﻿using UnityEngine;

namespace Entities.Abilities.NPCActivationDistance
{
    public class NPCActivatorComponent : MonoBehaviour
    {
        [SerializeField] private GameObject pawn;

        private void Awake()
        {
            Deactivate();
        }

        public void Activate()
        {
            pawn.SetActive(true);
        }

        public void Deactivate()
        {
            pawn.SetActive(false);
        }
    }
}