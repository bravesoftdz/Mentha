using Mentha.Code;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mentha.Banks {
    public abstract class BaseBank {
        public abstract string Id { get; }

        public abstract Task<List<Account>> GetAccountsAsync(Profile profile);

        public async Task<string> DeleteAsync(HttpClient HC, string url) {
            var Response = await HC.DeleteAsync(url);
            if (Response.IsSuccessStatusCode) {
                return await Response.Content.ReadAsStringAsync();
            } else {
                throw new Exception($"Unexpected response status '{Response.StatusCode}' for DELETE '{url}'");
            }
        }

        public async Task<string> GetAsync(HttpClient HC, string url) {
            var Response = await HC.GetAsync(url);
            if (Response.IsSuccessStatusCode) {
                return await Response.Content.ReadAsStringAsync();
            } else {
                throw new Exception($"Unexpected response status '{Response.StatusCode}' for GET '{url}'");
            }
        }

        public async Task<string> PostAsync(HttpClient HC, string url, HttpContent postData) {
            var Response = await HC.PostAsync(url, postData);
            if (Response.IsSuccessStatusCode) {
                return await Response.Content.ReadAsStringAsync();
            } else {
                throw new Exception($"Unexpected response status '{Response.StatusCode}' for POST to '{url}'");
            }
        }
    }
}
