using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyService
{
    public class CurrencyConverterFactory
    {
        List<Currency> _CurrencyCodes;
        List<CurrencyRate> _CurrencyRates;

        public CurrencyConverterFactory(IEnumerable<Currency> currency, IEnumerable<CurrencyRate> currencyRates)
        {
            _CurrencyCodes = currency.ToList();
            _CurrencyRates = currencyRates.ToList();

            foreach (var r in _CurrencyRates)
            {
                r.From = _CurrencyCodes.First(c => c.AlphabeticCode == r.FromAlfa3);
                r.To = _CurrencyCodes.First(c => c.AlphabeticCode == r.ToAlfa3);
            }
        }

        public CurrencyConverter GetConverter(Currency from, Currency to)
        {

            if (from.AlphabeticCode == to.AlphabeticCode)
                return new CurrencyConverter(from, to, value => 1.0m);

            var rate = _CurrencyRates.FirstOrDefault(r =>
                    r.From.AlphabeticCode == from.AlphabeticCode && r.To.AlphabeticCode == to.AlphabeticCode);

            if(rate == null)
            {
                rate = _CurrencyRates.FirstOrDefault(r =>
                    r.To.AlphabeticCode == from.AlphabeticCode && r.From.AlphabeticCode == to.AlphabeticCode);

                if(rate != null)
                {
                    return new CurrencyConverter(from, to, value => value / rate.Rate);
                }

                var currencyRates = new List<CurrencyRate>(_CurrencyRates);

                foreach(var r in _CurrencyRates)
                {
                    var newRate = new CurrencyRate()
                    {
                        Ticker = r.Ticker + "_INV",
                        Rate = 1 / r.Rate,
                        From = r.To,
                        To = r.From,
                        FromAlfa3 = r.ToAlfa3,
                        ToAlfa3 = r.FromAlfa3
                    };

                    currencyRates.Add(newRate);
                }

                decimal resRate = 1.0m;
                var visited = new HashSet<string>();
                var stack = new Stack<string>();
                stack.Push(from.AlphabeticCode);

                while(stack.Count > 0)
                {
                    var curr = stack.Pop();

                    if (visited.Contains(curr)) 
                        continue;

                    visited.Add(curr);

                    // TODO: FIND A WAY TO UPDATE THE RATE USING CURRENT CURRENCY RATE
                    //var rate = currencyRates.First
                    //resRate += 

                    var neighborRates = currencyRates.Where(r => r.From.AlphabeticCode == curr).ToList();

                    if (neighborRates.Count == 0)
                        throw new ArgumentException("Couldn't convert.");

                    var neightborCurrs = neighborRates.Select(r => r.To.AlphabeticCode).Where(s => !visited.Contains(s)).ToList();

                    foreach (var item in neightborCurrs)
                    {
                        stack.Push(item);
                    }
                }

                return new CurrencyConverter(from, to, value => value * resRate);
            }

            return new CurrencyConverter(from, to, value => value * rate.Rate);
        }
    }
}
