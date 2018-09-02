using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Web.Http;
using System.Web.Http.Cors;

namespace ApiSandbox
{
  [EnableCors(origins: "*", headers: "*", methods: "*")]

  public class ApiSandboxController : ApiController
  {
    private PetaPoco.Database GetDb()
    {
      return new PetaPoco.Database("ApiSandbox");
    }

    private static Dictionary<string, Dictionary<string, JObject>> _dicAllCollections = new Dictionary<string, Dictionary<string, JObject>>();

    private Dictionary<string, JObject> GetCollection(string name)
    {
      name = name.ToLower();
      if (!_dicAllCollections.ContainsKey(name))
        _dicAllCollections.Add(name, new Dictionary<string, JObject>());
      return _dicAllCollections[name];
    }

    private void setItem(Dictionary<string, JObject> dicCollection, string id, JObject value)
    {
      if (dicCollection.ContainsKey(id))
        dicCollection[id] = value;
      else
        dicCollection.Add(id, value);
    }

    private void setItem(Dictionary<string, JObject> dicCollection, JObject value)
    {
      if (value["id"] == null)
      {
        long newId = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
        value["id"] = newId.ToString();
      }
      string id = value["id"].ToString();
      setItem(dicCollection, id, value);
    }

    private JObject getItem(Dictionary<string, JObject> dicCollection, string id)
    {
      if (dicCollection.ContainsKey(id))
        return dicCollection[id];
      else
        return null;
    }

    private bool deleteItem(Dictionary<string, JObject> dicCollection, string id)
    {
      if (dicCollection.ContainsKey(id))
      {
        dicCollection.Remove(id);
        return true;
      }
      else
        return false;
    }

    [Route("{collection}")]
    public List<JObject> Get(string collection)
    {
      Dictionary<string, JObject> dicCollection = GetCollection(collection);
      return dicCollection.Values.ToList();
    }

    [Route("{collection}/{id}")]
    public JObject Get(string collection, string id)
    {
      return getItem(GetCollection(collection), id);
    }

    [Route("{collection}")]
    public HttpResponseMessage Post(string collection, [FromBody]JObject jsonbody)
    {
      if (jsonbody["id"] == null)
      {
        long newId = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
        jsonbody["id"] = newId.ToString();
      }
      string id = jsonbody["id"].ToString();
      setItem(GetCollection(collection), id, jsonbody);
      return new HttpResponseMessage(HttpStatusCode.Created);
    }

    [Route("{collection}/{id}")]
    public void Put(string collection, string id, [FromBody]JObject jsonbody)
    {
      jsonbody["id"] = id;
      setItem(GetCollection(collection), id, jsonbody);
    }

    [Route("{collection}/{id}")]
    public void Delete(string collection, string id)
    {
      deleteItem(GetCollection(collection), id);
    }

    [HttpPost]
    [Route("bulkload/{collection}")]
    public void BulkLoad(string collection, [FromBody]JObject[] jsonbody)
    {
      Dictionary<string, JObject> dicCollection = GetCollection(collection);
      dicCollection.Clear();

      foreach (JObject item in jsonbody)
        setItem(dicCollection, item);
    }

    [HttpGet]
    [Route("listloaded")]
    public List<string> GetCollectionNames()
    {
      return _dicAllCollections.Select(x => x.Key).ToList();
    }

    [HttpGet]
    [Route("listdb")]
    public List<ApiSandbox.Collection> GetDbCollectionInfo()
    {
      using (var db = GetDb())
      {
        return db.Fetch<ApiSandbox.Collection>("SELECT Id,name,groupname,modified FROM Collections");
      }
    }

    [HttpGet]
    [Route("savegroup/{group_name}")]
    public void dbcollection_group_save(string group_name)
    {
      dbcollection_group_delete(group_name);

      foreach ( var dicColl in _dicAllCollections)
      {
        dbcollection_save(dicColl.Key, dicColl.Key, group_name);
      }
    }

    [HttpGet]
    [Route("loadgroup/{group_name}")]
    public int dbcollection_group_load(string group_name)
    {
      using (var db = GetDb())
      {
        List<ApiSandbox.Collection> dbCollections = db.Fetch<ApiSandbox.Collection>("SELECT * FROM Collections WHERE GroupName=@0", group_name);
        foreach (ApiSandbox.Collection item in dbCollections)
        {
          CollectionLoad(item.Name, item.Value);
        }

        return dbCollections.Count;
      }
    }

    [HttpGet]
    [Route("deletegroup/{group_name}")]
    public void dbcollection_group_delete(string group_name)
    {
      using (var db = GetDb())
      {
        db.Execute("DELETE FROM Collections WHERE GroupName = @0", group_name);
      }
    }

    [HttpGet]
    [Route("delete/{name}")]
    public int dbcollection_delete(string name)
    {
      using (var db = GetDb())
      {
        return db.Execute("DELETE FROM Collections WHERE Name = @0 AND GroupName IS NULL", name);
      }
    }

    [HttpGet]
    [Route("save/{coll_name}/{target_name}")]
    public void dbcollection_save(string coll_name, string target_name, string group_name = null)
    {
      StringBuilder sb = new StringBuilder("[");
      Dictionary<string, JObject> coll = GetCollection(coll_name);
      foreach (JObject item in coll.Values)
      {
        sb.Append(item.ToString() + ",");
      }
      sb.Append("]");
      using (var db = GetDb())
      {
        Collection existingRecord;
        if(group_name != null)
          existingRecord = db.SingleOrDefault<ApiSandbox.Collection>("WHERE Name = @0 AND GroupName = @1", target_name, group_name);
        else
          existingRecord = db.SingleOrDefault<ApiSandbox.Collection>("WHERE Name = @0 AND GroupName IS NULL", target_name);

        if (existingRecord != null)
          db.Update(new ApiSandbox.Collection { Id = existingRecord.Id, GroupName = group_name, Name = target_name, Value = sb.ToString(), Modified = DateTime.Now });
        else
          db.Insert(new ApiSandbox.Collection { Name = target_name, GroupName = group_name, Value = sb.ToString(), Modified=DateTime.Now });
      }
    }

    [HttpGet]
    [Route("load/{db_Coll_Id}/{coll_name}")]
    public int dbcollection_load(int db_Coll_Id, string coll_name)
    {
      Dictionary<string, JObject> dicColl = GetCollection(coll_name);
      dicColl.Clear();

      using (var db = GetDb())
      {
        var dbColl = db.SingleOrDefault<ApiSandbox.Collection>("SELECT * FROM Collections WHERE Id=@0", db_Coll_Id);

        if (dbColl != null)
        {
          int itemCount = CollectionLoad(coll_name, dbColl.Value);
          return itemCount;
        }
      }

      return 0;
    }

    private int CollectionLoad(string coll_name, string value)
    {
      Dictionary<string, JObject> dicColl = GetCollection(coll_name);
      dicColl.Clear();

      int itemCount = 0;

      JArray arr = JArray.Parse(value);
      foreach (JObject item in arr)
      {
        itemCount++;
        var id = item["id"];
        if (id != null)
        {
          string sId = id.ToString();
          if (!dicColl.ContainsKey(sId))
            dicColl.Add(sId, item);
        }
      }
      return itemCount;
    }
  }
}
