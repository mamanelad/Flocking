using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Avrahamy;
using Avrahamy.Math;
using BitStrap;

namespace Flocking
{
    public class FollowerPeepController : MonoBehaviour
    {
        enum FollowerKind
        {
            Crazy, // Just walk crazy on the field.
            Suicide, // Will go toward danger
            Runner, // Run away from danger
            Friendly, // Just go with his friends around him 
            Annoying // Will find the player and annoy him.
        }


        #region Private Fields

        private GameObject player;
        private FollowerKind followerKind;
        private Collider curColliderHit;
        private int hits; // Save the amount of colliders stored into the results buffer of the Sphere.
        private Vector3 avgDirectionMain;
        private Vector3 avgDirectionSeparation;
        private Vector3 avgDirectionAlignment;
        private Vector3 avgDirectionCohesion;

        private readonly List<FollowerKind> followerKinds = new List<FollowerKind>();
        private static readonly Collider[] ColliderResults = new Collider[10];

        #endregion

        #region Inspector Control

        [Header("Regular Flocking Settings")] 
        
        [SerializeField] PeepController peep;
        [SerializeField] float senseRadius = 2f;
        [SerializeField] PassiveTimer navigationTimer;
        [SerializeField] LayerMask navigationMask;
        [TagSelector] [SerializeField] string peepTag;
        [SerializeField] bool repelFromSameGroup;
        [SerializeField] private float alignmentFactor = 1f;

        [Header("Special Flocking Settings")]
        [SerializeField] private float crazyRange = 100f;
        [SerializeField] private float suicideFactor = 0.2f;
        [SerializeField] private float runnerFactor = 1f;
        [SerializeField] private float runnerMinDistance = 6f;
        [SerializeField] private float annoyingFactor = 0.8f;
        [SerializeField] private float annoyingMinDistance = 4f;

        #endregion


        protected void Reset()
        {
            if (peep == null)
                peep = GetComponent<PeepController>();
        }

        private void Start()
        {
            InitializeKinds();
            player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                print("no game object with tag player!!!");
            }
        }

        private void InitializeKinds()
        {
            followerKinds.Add(FollowerKind.Crazy);
            followerKinds.Add(FollowerKind.Suicide);
            followerKinds.Add(FollowerKind.Runner);
            followerKinds.Add(FollowerKind.Friendly);
            followerKinds.Add(FollowerKind.Annoying);
            var index = Random.Range(0, followerKinds.Count);

            followerKind = followerKinds[index];
        }

        protected void OnEnable()
        {
            navigationTimer.Start();
            peep.DesiredVelocity = Random.insideUnitCircle.normalized;
        }


        private void Separation()
        {
            var position = peep.Position;
            // Always repel from walls.
            var repel = true;
            if (curColliderHit.CompareTag(peepTag))
            {
                // Sensed another peep.
                var otherPeed = curColliderHit.attachedRigidbody.GetComponent<PeepController>();
                // Ignore peeps that are not from this group.
                // if (otherPeed.Group != peep.Group) return;
                repel = repelFromSameGroup;
            }

            var closestPoint = curColliderHit.ClosestPoint(position);
            closestPoint.y = 0f;
            DebugDraw.DrawLine(
                position + Vector3.up,
                closestPoint + Vector3.up,
                Color.cyan,
                navigationTimer.Duration / 2);
            var direction = closestPoint - position;

            var magnitude = direction.magnitude;
            var distancePercent = repel
                ? Mathf.InverseLerp(peep.SelfRadius, senseRadius, magnitude)
                // Inverse attraction factor so peeps won't be magnetized to
                // each other without being able to separate.
                : Mathf.InverseLerp(senseRadius, peep.SelfRadius, magnitude);

            // Make sure the distance % is not 0 to avoid division by 0.
            distancePercent = Mathf.Max(distancePercent, 0.01f);

            // Force is stronger when distance percent is closer to 0 (1/x-1).
            var forceWeight = 1f / distancePercent - 1f;

            // Angle between forward to other collider.
            var angle = transform.forward.GetAngleBetweenXZ(direction);
            var absAngle = Mathf.Abs(angle);
            if (absAngle > 90f)
            {
                // Decrease weight of colliders that are behind the peep.
                // The closer to the back, the lower the weight.
                var t = Mathf.InverseLerp(180f, 90f, absAngle);
                forceWeight *= Mathf.Lerp(0.1f, 1f, t);
            }

            direction = direction.normalized * forceWeight;
            if (repel)
            {
                avgDirectionSeparation -= direction;
                DebugDraw.DrawArrowXZ(
                    position + Vector3.up,
                    -direction * 3f,
                    1f,
                    30f,
                    Color.magenta,
                    navigationTimer.Duration / 2);
            }
            else
            {
                avgDirectionSeparation += direction;
                DebugDraw.DrawArrowXZ(
                    position + Vector3.up,
                    direction * 3f,
                    1f,
                    30f,
                    Color.green,
                    navigationTimer.Duration / 2);
            }
        }

        private void Alignment()
        {
            if (curColliderHit.CompareTag(peepTag))
            {
                // Sensed another peep.
                var otherPeed = curColliderHit.attachedRigidbody.GetComponent<PeepController>();
                avgDirectionAlignment += (Vector3) otherPeed.Velocity;
            }
        }

        private void Cohesion()
        {
            if (curColliderHit.CompareTag(peepTag))
            {
                avgDirectionCohesion += curColliderHit.transform.position;
            }
        }

        private void FinalDirection()
        {
            //Final calculation for the Alignment vector.
            avgDirectionAlignment = new Vector3(avgDirectionAlignment.x * alignmentFactor,
                avgDirectionAlignment.y * alignmentFactor, avgDirectionAlignment.z * alignmentFactor);

            //Final calculation for the Cohesion vector.
            avgDirectionCohesion /= hits;
            avgDirectionCohesion = (avgDirectionCohesion - transform.position);


            avgDirectionMain = (avgDirectionSeparation + avgDirectionAlignment + avgDirectionCohesion).normalized;
        }

        private void SpecialAddDirection()
        {
            var target = GameObject.FindWithTag("Target");
            annoyingMinDistance += Time.deltaTime;
            runnerMinDistance -= Time.deltaTime / 2;
            switch (followerKind)
            {
                case FollowerKind.Suicide:
                    if (target != null)
                    {
                        var dir = (target.transform.position - transform.position);
                        dir = new Vector3(dir.x * suicideFactor, 0, dir.z * suicideFactor);
                        avgDirectionMain += dir;
                    }

                    break;

                case FollowerKind.Annoying:
                    if (player != null)
                    {
                        var dist = Vector3.Distance(player.transform.position, transform.position);
                        if (dist <= annoyingMinDistance)
                        {
                            var dir = (player.transform.position - transform.position);
                            dir = new Vector3(dir.x * annoyingFactor, 0, dir.z * annoyingFactor);
                            avgDirectionMain += dir;
                        }
                    }

                    break;


                case FollowerKind.Crazy:
                    var x = Random.Range(-crazyRange, crazyRange);
                    var z = Random.Range(-crazyRange, crazyRange);
                    avgDirectionMain += new Vector3(x, 0, z);
                    break;

                case FollowerKind.Runner:
                    if (target != null)
                    {
                        var dist = Vector3.Distance(target.transform.position, transform.position);
                        if (dist <= runnerMinDistance)
                        {
                            var dir = (target.transform.position - transform.position);
                            dir = new Vector3(dir.x * runnerFactor, 0, dir.z * runnerFactor);
                            avgDirectionMain = -dir;
                        }
                    }

                    break;

                case FollowerKind.Friendly:
                    avgDirectionMain += avgDirectionCohesion + avgDirectionAlignment;
                    break;
            }
        }

        protected void Update()
        {
            if (navigationTimer.IsActive) return;
            navigationTimer.Start();

            var position = peep.Position;

            // Check for colliders in the sense radius.
            hits = Physics.OverlapSphereNonAlloc(
                position,
                senseRadius,
                ColliderResults,
                navigationMask.value);

            // There will always be at least one hit on our own collider.
            if (hits <= 1) return;


            avgDirectionMain = Vector3.zero;
            avgDirectionSeparation = Vector3.zero;
            avgDirectionAlignment = Vector3.zero;
            avgDirectionCohesion = Vector3.zero;

            for (var i = 0; i < hits; i++)
            {
                curColliderHit = ColliderResults[i];
                // Ignore self.
                if (curColliderHit.attachedRigidbody != null &&
                    curColliderHit.attachedRigidbody.gameObject == peep.gameObject) continue;

                Separation();
                Alignment();
                Cohesion();
            }


            FinalDirection();
            SpecialAddDirection();

            if (avgDirectionMain.sqrMagnitude < 0.1f) return;

            peep.DesiredVelocity = avgDirectionMain.normalized.ToVector2XZ();
        }

        protected void OnDrawGizmos()
        {
            var angle = transform.forward.GetAngleXZ();
            DebugDraw.GizmosDrawSector(
                transform.position,
                senseRadius,
                180f + angle,
                -180f + angle,
                Color.green);
        }
    }
}