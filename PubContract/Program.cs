using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using ThinNeo;

namespace PubContract
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please input your wif:");
            var wif = Console.ReadLine();
            PubScDemo(wif);
            Console.ReadKey();
        }

        private static void PubScDemo(string wif)
        {
            string assetid = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";
            string api = "https://api.nel.group/api/testnet";
            byte[] prikey = ThinNeo.Helper_NEO.GetPrivateKeyFromWIF(wif);
            byte[] pubkey = ThinNeo.Helper_NEO.GetPublicKey_FromPrivateKey(prikey);
            string address = ThinNeo.Helper_NEO.GetAddress_FromPublicKey(pubkey);

            Dictionary<string, List<Utxo>> dir = GetBalanceByAddress(api, address);

            //从文件中读取合约脚本
            byte[] script = System.IO.File.ReadAllBytes("TestCoin.avm"); //这里填你的合约所在地址
            Console.WriteLine("合约脚本hash：" + ThinNeo.Helper_NEO.CalcHash160(script)); //合约 hash，也就是 assetId

            byte[] parameter__list = ThinNeo.Helper.HexString2Bytes("0710");  //合约入参类型  例：0610代表（string，[]）参考：http://docs.neo.org/zh-cn/sc/Parameter.html
            byte[] return_type = ThinNeo.Helper.HexString2Bytes("05");  //合约返回值类型 05 代表 ByteArray
            int need_storage = 1; //是否需要使用存储 0false 1true
            int need_nep4 = 0; //是否需要动态调用 0false 2true
            int need_canCharge = 4; //是否支持收款 4true
            using (ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder())
            {
                //倒序插入数据
                sb.EmitPushString("test"); //description
                sb.EmitPushString("xxx@neo.com"); //email
                sb.EmitPushString("test"); //auther
                sb.EmitPushString("1.0");  //version
                sb.EmitPushString("ABC Coin"); //name
                sb.EmitPushNumber(need_storage | need_nep4 | need_canCharge);
                sb.EmitPushBytes(return_type);
                sb.EmitPushBytes(parameter__list);
                sb.EmitPushBytes(script);
                sb.EmitSysCall("Neo.Contract.Create");

                string scriptPublish = ThinNeo.Helper.Bytes2HexString(sb.ToArray());

                //用ivokescript试运行得到 gas 消耗
                var result = HttpGet($"{api}?method=invokescript&id=1&params=[\"{scriptPublish}\"]");
                var consume = (JObject.Parse(result)["result"] as JArray)[0]["gas_consumed"].ToString();
                decimal gas_consumed = decimal.Parse(consume);

                ThinNeo.InvokeTransData extdata = new ThinNeo.InvokeTransData();
                extdata.script = sb.ToArray();
                extdata.gas = Math.Ceiling(gas_consumed - 10);

                //拼装交易体
                ThinNeo.Transaction tran = MakeTran(dir, null, new ThinNeo.Hash256(assetid), extdata.gas);
                tran.version = 1;
                tran.extdata = extdata;
                tran.type = ThinNeo.TransactionType.InvocationTransaction;
                byte[] msg = tran.GetMessage();
                byte[] signdata = ThinNeo.Helper_NEO.Sign(msg, prikey);
                tran.AddWitness(signdata, pubkey, address);
                string txid = tran.GetHash().ToString();
                byte[] data = tran.GetRawData();
                string rawdata = ThinNeo.Helper.Bytes2HexString(data);

                string input = @"{
	            'jsonrpc': '2.0',
                'method': 'sendrawtransaction',
	            'params': ['#'],
	            'id': '1'
                }";
                input = input.Replace("#", rawdata);
                
                result = HttpPost(api, input);

                Console.WriteLine(result.ToString());
            }

        }

        private static Dictionary<string, List<Utxo>> GetBalanceByAddress(string api, string address)
        {
            JObject response = JObject.Parse(HttpGet(api + "?method=getutxo&id=1&params=['" + address + "']"));
            JArray resJA = (JArray)response["result"];
            Dictionary<string, List<Utxo>> _dir = new Dictionary<string, List<Utxo>>();
            
            foreach (JObject j in resJA)
            {
                Utxo utxo = new Utxo(j["addr"].ToString(), new ThinNeo.Hash256(j["txid"].ToString()), j["asset"].ToString(), decimal.Parse(j["value"].ToString()), int.Parse(j["n"].ToString()));
                if (_dir.ContainsKey(j["asset"].ToString()))
                {
                    _dir[j["asset"].ToString()].Add(utxo);
                }
                else
                {
                    List<Utxo> l = new List<Utxo>();
                    l.Add(utxo);
                    _dir[j["asset"].ToString()] = l;
                }

            }
            return _dir;
        }

        public static Transaction MakeTran(Dictionary<string, List<Utxo>> dic_UTXO, string targetAddr, Hash256 assetid, decimal sendCount)
        {
            if (!dic_UTXO.ContainsKey(assetid.ToString()))
                throw new Exception("No Money!");
            List<Utxo> utxos = dic_UTXO[assetid.ToString()];
            var tran = new ThinNeo.Transaction();
            tran.type = ThinNeo.TransactionType.ContractTransaction;
            tran.version = 0;
            tran.extdata = null;
            tran.attributes = new ThinNeo.Attribute[0];
            var scraddr = "";

            utxos.Sort((a, b) =>
            {
                if (a.value > b.value)
                    return 1;
                else if (a.value < b.value)
                    return -1;
                else
                    return 0;
            });

            decimal count = decimal.Zero;
            List<ThinNeo.TransactionInput> list_inputs = new List<TransactionInput>();
            for (int i = 0; i < utxos.Count; i++)
            {
                ThinNeo.TransactionInput input = new TransactionInput();
                input.hash = utxos[i].txid;
                input.index = (ushort)utxos[i].n;
                list_inputs.Add(input);
                count += utxos[i].value;
                scraddr = utxos[i].addr;
                if (count >= sendCount)
                    break;
            }
            tran.inputs = list_inputs.ToArray();
            if (count >= sendCount)
            {
                List<ThinNeo.TransactionOutput> list_outputs = new List<TransactionOutput>();
                if (sendCount > decimal.Zero && targetAddr != null)
                {
                    ThinNeo.TransactionOutput output = new TransactionOutput();
                    output.assetId = assetid;
                    output.value = sendCount;
                    output.toAddress = ThinNeo.Helper_NEO.GetScriptHash_FromAddress(targetAddr);
                    list_outputs.Add(output);
                }

                var change = count - sendCount;
                if (change > decimal.Zero)
                {
                    ThinNeo.TransactionOutput outputchange = new TransactionOutput();
                    outputchange.assetId = assetid;
                    outputchange.toAddress = ThinNeo.Helper_NEO.GetScriptHash_FromAddress(scraddr);
                    outputchange.value = change;
                    list_outputs.Add(outputchange);
                }

                tran.outputs = list_outputs.ToArray();
            }
            else
            {
                throw new Exception("no enough money!");
            }

            return tran;
        }

        public static string HttpGet(string url)
        {
            WebClient wc = new WebClient();
            return wc.DownloadString(url);
        }

        public static string HttpPost(string url, string data)
        {
            HttpWebRequest req = null;
            HttpWebResponse rsp = null;
            Stream reqStream = null;
            req = WebRequest.CreateHttp(new Uri(url));
            req.ContentType = "application/json;charset=utf-8";

            req.Method = "POST";
            //req.Accept = "text/xml,text/javascript";
            req.ContinueTimeout = 10000;

            byte[] postData = Encoding.UTF8.GetBytes(data);
            reqStream = req.GetRequestStream();
            reqStream.Write(postData, 0, postData.Length);
            //reqStream.Dispose();

            rsp = (HttpWebResponse)req.GetResponse();
            string result = GetResponseAsString(rsp);

            return result;
        }

        private static string GetResponseAsString(HttpWebResponse rsp)
        {
            Stream stream = null;
            StreamReader reader = null;

            try
            {
                // 以字符流的方式读取HTTP响应
                stream = rsp.GetResponseStream();
                reader = new StreamReader(stream, Encoding.UTF8);

                return reader.ReadToEnd();
            }
            finally
            {
                // 释放资源
                if (reader != null)
                    reader.Close();
                if (stream != null)
                    stream.Close();

                reader = null;
                stream = null;

            }
        }
    }

    public class Utxo
    {
        //txid[n] 是utxo的属性
        public ThinNeo.Hash256 txid;
        public int n;

        //asset资产、addr 属于谁，value数额，这都是查出来的
        public string addr;
        public string asset;
        public decimal value;
        public Utxo(string _addr, ThinNeo.Hash256 _txid, string _asset, decimal _value, int _n)
        {
            this.addr = _addr;
            this.txid = _txid;
            this.asset = _asset;
            this.value = _value;
            this.n = _n;
        }
    }
}
