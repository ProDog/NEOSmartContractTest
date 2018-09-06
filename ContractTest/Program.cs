using System;
using System.Collections.Generic;
using ThinNeo;

namespace ContractTest
{
    class Program
    {
        public const string api = "https://api.nel.group/api/testnet";
        public const string id_GAS = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";
        static void Main(string[] args)
        {
            Console.WriteLine("wif:");
            Test_Deploy();
            Console.ReadKey();
        }

        private static void Test_Deploy()
        {
            var wif = Console.ReadLine();
            var prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
            var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            var address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            var scriptHash = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);

            byte[] script;
            using (var sb = new ThinNeo.ScriptBuilder())
            {
                var array = new MyJson.JsonNode_Array();
                array.AddArrayValue("(int)1");
                sb.EmitParamJson(array);//参数倒序入
                sb.EmitPushString("deploy");//参数倒序入
                sb.EmitAppCall(new Hash160("0xa0b53d2efa8b1c4a62fcc1fcb54b7641510810c7"));//nep5脚本
                script = sb.ToArray();
                Console.WriteLine(ThinNeo.Helper.Bytes2HexString(script));
            }
            var result = SendTransaction(prikey, script);
            Console.WriteLine(result);
        }

        private static object SendTransaction(byte[] prikey, byte[] script)
        {
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            //获取地址的资产列表
            Dictionary<string, List<Utxo>> dir = Helper.GetBalanceByAddress(api, address);
            if (dir.ContainsKey(id_GAS) == false)
            {
                Console.WriteLine("no gas");
                return null;
            }
            //MakeTran
            ThinNeo.Transaction tran = null;
            {

                byte[] data = script;
                tran = Helper.makeTran(dir[id_GAS], null, new ThinNeo.Hash256(id_GAS), 0);
                tran.type = ThinNeo.TransactionType.InvocationTransaction;
                var idata = new ThinNeo.InvokeTransData();
                tran.extdata = idata;
                idata.script = data;
                idata.gas = 0;
            }

            //sign and broadcast
            var signdata = ThinNeo.Helper.Sign(tran.GetMessage(), prikey);
            tran.AddWitness(signdata, pubkey, address);
            var trandata = tran.GetRawData();
            var strtrandata = ThinNeo.Helper.Bytes2HexString(trandata);
            byte[] postdata;
            var url = Helper.MakeRpcUrlPost(api, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(strtrandata));
            var result = Helper.HttpPost(url, postdata);
            return result;
        }
    }
}
