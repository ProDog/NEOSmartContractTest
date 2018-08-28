using System;
using Neo.VM;

namespace ContractTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var noparamAVM = System.IO.File.ReadAllBytes("E:\\VSProject\\NeoContractTest\\NeoContracts\\bin\\Debug\\NeoContracts.avm");
            var str = Neo.Helper.ToHexString(noparamAVM);

            Neo.VM.ScriptBuilder sb = new ScriptBuilder();
            sb.EmitPush(12);
            sb.EmitPush(14);
            sb.EmitPush(2);
            sb.Emit(Neo.VM.OpCode.PACK);
            sb.EmitPush("parame");

            var _parames = sb.ToArray();
            var str2 = Neo.Helper.ToHexString(_parames);


            Console.WriteLine("AVM=" + str2 + str);
            Console.ReadLine();
        }
    }
}
