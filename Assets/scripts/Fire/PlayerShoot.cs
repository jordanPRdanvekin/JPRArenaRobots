using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    PoolBala bulletsPool;

    [SerializeField] Vector3 rightGunSpawn;
    [SerializeField] Vector3 leftGunSpawn;

    bool usingRightGun = true;
    Vector3 wantedGun;

    bool canShoot = true;
    float shootCooldown = 2f;

    [SerializeField] float areaRadius;
    [SerializeField] LayerMask enemiesLayer;

    GameObject nearestEnemy;

    //Debug
    [SerializeField] GameObject transformTemp;

    private void Start()
    {
        bulletsPool = GetComponent<PoolBala>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && canShoot && energySystem.instance.HasEnoughEnergy())
        {
            energySystem.instance.ConsumeShotEnergy();
            //ScoreManagerBehaviour.instance.ReduceScore();
            Shoot();
            canShoot = false;
            StartCoroutine(ShootCooldown());
        }
    }

    IEnumerator ShootCooldown()
    {
        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
        yield return null;
    }

    void Shoot()
    {
        GameObject bullet = bulletsPool.GetElementFromPool();
        bullet.transform.rotation = gameObject.transform.rotation;
        wantedGun = usingRightGun ? rightGunSpawn : leftGunSpawn;
        usingRightGun = !usingRightGun;
        bullet.transform.position = transform.position + transform.up * wantedGun.y + transform.forward * wantedGun.z + transform.right * wantedGun.x;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, areaRadius, enemiesLayer);

        GameObject searchEnemy = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider hitCollider in hitColliders)
        {
            float distance = Vector3.Distance(transform.position, hitCollider.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                searchEnemy = hitCollider.gameObject;
            }
        }
        
        bullet.GetComponentInChildren<Mira>().target = searchEnemy;

        bullet.SetActive(true);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position + transform.up * rightGunSpawn.y + transform.forward * rightGunSpawn.z + transform.right * rightGunSpawn.x, new Vector3(0.1f, 0.1f, 0.1f));
        Gizmos.color = Color.green;
        Gizmos.DrawCube(transform.position + transform.up * leftGunSpawn.y + transform.forward * leftGunSpawn.z + transform.right * leftGunSpawn.x, new Vector3(0.1f, 0.1f, 0.1f));
        Gizmos.DrawSphere(transform.position, areaRadius);
    }
}
