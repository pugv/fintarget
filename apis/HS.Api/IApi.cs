using System.Collections.Generic;
using System.Threading.Tasks;
using ExchangeHelpers.HS;

namespace HS.ApiClient
{
    public interface IApi
    {
        Task<decimal> GetExchangeRate(string curIn, string curOut);

        Task<SecurityRecord[]> GetSecurities(IEnumerable<string> securityKeys);
        
        Task<SecurityRecord[]> GetAllSecurities();
    }
}