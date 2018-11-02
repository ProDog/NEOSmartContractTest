using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Neo.IO.Json;
using ThinNeo;

namespace ContractTest
{
    class Program
    {
        public const string api = "https://api.nel.group/api/testnet";
        public const string id_GAS = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";
        static void Main(string[] args)
        {
            //Console.WriteLine("wif:");
            //est_DeployBCT();
            //Test_DeployBCP();
            //Test_DeployBCPWithoutUtxo();
            //TransferBCWithoutUtxo();
            GetBalanceOf();
            
            //TransferBCT();
            //TransferBCP();

            //NeoBankDeposit();
            //NeoBankBalanceOf();
            //NeoBankSetCanBack();
            //NeoGetMoneyBack();

            Console.ReadKey();
        }

        private static void NeoGetMoneyBack()
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
                array.AddArrayValue("(addr)" + "AWN6jngST5ytpNnY1dhBQG7QHd7V8SqSCp");
                array.AddArrayValue("(int)" + "31259" + "05385562");
                sb.EmitParamJson(array);//参数倒序入
                sb.EmitPushString("getmoneyback");//参数倒序入
                sb.EmitAppCall(new Hash160("78f0ffad20d31ee1dd9d77d598d42bad4f639695"));//nep5脚本
                script = sb.ToArray();
                Console.WriteLine(ThinNeo.Helper.Bytes2HexString(script));
            }
            var result = SendTransaction(prikey, script);
        }

        private static void NeoBankSetCanBack()
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
                array.AddArrayValue("(addr)" + "AWN6jngST5ytpNnY1dhBQG7QHd7V8SqSCp");
                array.AddArrayValue("(int)" + "31259" + "05385562");
                sb.EmitParamJson(array);//参数倒序入
                sb.EmitPushString("setcanback");//参数倒序入
                sb.EmitAppCall(new Hash160("78f0ffad20d31ee1dd9d77d598d42bad4f639695"));//nep5脚本
                script = sb.ToArray();
                Console.WriteLine(ThinNeo.Helper.Bytes2HexString(script));
            }
            var result = SendTransaction(prikey, script);
        }

        private static void NeoBankBalanceOf()
        {
            var wif = Console.ReadLine();
            var prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
            var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            var address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            var scriptHash = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);

            byte[] data = null;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                MyJson.JsonNode_Array array = new MyJson.JsonNode_Array();
                array.AddArrayValue("(addr)" + "AWN6jngST5ytpNnY1dhBQG7QHd7V8SqSCp");
                sb.EmitParamJson(array);
                sb.EmitPushString("balanceOf");
                sb.EmitAppCall(new Hash160("78f0ffad20d31ee1dd9d77d598d42bad4f639695"));//合约脚本hash
                data = sb.ToArray();
            }

            string script = ThinNeo.Helper.Bytes2HexString(data);
            byte[] postdata;
            var url = Helper.MakeRpcUrlPost("https://api.nel.group/api/testnet", "invokescript", out postdata, new MyJson.JsonNode_ValueString(script));
            var result = Helper.HttpPost(url, postdata);
            var aa = MyJson.Parse(result).AsDict();
            byte[] balance = ThinNeo.Helper.HexString2Bytes(aa["result"].AsList()[0].AsDict()["stack"].AsList()[0].AsDict()["value"].ToString());
            var ba = new BigInteger(balance);
            Console.WriteLine(result);
            Console.WriteLine(ba);
        }

        private static void NeoBankDeposit()
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
                array.AddArrayValue("(hex256)" + "0xcf7880b42e9f1858d7dab05bdd8eea2262535ea3864f3bdbb4d8ed3794dd2c0e");
                sb.EmitParamJson(array);//参数倒序入
                sb.EmitPushString("deposit");//参数倒序入
                sb.EmitAppCall(new Hash160("18d0bec5726b43595376a469c029e221a22292c9"));//nep5脚本
                script = sb.ToArray();
                Console.WriteLine(ThinNeo.Helper.Bytes2HexString(script));
            }
            var result = SendTransaction(prikey, script);
        }

        private static void TransferBCT()
        {
            var wif = Console.ReadLine();
            Console.WriteLine("targetAddress:");
            var targetAddress = Console.ReadLine();

            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            byte[] script = null;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                var array = new MyJson.JsonNode_Array();
                array.AddArrayValue("(addr)" + address);//from
                array.AddArrayValue("(addr)" + targetAddress);//to
                array.AddArrayValue("(int)" + "800" + "8538");//value
                sb.EmitParamJson(array);//参数倒序入
                sb.EmitPushString("transfer");//参数倒序入
                sb.EmitAppCall(new Hash160("025761aea3bb99baaec1a6a53744992c8e6b0681"));
                script = sb.ToArray();
            }

            //获取自己的utxo
            Dictionary<string, List<Utxo>> dir = Helper.GetBalanceByAddress(api, address);
            Transaction tran = Helper.makeTran(dir[id_GAS], null, new ThinNeo.Hash256(id_GAS), 0);
            tran.type = ThinNeo.TransactionType.InvocationTransaction;
            //tran.version = 0;
            //tran.attributes = new ThinNeo.Attribute[0];
            var idata = new ThinNeo.InvokeTransData();
            tran.extdata = idata;
            idata.script = script;
            idata.gas = 1;

            byte[] msg = tran.GetMessage();
            string msgstr = ThinNeo.Helper.Bytes2HexString(msg);
            byte[] signdata = ThinNeo.Helper.Sign(msg, prikey);
            tran.AddWitness(signdata, pubkey, address);
            string txid = tran.GetHash().ToString();
            byte[] data = tran.GetRawData();
            string rawdata = ThinNeo.Helper.Bytes2HexString(data);

            byte[] postdata;
            var url = Helper.MakeRpcUrlPost("https://api.nel.group/api/testnet", "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(rawdata));
            var result = Helper.HttpPost(url, postdata);
            MyJson.JsonNode_Object resJO = (MyJson.JsonNode_Object)MyJson.Parse(result);
            Console.WriteLine(resJO.ToString());
        }

        private static void TransferBCP()
        {
            var wif = Console.ReadLine();
            Console.WriteLine("targetAddress:");
            var targetAddress = Console.ReadLine();

            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            byte[] script = null;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                var array = new MyJson.JsonNode_Array();
                array.AddArrayValue("(addr)" + address);//from
                array.AddArrayValue("(addr)" + targetAddress);//to
                array.AddArrayValue("(int)" + "5500" + "85848838");//value
                sb.EmitParamJson(array);//参数倒序入
                sb.EmitPushString("transfer");//参数倒序入
                sb.EmitAppCall(new Hash160("04e31cee0443bb916534dad2adf508458920e66d"));
                script = sb.ToArray();
            }

            //获取自己的utxo
            Dictionary<string, List<Utxo>> dir = Helper.GetBalanceByAddress(api, address);
            Transaction tran = Helper.makeTran(dir[id_GAS], address, new ThinNeo.Hash256(id_GAS), 0);
            tran.type = ThinNeo.TransactionType.InvocationTransaction;
            //tran.version = 0;
            //tran.attributes = new ThinNeo.Attribute[0];
            var idata = new ThinNeo.InvokeTransData();
            tran.extdata = idata;
            idata.script = script;
            idata.gas = 1;

            byte[] msg = tran.GetMessage();
            string msgstr = ThinNeo.Helper.Bytes2HexString(msg);
            byte[] signdata = ThinNeo.Helper.Sign(msg, prikey);
            tran.AddWitness(signdata, pubkey, address);
            string txid = tran.GetHash().ToString();
            byte[] data = tran.GetRawData();
            string rawdata = ThinNeo.Helper.Bytes2HexString(data);

            byte[] postdata;
            var url = Helper.MakeRpcUrlPost("https://api.nel.group/api/testnet", "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(rawdata));
            var result = Helper.HttpPost(url, postdata);
            MyJson.JsonNode_Object resJO = (MyJson.JsonNode_Object)MyJson.Parse(result);
            Console.WriteLine(resJO.ToString());
        }

        private static void TransferBCWithoutUtxo()
        {
            var wif = Console.ReadLine();
            Console.WriteLine("targetAddress:");
            var targetAddress = Console.ReadLine();

            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            byte[] script = null;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                var array = new MyJson.JsonNode_Array();
                array.AddArrayValue("(addr)" + address); //from
                array.AddArrayValue("(addr)" + targetAddress); //to
                array.AddArrayValue("(int)" + "100" + "00000000"); //value
                sb.EmitParamJson(array); //参数倒序入
                sb.EmitPushString("transfer"); //参数倒序入
                sb.EmitAppCall(new Hash160("93ae7faf78f67c9753e768a3c6ffcfd974ea1c6f"));
                script = sb.ToArray();
            }
            SendTransWithoutUtxo(prikey, pubkey, address, script);
        }

        private static void Test_DeployBCP()
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
                array.AddArrayValue("(addr)" + "AGNrEJ7iamdmiXHMf28xtifVBiKcjjrHpS");
                sb.EmitParamJson(array);//参数倒序入
                sb.EmitPushString("deploy");//参数倒序入
                sb.EmitAppCall(new Hash160("93ae7faf78f67c9753e768a3c6ffcfd974ea1c6f"));//nep5脚本
                script = sb.ToArray();
                Console.WriteLine(ThinNeo.Helper.Bytes2HexString(script));
            }
            var result = SendTransaction(prikey, script);
            Console.WriteLine(result);
        }

        private static void Test_DeployBCPWithoutUtxo()
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
                array.AddArrayValue("(addr)" + "AGNrEJ7iamdmiXHMf28xtifVBiKcjjrHpS");
                array.AddArrayValue("(int)" + "100" + "00000000"); //value
                sb.EmitParamJson(array);//参数倒序入
                sb.EmitPushString("deploy");//参数倒序入
                sb.EmitAppCall(new Hash160("07bc2c1398e1a472f3841a00e7e7e02029b8b38b"));//nep5脚本
                script = sb.ToArray();
                Console.WriteLine(ThinNeo.Helper.Bytes2HexString(script));
            }
            SendTransWithoutUtxo(prikey, pubkey, address, script);
        }

        private static void SendTransWithoutUtxo(byte[] prikey, byte[] pubkey, string address, byte[] script)
        {
            ThinNeo.Transaction tran = new Transaction();
            tran.inputs = new ThinNeo.TransactionInput[0];
            tran.outputs = new TransactionOutput[0];
            tran.attributes = new ThinNeo.Attribute[1];
            tran.attributes[0] = new ThinNeo.Attribute();
            tran.attributes[0].usage = TransactionAttributeUsage.Script;
            tran.attributes[0].data = ThinNeo.Helper.GetPublicKeyHashFromAddress(address);
            tran.version = 1;
            tran.type = ThinNeo.TransactionType.InvocationTransaction;

            var idata = new ThinNeo.InvokeTransData();
            tran.extdata = idata;
            idata.script = script;
            idata.gas = 0;

            byte[] msg = tran.GetMessage();
            string msgstr = ThinNeo.Helper.Bytes2HexString(msg);
            byte[] signdata = ThinNeo.Helper.Sign(msg, prikey);
            tran.AddWitness(signdata, pubkey, address);
            string txid = tran.GetHash().ToString();
            byte[] data = tran.GetRawData();
            string rawdata = ThinNeo.Helper.Bytes2HexString(data);

            byte[] postdata;
            var url = Helper.MakeRpcUrlPost(api, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(rawdata));
            var result = Helper.HttpPost(url, postdata);
            Console.WriteLine(txid);
            MyJson.JsonNode_Object resJO = (MyJson.JsonNode_Object)MyJson.Parse(result);
            Console.WriteLine(resJO.ToString());
        }

        private static void Test_DeployBCT()
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
                sb.EmitAppCall(new Hash160("04e31cee0443bb916534dad2adf508458920e66d"));//nep5脚本
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

        /// <summary>
        /// 调用合约中的balanceOf方法、
        /// </summary>
        /// <param name="address"></param>
        private static void GetBalanceOf()
        {
            decimal dd = (decimal)1100000 * (decimal)60000.0000000;
           byte[] data = null;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                MyJson.JsonNode_Array array = new MyJson.JsonNode_Array();
                array.AddArrayValue("(addr)" + "AbN2K2trYzgx8WMg2H7U7JHH6RQVzz2fnx");
                sb.EmitParamJson(array);
                sb.EmitPushString("balanceOf");
                sb.EmitAppCall(new Hash160("04e31cee0443bb916534dad2adf508458920e66d"));//合约脚本hash
                data = sb.ToArray();
            }

            string script = ThinNeo.Helper.Bytes2HexString(data);
            byte[] postdata;
            var url = Helper.MakeRpcUrlPost("https://api.nel.group/api/testnet", "invokescript", out postdata, new MyJson.JsonNode_ValueString(script));
            var result = Helper.HttpPost(url, postdata);
            var res = Newtonsoft.Json.Linq.JObject.Parse(result)["result"] as Newtonsoft.Json.Linq.JArray;
            var stack = (res[0]["stack"] as Newtonsoft.Json.Linq.JArray)[0] as Newtonsoft.Json.Linq.JObject;

            var balance = ThinNeo.Helper.HexString2Bytes((string) stack["value"]);
            var bb = new BigInteger(balance);
            decimal cc = (decimal)bb / (decimal)100000000.00000000;
            Console.WriteLine(result);
            Console.WriteLine(balance);
        }
    }
}
