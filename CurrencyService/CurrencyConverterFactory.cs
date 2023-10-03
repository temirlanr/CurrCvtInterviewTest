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

            var invertRate = false;
            CurrencyRate rate;

            try
            {
                rate = _CurrencyRates.FirstOrDefault(r =>
                    r.From.AlphabeticCode == from.AlphabeticCode && r.To.AlphabeticCode == to.AlphabeticCode)
                    ?? throw new ArgumentNullException("Rate not found;");
            }
            catch (ArgumentNullException)
            {
                rate = _CurrencyRates.FirstOrDefault(r =>
                    r.To.AlphabeticCode == from.AlphabeticCode && r.From.AlphabeticCode == to.AlphabeticCode)
                    ?? throw new ArgumentNullException("Inverted rate not found;");

                invertRate = true;
            }

            return new CurrencyConverter(from, to, value => invertRate ? value / rate.Rate : value * rate.Rate);
        }
    }
}
