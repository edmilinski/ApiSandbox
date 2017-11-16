using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace WebapiSandbox.Controllers
{
  public class ApiSandboxController : ApiController
  {
    private PetaPoco.Database GetDb()
    {
      return new PetaPoco.Database("ApiSandbox");
    }

    private static Dictionary<string, Dictionary<string, JObject>> _collections = new Dictionary<string, Dictionary<string, JObject>>();
    private Dictionary<string, JObject> GetCollection(string name)
    {
      name = name.ToLower();

      if (!_collections.ContainsKey(name))
        _collections.Add(name, new Dictionary<string, JObject>());

      return _collections[name];
    }

    private void setItem(string collection, string id, JObject value)
    {
      Dictionary<string, JObject> coll = GetCollection(collection);

      if (coll.ContainsKey(id))
        coll[id] = value;
      else
        coll.Add(id, value);
    }

    private void setItem(string collection, JObject value)
    {
      if (value["id"] == null)
      {
        long newId = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
        value["id"] = newId.ToString();
      }
      string id = value["id"].ToString();

      setItem(collection, id, value);
    }

    private JObject getItem(string collection, string id)
    {
      Dictionary<string, JObject> coll = GetCollection(collection);
      
      if (coll.ContainsKey(id))
        return coll[id];
      else
        return null;
    }

    private bool deleteItem(string collection, string id)
    {
      Dictionary<string, JObject> coll = GetCollection(collection);
      if (coll.ContainsKey(id))
      {
        coll.Remove(id);
        return true;
      }
      else
        return false;
    }

    [Route("ApiSandbox/{collection}")]
    public List<JObject> Get(string collection)
    {
      Dictionary<string, JObject> coll = GetCollection(collection);
      return coll.Values.ToList();
    }

    [Route("ApiSandbox/{collection}/{id}")]
    public JObject Get(string collection, string id)
    {
      return getItem(collection, id);
    }

    [Route("ApiSandbox/{collection}")]
    public HttpResponseMessage Post(string collection, [FromBody]JObject jsonbody)
    {
      if(jsonbody["id"] == null)
      {
        long newId = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
        jsonbody["id"] = newId.ToString();
      }

      string id = jsonbody["id"].ToString();
      setItem(collection, id, jsonbody);

      return new HttpResponseMessage(HttpStatusCode.Created);
    }

    [Route("ApiSandbox/{collection}/{id}")]
    public void Put(string collection, string id, [FromBody]JObject jsonbody)
    {
      jsonbody["id"] = id;
      setItem(collection, id, jsonbody);
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
      StringBuilder sb = new StringBuilder("[");

      Dictionary<string, JObject> coll =  GetCollection(name);
      foreach (JObject item in coll.Values)
      {
        sb.Append(item.ToString()+",");
      }
      sb.Append("]");

      using (var db = GetDb())
      {
        var existingRecord = db.SingleOrDefault<ApiSandbox.Collection>("WHERE Name = @0", name);

        if (existingRecord != null)
          db.Update(new ApiSandbox.Collection { Id = existingRecord.Id, Name = name, Value = sb.ToString() });
        else
          db.Insert(new ApiSandbox.Collection { Name = name, Value = sb.ToString() });
      }
    }

    [HttpGet]
    [Route("ApiSandbox/{collection}/load/{name}")]
    public void LoadCollection(string collection, string name)
    {
      Dictionary<string, JObject> coll = GetCollection(name);
      coll.Clear();

      using (var db = GetDb())
      {
        var res = db.Single<ApiSandbox.Collection>("SELECT * FROM Collections WHERE Name=@0", name);
        JArray arr = JArray.Parse(res.Value);
        foreach(JObject item in arr)
        {
          var id = item["id"];
          if(id != null)
          {
            string sId = id.ToString();
            if(! coll.ContainsKey(sId))
              coll.Add(sId, item);
          }
        }
      }
    }

    [HttpPost]
    [Route("ApiSandbox/{collection}/bulkload")]
    public void BulkLoad(string collection, [FromBody]JObject[] jsonbody)
    {
      foreach (JObject item in jsonbody)
        setItem(collection, item);
    }
    
  }
}
