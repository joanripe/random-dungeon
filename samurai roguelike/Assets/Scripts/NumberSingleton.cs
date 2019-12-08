using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberSingleton : MonoBehaviour
{
    public static NumberSingleton instance;

    bool numInicialiced = false;
    public long num;

    void Awake()
    {

        if (instance == null)
        {

            instance = this;
            DontDestroyOnLoad(this.gameObject);

            //Rest of your Awake code

        }
        else
        {
            Destroy(this);
        }
    }

    public void SetNum (long i)
    {
        if (!numInicialiced)
        {
            Debug.Log("seteando a " + i);
            num = i;
            numInicialiced = true;
        } else
        {
            Debug.Log("sumando 1");
            //num++;
        }
    }

    //Rest of your class code

}
