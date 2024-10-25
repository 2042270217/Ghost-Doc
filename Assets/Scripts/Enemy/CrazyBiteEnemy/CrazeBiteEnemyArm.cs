﻿using UnityEngine;

public class CrazeBiteEnemyArm : MonoBehaviour
{
    private Enemy enemy;
    private float damage;

    private void Awake()
    {
        enemy = transform.root.GetComponent<Enemy>();
        damage = enemy.blackboard.damage;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.root.GetComponent<Player>().TakeDamage(damage);
        }
    }
}