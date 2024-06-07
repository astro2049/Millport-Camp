using UnityEngine;

public class DamageDealerComponent : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

    }

    public void DealDamage(GameObject other, float damage)
    {
        // Debug.Log(other.name);
        other.GetComponent<HealthComponent>().changeHealth(-damage);
    }
}
