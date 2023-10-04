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
                var currRates = new List<CurrencyRate>(_CurrencyRates);

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

                    currRates.Add(newRate);
                }

                CurrencyRate temp;
                decimal tempRate;

                while(rate == null)
                {
                    temp = currRates.FirstOrDefault(r => r.From.AlphabeticCode == from.AlphabeticCode);

                    if(temp == null)
                    {

                    }
                }
            }

            return new CurrencyConverter(from, to, value => value * rate.Rate);
        }
    }
}
