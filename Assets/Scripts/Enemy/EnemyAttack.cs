using UnityEngine;

public class EnemyAttack : MonoBehaviour, IAttack
{
    private Damageable _damageable;

    private void Awake()
    {
        _damageable = GetComponent<Damageable>();
    }

    #region Normal Attack
    public void Detection_NA(GameObject _gameObject)
    {
        if (_damageable == null) return;

        _damageable.CauseDMG(_gameObject, AttackType.NormalAttack);
    }
    #endregion


    #region Charged Attack
    public void Detection_CA(GameObject _gameObject)
    {
        if (_damageable == null) return;

        _damageable.CauseDMG(_gameObject, AttackType.ChargedAttack);
    }
    #endregion


    #region Elemental Skill
    public void Detection_E(GameObject _gameObject)
    {
        if (_damageable == null) return;

        _damageable.CauseDMG(_gameObject, AttackType.ElementalSkill);
    }
    #endregion


    #region Elemental Burst
    public void Detection_Q(GameObject _gameObject)
    {
        if (_damageable == null) return;

        _damageable.CauseDMG(_gameObject, AttackType.ElementalBurst);
    }
    #endregion
}