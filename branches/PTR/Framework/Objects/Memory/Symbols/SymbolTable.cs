using System;
using System.Collections.Generic;
using Zeta.Game;

namespace Trinity.Framework.Objects.Memory.Symbols
{
    public class SymbolTable : MemoryWrapper
    {
        // Starts on FF FF FF FF, then ID long, then items @ 8bytes each

        public IntPtr NextTableAddress;
        public List<Symbol> Symbols;

        public string Name;
        public int Index;

        public int Id => ReadOffset<int>(0x4);

        protected override void OnUpdated()
        {
            var symbols = new List<Symbol>();
            var address = BaseAddress;
            var symbol = Create<Symbol>(address);

            while (symbol != null)
            {
                //var nextValueAddress = address + Symbol.SizeOf;
                //var nextValueTest = Read<long>(nextValueAddress);

                symbols.Add(symbol);

                //if (nextValueTest == 0) // End of enum.
                //{
                //    NextTableAddress = nextValueAddress;
                //    break;
                //}

                //if (nextValueTest == -1) // End of section.
                //{
                //    var nextTableAddress = nextValueAddress + 8;
                //    if (Read<int>(nextTableAddress) <= 0)
                //        NextTableAddress = nextTableAddress;

                //    break;
                //}

                address += Symbol.SizeOf;

                if (Read<long>(address) == 0)
                {
                    //if(Read<long>(address + 0x4) == -1)
                    //{
                    //    NextTableAddress = address + 16;
                    //}
                    //else
                    //{
                    //    NextTableAddress = address + 8;
                    //}
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