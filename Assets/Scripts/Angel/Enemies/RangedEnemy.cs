using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class RangedEnemy : MonoBehaviour
{
    [SerializeField] private GameObject projectile;
    [SerializeField] private CapsuleCollider2D col;
    private GameObject _player;

    [SerializeField] private float range;
    private LayerMask _playerMask;
    private float _cooldownTimer = Mathf.Infinity;
    [SerializeField] private float attackCooldown;
    
    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _playerMask = LayerMask.GetMask("Player");
    }

    // Update is called once per frame
    void Update()
    {
        _cooldownTimer += Time.deltaTime;

        if (PlayerInSight())
        {
            if (_cooldownTimer >= attackCooldown)
            {
                _cooldownTimer = 0;
                Shoot();
            }
        }
    }

    void Shoot()
    {
        //set up target and change rotation to look directly at it.
        Vector3 position = _player.transform.position - transform.position;
        float z = Mathf.Atan2(position.x, position.y) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0,0, -z);
        Instantiate(projectile, transform.position, rotation);
    }

    bool PlayerInSight()
    {
        RaycastHit2D hit = Physics2D.BoxCast(col.bounds.center, new Vector2(col.bounds.size.x * range, col.bounds.size.y), 0, Vector2.left, 0, _playerMask);
        return hit;
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.DrawWireCube(col.bounds.center, new Vector2(col.bounds.size.x * range, col.bounds.size.y));
    // }
}
