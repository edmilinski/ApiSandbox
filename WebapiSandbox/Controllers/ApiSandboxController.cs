using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace WebapiSandbox.Controllers
{
  public class ApiSandboxController : ApiController
  {
    private static Dictionary<string, Dictionary<string, string>> _collections = new Dictionary<string, Dictionary<string, string>>();
    private Dictionary<string, string> GetCollection(string name)
    {
      if (!_collections.ContainsKey(name))
        _collections.Add(name, new Dictionary<string, string>());

      return _collections[name];
    }

    private void setItem(string collection, string id, string value)
    {
      Dictionary<string, string> coll = GetCollection(collection);

      if (coll.ContainsKey(id))
        coll[id] = value;
      else
        coll.Add(id, value);
    }

    private string getItem(string collection, string id)
    {
      Dictionary<string, string> coll = GetCollection(collection);
      return coll[id]; 
    }

    private bool deleteItem(string collection, string id)
    {
      Dictionary<string, string> coll = GetCollection(collection);
      if (coll.ContainsKey(id))
      {
        coll.Remove(id);
        return true;
      }
      else
        return false;
    }

    [Route("ApiSandbox/{collection}")]
    public IEnumerable<string> Get(string collection)
    {
      Dictionary<string, string> coll = GetCollection(collection);
      return coll.Values;
    }

    [Route("ApiSandbox/{collection}/{id}")]
    public string Get(string collection, string id)
    {
      return getItem(collection, id);
    }

    [Route("ApiSandbox/{collection}")]
    public HttpResponseMessage Post(string collection, [FromBody]Newtonsoft.Json.Linq.JToken jsonbody)
    {
      if(jsonbody["id"] == null)
      {
        long newId = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
        jsonbody["id"] = newId.ToString();
      }

      string id = jsonbody["id"].ToString();
      setItem(collection, id, jsonbody.ToString());

      return new HttpResponseMessage(HttpStatusCode.Created);
    }

    [Route("ApiSandbox/{collection}/{id}")]
    public void Put(string collection, string id, [FromBody]Newtonsoft.Json.Linq.JToken jsonbody)
    {
      jsonbody["id"] = id;
      setItem(collection, id, jsonbody.ToString());
    }

    [Route("ApiSandbox/{collection}/{id}")]
    public void Delete(string collection, string id)
    {
      deleteItem(collection, id);
    }

    [HttpGet]
    [Route("ApiSandbox/{collection}/save/{name}")]
    public void SaveCollection(string collection, string name)
    {
    }

    [HttpGet]
    [Route("ApiSandbox/{collection}/load/{name}")]
    public void LoadCollection(string collection, string name)
    {
    }

    [HttpPost]
    [Route("ApiSandbox/{collection}/bulkload")]
    public void BulkLoad(string collection, [FromBody]Newtonsoft.Json.Linq.JToken jsonbody)
    {
    }
    
  }
}
