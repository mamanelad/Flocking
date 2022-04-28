using UnityEngine;
using Random = UnityEngine.Random;
using Avrahamy;
using Avrahamy.Math;
using BitStrap;

namespace Flocking
{
    public class FollowerPeepController : MonoBehaviour
    {
        [SerializeField] PeepController peep;
        [SerializeField] float senseRadius = 2f;
        [SerializeField] PassiveTimer navigationTimer;
        [SerializeField] LayerMask navigationMask;
        [TagSelector] [SerializeField] string peepTag;
        [SerializeField] bool repelFromSameGroup;


        private Collider curColliderHit;
        private int hits; // Save the amount of colliders stored into the results buffer of the Sphere.

        [SerializeField] private float alignmentFactor = 1f;  
        
        private Vector3 avgDirectionMain;
        private Vector3 avgDirectionSeparation;
        private Vector3 avgDirectionAlignment;
        private Vector3 avgDirectionCohesion;


        private static readonly Collider[] COLLIDER_RESULTS = new Collider[10];

        protected void Reset()
        {
            if (peep == null)
            {
                peep = GetComponent<PeepController>();
            }
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
                if (otherPeed.Group != peep.Group) return;
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
                // Ignore peeps that are not from this group.
                if (otherPeed.Group != peep.Group) return;

                //option 1 for alignment
                // avgDirectionAlignment += otherPeed.GetForwardVelocity();
                
                //option 2 for alignment
                avgDirectionAlignment += (Vector3) otherPeed.Velocity;
            }
        }

        private void Cohesion()
        {
            if (curColliderHit.CompareTag(peepTag))
            {
                var otherPeed = curColliderHit.attachedRigidbody.GetComponent<PeepController>();
                if (otherPeed.Group != peep.Group) return;
                avgDirectionCohesion += curColliderHit.transform.position;
            }
        }

        private void FinalDirection()
        {
            //Final calculation for the Alignment vector.
            //option 1 for alignment
            // avgDirectionAlignment.x /= hits;
            // avgDirectionAlignment.y /= hits;
            
            //If we want to normalize them. 
            // avgDirectionSeparation.Normalize();
            // avgDirectionAlignment.Normalize();
            // avgDirectionCohesion.Normalize();
            
            avgDirectionAlignment = new Vector3(avgDirectionAlignment.x * alignmentFactor,
                avgDirectionAlignment.y * alignmentFactor, avgDirectionAlignment.z * alignmentFactor);

            //Final calculation for the Cohesion vector.
            avgDirectionCohesion /= hits;
            avgDirectionCohesion = (avgDirectionCohesion - transform.position);
            
            
            avgDirectionMain = avgDirectionSeparation + avgDirectionAlignment + avgDirectionCohesion;
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
                COLLIDER_RESULTS,
                navigationMask.value);

            // There will always be at least one hit on our own collider.
            if (hits <= 1) return;


            avgDirectionMain = Vector3.zero;
            avgDirectionSeparation = Vector3.zero;
            avgDirectionAlignment = Vector3.zero;
            avgDirectionCohesion = Vector3.zero;

            for (int i = 0; i < hits; i++)
            {
                curColliderHit = COLLIDER_RESULTS[i];
                // Ignore self.
                if (curColliderHit.attachedRigidbody != null &&
                    curColliderHit.attachedRigidbody.gameObject == peep.gameObject) continue;

                Separation();
                Alignment();
                Cohesion();
            }


            
            
            FinalDirection();
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





