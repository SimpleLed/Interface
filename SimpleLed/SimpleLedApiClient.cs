using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SimpleLed
{
    public class SimpleLedApiClient
    {
        public static string BaseUrl = "http://api.simpleled.net/";
        public async Task<List<DriverProperties>> GetProducts() => await GetAsync<List<DriverProperties>>(BaseUrl + "Product/GetProducts");

        public async Task<List<DriverProperties>> GetProductsByCategory(ProductCategory category)
        {
            string url = BaseUrl + "Product/GetProductsByCategory?category=" + ((int) category);
            var result= await GetAsync<List<DriverProperties>>(url);

            return result.OrderBy(x=>x.ProductId).ThenByDescending(x=>x.CurrentVersion).ThenBy(x=>x.AuthorId).ToList();
        }

        public async Task<byte[]> GetProduct(Guid productId)
        {
            using (var client = new HttpClient())
            {
                return await client.GetByteArrayAsync(BaseUrl + "Product/GetBinary/" + productId);
            }
        }


        public async Task<byte[]> GetProduct(Guid productId, ReleaseNumber release)
        {
            using (var client = new HttpClient())
            {
                return await client.GetByteArrayAsync(BaseUrl + "Product/GetBinary/" + productId+"?version="+release);
            }
        }



        private static async Task<T> PostAsync<T>(string url, string model)
        {
            using (var client = new HttpClient())
            {
                var data = new StringContent(model, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, data).ConfigureAwait(false);
                string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<T>(result);
            }
        }

        private static async Task<T> GetAsync<T>(string url)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url).ConfigureAwait(false);
                string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<T>(result);
            }
        }

        private static void Put<T>(string url, T model)
        {
            using (var client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(model);
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                var response = client.PutAsync(url, data).Result;
            }
        }

        private static void Put(string url)
        {
            using (var client = new HttpClient())
            {
                var response = client.PutAsync(url, null).Result;
            }
        }
    }
}
