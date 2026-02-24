using UnityEngine;

/// <summary>
/// Responsive movement based on "Building Better Movement" by Nishchal Bhandari
/// Uses normalized curve functions for attack/release instead of linear acceleration
/// </summary>
public class ResponsiveMovementHandler : IMovementHandler
{
    private readonly float _maxSpeed;
    private readonly float _tAttack;  // Time to reach max speed
    private readonly float _tRelease; // Time to stop from max speed

    public ResponsiveMovementHandler(float maxSpeed, float tAttack, float tRelease)
    {
        _maxSpeed = maxSpeed;
        _tAttack = tAttack;
        _tRelease = tRelease;
    }

    public void ApplyMovement(Vector3 moveDir, float targetSpeed, ref Vector3 currentVelocity)
    {
        float currentHorizontalSpeed = new Vector3(currentVelocity.x, 0, currentVelocity.z).magnitude;

        // Normalize speed to [0, 1]
        float normalizedSpeed = currentHorizontalSpeed / _maxSpeed;
        
        // Clamp to prevent NaN
        normalizedSpeed = Mathf.Clamp01(normalizedSpeed);

        // Calculate current t from current speed using inverse attack equation
        // Attack: v = 1 - (1-t)^2
        // Inverse: t = 1 - sqrt(1 - v)
        float t = 1f - Mathf.Sqrt(Mathf.Max(0f, 1f - normalizedSpeed));

        // Move forward in time
        float tNew = t + (Time.deltaTime / _tAttack);

        float newNormalizedSpeed;
        if (tNew >= 1f)
        {
            newNormalizedSpeed = 1f;
        }
        else
        {
            // Attack curve: v = 1 - (1-t)^2
            float oneMinusT = 1f - tNew;
            newNormalizedSpeed = 1f - (oneMinusT * oneMinusT);
        }

        float newSpeed = newNormalizedSpeed * _maxSpeed;
        Vector3 newVel = moveDir * newSpeed;
        currentVelocity.x = newVel.x;
        currentVelocity.z = newVel.z;
    }

    public void ApplyAirControl(Vector3 moveDir, ref Vector3 currentVelocity)
    {
        // Same as ground but with modified time constant
        float currentHorizontalSpeed = new Vector3(currentVelocity.x, 0, currentVelocity.z).magnitude;
        float normalizedSpeed = Mathf.Clamp01(currentHorizontalSpeed / _maxSpeed);

        float t = 1f - Mathf.Sqrt(Mathf.Max(0f, 1f - normalizedSpeed));
        float tNew = t + (Time.deltaTime / (_tAttack * 2f)); // Slower in air

        float newNormalizedSpeed;
        if (tNew >= 1f)
        {
            newNormalizedSpeed = 1f;
        }
        else
        {
            float oneMinusT = 1f - tNew;
            newNormalizedSpeed = 1f - (oneMinusT * oneMinusT);
        }

        float newSpeed = newNormalizedSpeed * _maxSpeed;
        Vector3 newVel = moveDir * newSpeed;
        currentVelocity.x = newVel.x;
        currentVelocity.z = newVel.z;
    }
}
