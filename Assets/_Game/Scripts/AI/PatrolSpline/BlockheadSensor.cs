using UnityEngine;

/// <summary>
/// 
/// </summary>
namespace Hedronoid.AI
{
    public abstract class BlockheadSensor : AIBaseSensor
    {
        [SerializeField]
        [Tooltip("This is a multiplier for the aggro value of the Warrior based on how far he is from the Enemy.")]
        private AnimationCurve m_WarriorAggroRangeModifier;
        [SerializeField]
        [Tooltip("This is a multiplier for the aggro value of the Wizard based on how far he is from the Enemy.")]
        private AnimationCurve m_MageAggroRangeModifier;

        public abstract Transform GetTargetWithinReach(float distance);

        protected float GetModifiedAggroValue(GameObject gameObject)
        {
            // TODO : check this part of code !
            // var aggro = gameObject.GetComponent<Aggro>();
            // if (aggro)
            // {
            //     var curve = (aggro.Character == CharacterType.WARRIOR) ? m_WarriorAggroRangeModifier : m_MageAggroRangeModifier;
            //     var distance = Vector3.Distance(gameObject.transform.position, transform.position);
            //     var finalAggro = aggro.CurrentAggroLevel * curve.Evaluate(distance);
            //     //Debug.Log("Aggro for " + aggro.Character + ": " + finalAggro);
            //     return finalAggro;
            // }
            return 0;
        }
    }
}