﻿using UnityEngine;

namespace Gameplay.Quests
{
    public class QuestLocationIndicatorComponent : MonoBehaviour
    {
        [SerializeField] private int radius = 16;
        private Transform actorTransform;
        private Transform destinationTransform;

        public void SetActor(GameObject actor)
        {
            actorTransform = actor.transform;
        }

        public void SetDestination(GameObject destination)
        {
            destinationTransform = destination.transform;
        }

        private void Update()
        {
            Vector3 actorPosition = actorTransform.position;
            Vector3 destinationPosition = destinationTransform.position;

            // Set world rotation: point to quest destination
            transform.rotation = Quaternion.LookRotation(destinationPosition - actorPosition);

            // Set world position: on the circle centered at the current actor (player/vehicle)
            float positionOffsetDistance = Mathf.Min(Vector3.Distance(actorPosition, destinationPosition) - 10f, radius);
            transform.position = new Vector3(actorPosition.x, 0.1f, actorPosition.z) + transform.forward * positionOffsetDistance;
        }
    }
}
