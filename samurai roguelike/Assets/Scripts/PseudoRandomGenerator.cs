using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PseudoRandomGenerator : MonoBehaviour
{
    public enum Seed {random, number, date};
    public Seed seed;

    public long seedLocal = 333;
    int a = 1993;
    int c;
    float mod = 10;

    private void OnEnable()
    {
        c = 8 * a + 3;
        mod = Mathf.Pow(2, mod);

        switch (seed)
        {
            case Seed.random:
                break;
            case Seed.number:
                break;
            case Seed.date:
                seedLocal = GetDateTicks();
                break;
            default:
                break;
        }

        if((seedLocal % 2) == 0)
        {
            seedLocal += (seedLocal / 2);
        }
        NumberSingleton.instance.SetNum(seedLocal);
    }

    public int GetNumber(int _maxNum)
    {
        if (_maxNum == 0){
            _maxNum = 1;
        }
        switch (seed)
        {
            case Seed.random:
                return UnityEngine.Random.Range(0, _maxNum);

            case Seed.number:
                //Debug.Log("1993 * " + NumberSingleton.instance.num + " + 6 = " + (1993 * NumberSingleton.instance.num + 6) + " % " + _maxNum + " = " + (1993 * NumberSingleton.instance.num + 6) % _maxNum);
                MetodoCongruencialMixto();
                
                break;

            case Seed.date:
                //Debug.Log("1993 * " + NumberSingleton.instance.num + " + 1997 = " + (1993 * NumberSingleton.instance.num + 1997) + " % 100 = " + (1993 * NumberSingleton.instance.num + 1997) % 100);
                MetodoCongruencialMixto();
                break;

            default:
                break;
        }

        //Debug.Log("numero:" + NumberSingleton.instance.num + " ||numero Maximo: " + (_maxNum - 1));
        return (int)(NumberSingleton.instance.num % _maxNum);
    }

    int GetDateTicks()
    {
        int i;
        DateTime todayDate = DateTime.Now;
        string dateString = todayDate.ToString("ddMMyyyy");
        int dateNum = int.Parse(dateString);
        //Debug.Log("numero: " + dateNum);
        return dateNum;
    }

    void MetodoCongruencialMixto ()
    {
        NumberSingleton.instance.num = (long) ((a * NumberSingleton.instance.num + c) % mod);
    }
}
