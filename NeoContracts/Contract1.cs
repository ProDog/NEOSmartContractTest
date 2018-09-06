using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;

namespace NeoContracts
{
    public class Contract1 : SmartContract
    {
        public static object Main(string method, object[] args)
        {
            //return "Hello world!";
            var magicstr = "NEL";

            if (Runtime.Trigger == TriggerType.Verification)//取钱才会涉及这里
            {
                return true;
            }

            else if (Runtime.Trigger == TriggerType.VerificationR)//取钱才会涉及这里
            {
                return true;
            }
            else if (Runtime.Trigger == TriggerType.Application)
            {
                if (method == "put")
                {
                    //Storage.Put(Storage.CurrentContext, "put", "1");
                    return "Hello , Put()";
                }
                if (method == "get")
                {
                    //return Storage.Get(Storage.CurrentContext, "put");
                    return "Hello , Get()";
                }
                if (method == "test")
                {
                    //return Storage.Get(Storage.CurrentContext, "put");
                    return 1;
                }
                if (method == "bool")
                {
                    //return Storage.Get(Storage.CurrentContext, "put");
                    return false;
                }
            }
            return false;
        }

        
    }
}
