using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;

namespace NeoContracts
{
    public class Contract1 : SmartContract
    {
        public static void Main()
        {
            Storage.Put(Storage.CurrentContext, "Hello", "World");
            //var test = "hello world!";
            //return test;
        }

        //public static object Main(string param, int[] value)
        //{
        //    var magicstr = "2018 02 21";
        //    return value[0] + value[1];
        //}
    }
}
