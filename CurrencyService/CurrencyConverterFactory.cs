using System.Collections.Generic;
using System.Linq;

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
            // same currency conversion
            if (from.AlphabeticCode == to.AlphabeticCode)
                return new CurrencyConverter(from, to, value => 1.0m);

            // try to find rate for straightforward conversion
            var rate = _CurrencyRates.FirstOrDefault(r =>
                    r.From.AlphabeticCode == from.AlphabeticCode && r.To.AlphabeticCode == to.AlphabeticCode);
            
            // in case straighforward conversion is not possible
            if(rate == null)
            {
                // cross conversion
                rate = _CurrencyRates.FirstOrDefault(r =>
                    r.To.AlphabeticCode == from.AlphabeticCode && r.From.AlphabeticCode == to.AlphabeticCode);

                if(rate != null)
                {
                    return new CurrencyConverter(from, to, value => value / rate.Rate);
                }

                // make currency rates go both sides
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

                // preparing for depth first search
                decimal resRate = 1.0m;
                var visited = new List<string>();
                var stack = new Stack<CurrencyRate>();
                var fromRate = currencyRates.First(r => r.From.AlphabeticCode == from.AlphabeticCode);
                stack.Push(fromRate);

                // DFS
                while(stack.Count > 0)
                {
                    var curr = stack.Pop();

                    // dont go back to root!
                    if (curr.To.AlphabeticCode == from.AlphabeticCode)
                        continue;

                    // check if it was visited before
                    if (visited.Contains(curr.Ticker)) 
                        continue;

                    visited.Add(curr.Ticker);
                    // (need to figure out how to stop from going back using inverted currency rate...)
                    var neighborRates = currencyRates.Where(r => r.From.AlphabeticCode == curr.To.AlphabeticCode 
                                                                    && !visited.Contains(r.Ticker)).ToList();

                    // no neighbors means it is an isolated vertex (or edge..? it is a currency rate with 2 links and logically it should be edge, but i use it as a vertex)
                    if (neighborRates.Count == 0)
                        continue;

                    // update rate (i think this is the reason why i got a little deviation in some test results, but maybe its not)
                    resRate *= curr.Rate;

                    // we reached the target!!! CUT
                    if(curr.To.AlphabeticCode == to.AlphabeticCode)
                        return new CurrencyConverter(from, to, value => value * resRate);

                    // add neighbors to stack to use them later
                    foreach (var item in neighborRates)
                    {
                        stack.Push(item);
                    }
                }
            }

            return new CurrencyConverter(from, to, value => value * rate.Rate);
        }
    }
}
