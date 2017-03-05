using System;
using System.Collections.Generic;
using Zeta.Game;

namespace Trinity.Framework.Objects.Memory.Symbols
{
    public class SymbolTable : MemoryWrapper
    {
        public IntPtr NextTableAddress;
        public List<Symbol> Symbols;

        public string Name;
        public int Index;

        protected override void OnUpdated()
        {
            var symbols = new List<Symbol>();
            var address = BaseAddress;
            var symbol = Create<Symbol>(address);

            while (symbol != null)
            {
                symbols.Add(symbol);
                address += Symbol.SizeOf;

                if (Read<long>(address) == 0)
                {
                    NextTableAddress = address + 8;
                    break;
                }
                symbol = Create<Symbol>(address);
            }            
            Symbols = symbols;
        }

        public override string ToString()
        {
            return $"{GetType().Name}, {Name} ({Symbols.Count})";
        }
    }
}