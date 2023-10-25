using System.Collections;
using System.Collections.Generic;
using Angel;
using UnityEngine;

public class powerUpController : MonoBehaviour
{
    private SpriteRenderer _sr;
    public int PowerUpIndex { get; private set; }


    // Start is called before the first frame update
    void Start()
    {
         _sr = GetComponent<SpriteRenderer>();
        PowerUpIndex = SetRandomPowerUp();
    }

    // Update is called once per frame
    private int SetRandomPowerUp()
    {
        int randomInt = Random.Range(0, 4);
        
        switch (randomInt)
        {
            case 0:  //Fire PowerUp
                _sr.color = Color.red;
                break;
            case 1:  //Electric PowerUp
                _sr.color = Color.yellow;
                break;
            case 2:  //Wind PowerUp
                _sr.color = Color.green;
                break;
            case 3:  //Ice PowerUp
                _sr.color = Color.cyan;
                break;
            default:
                Destroy(gameObject);
                break;
        }

        return randomInt;
    }
}
